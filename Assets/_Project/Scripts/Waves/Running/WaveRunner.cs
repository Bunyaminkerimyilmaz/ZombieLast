using System;
using System.Collections.Generic;
using LastHaven.Core;

namespace LastHaven.Waves
{
    /// <summary>
    /// Bir gecenin dalga mantığı (saf C#, GDD §8). Sadece <see cref="Tick"/> ile ilerler; Unity'ye dokunmaz.
    /// <list type="bullet">
    /// <item>Her grup gece başından <see cref="WaveGroup.StartDelay"/> sonra, <see cref="WaveGroup.SpawnInterval"/> aralıkla
    /// spawn üretir; yönler sırayla dağıtılır. Uzun bir karede birikmiş spawn'ların hepsi o karede üretilir.</item>
    /// <item>Üretilen istek kuyruğa girer. Canlı düşman tavanın altındaysa kuyruktan sırayla (FIFO) spawn callback'ine verilir.
    /// Tavan doluysa gruplar üretmeye devam eder, istekler kuyrukta bekler.</item>
    /// <item>Callback false dönerse (doğurulamadı) düşman geceden düşülür; gece takılmaz.</item>
    /// <item>Gece biter: tüm gruplar üretmiş, kuyruk boş ve canlı düşman 0. Bitiş sadece <see cref="Tick"/>'te kontrol edilir.</item>
    /// </list>
    /// </summary>
    public sealed class WaveRunner
    {
        private struct GroupState
        {
            public WaveGroup Group;
            public int Emitted;
        }

        private readonly Func<WaveSpawnRequest, bool> _spawn;
        private readonly Queue<WaveSpawnRequest> _queue = new Queue<WaveSpawnRequest>();
        private GroupState[] _groups = Array.Empty<GroupState>();

        /// <param name="spawn">Spawn isteğini uygular; düşman doğduysa true, doğurulamadıysa false döner.</param>
        public WaveRunner(Func<WaveSpawnRequest, bool> spawn)
        {
            _spawn = spawn ?? throw new ArgumentNullException(nameof(spawn));
        }

        /// <summary><see cref="RemainingCount"/> azaldığında: dalga düşmanı öldü veya doğurulamayan düşman düşüldü.</summary>
        public event Action RemainingChanged;

        /// <summary>Gece bittiğinde bir kez (<see cref="Tick"/> içinde).</summary>
        public event Action Completed;

        public bool IsRunning { get; private set; }

        /// <summary>Gece başından beri geçen süre (sn).</summary>
        public float Elapsed { get; private set; }

        /// <summary>Aynı anda en fazla canlı düşman (en az 1).</summary>
        public int MaxAlive { get; private set; }

        /// <summary>Gecenin toplam düşmanı.</summary>
        public int TotalCount { get; private set; }

        /// <summary>Doğmuş ve henüz ölmemiş düşmanlar.</summary>
        public int AliveCount { get; private set; }

        /// <summary>Zamanı gelmiş ama tavan yüzünden doğmayı bekleyen istekler.</summary>
        public int QueuedCount => _queue.Count;

        /// <summary>Zamanı henüz gelmemiş spawn'lar.</summary>
        public int PendingCount { get; private set; }

        /// <summary>Doğurulamayıp düşülen düşmanlar.</summary>
        public int DroppedCount { get; private set; }

        /// <summary>Kalan düşman: bekleyen + kuyrukta + canlı.</summary>
        public int RemainingCount => PendingCount + _queue.Count + AliveCount;

        /// <summary>
        /// Geceyi baştan başlatır (önceki durum silinir). İlk spawn'lar ilk <see cref="Tick"/>'te üretilir.
        /// </summary>
        /// <param name="maxAlive">Canlı düşman tavanı; 1'den küçükse 1 sayılır.</param>
        /// <exception cref="ArgumentException">Adedi olan bir grubun yönü yoksa.</exception>
        public void Start(IReadOnlyList<WaveGroup> groups, int maxAlive)
        {
            if (groups == null)
            {
                throw new ArgumentNullException(nameof(groups));
            }

            var states = new GroupState[groups.Count];
            int total = 0;
            for (int i = 0; i < groups.Count; i++)
            {
                WaveGroup group = groups[i];
                if (group.Count > 0 && group.Directions.Count == 0)
                {
                    throw new ArgumentException($"{i}. grubun yönü yok.", nameof(groups));
                }

                states[i] = new GroupState { Group = group };
                total += group.Count;
            }

            _groups = states;
            _queue.Clear();
            MaxAlive = Math.Max(1, maxAlive);
            TotalCount = total;
            PendingCount = total;
            AliveCount = 0;
            DroppedCount = 0;
            Elapsed = 0f;
            IsRunning = true;
        }

        /// <summary>Zamanı ilerletir: vadesi gelen spawn'ları üretir, kuyruğu tavana kadar boşaltır, bitişi kontrol eder.</summary>
        public void Tick(float deltaTime)
        {
            if (!IsRunning)
            {
                return;
            }

            Elapsed += Math.Max(0f, deltaTime);
            EmitDueSpawns();
            DrainQueue();

            if (PendingCount == 0 && _queue.Count == 0 && AliveCount == 0)
            {
                IsRunning = false;
                Completed?.Invoke();
            }
        }

        /// <summary>
        /// Dalga dışında doğmuş <paramref name="count"/> canlı düşmanı bu geceye katar (gündüz gezginleri, GDD §9): toplam ve canlı
        /// sayısı artar, tavana sayılırlar; ölümleri <see cref="ReportDeath"/> ile bildirilir ve gece onlar da ölmeden bitmez.
        /// Gece sürmüyorsa veya sayı pozitif değilse yok sayılır (false).
        /// </summary>
        public bool Adopt(int count)
        {
            if (!IsRunning || count <= 0)
            {
                return false;
            }

            TotalCount += count;
            AliveCount += count;
            return true;
        }

        /// <summary>Bu gecenin canlı bir düşmanı öldü. Gece sürmüyorsa veya canlı düşman yoksa yok sayılır (false).</summary>
        public bool ReportDeath()
        {
            if (!IsRunning || AliveCount == 0)
            {
                return false;
            }

            AliveCount--;
            RemainingChanged?.Invoke();
            return true;
        }

        private void EmitDueSpawns()
        {
            for (int i = 0; i < _groups.Length; i++)
            {
                ref GroupState state = ref _groups[i];
                WaveGroup group = state.Group;

                // Vade birikerek değil indeksle hesaplanır; float kayması birikmez.
                while (state.Emitted < group.Count && group.StartDelay + state.Emitted * group.SpawnInterval <= Elapsed)
                {
                    CompassDirection direction = group.Directions[state.Emitted % group.Directions.Count];
                    _queue.Enqueue(new WaveSpawnRequest(group.Enemy, direction, i, state.Emitted, group.IsBoss));
                    state.Emitted++;
                    PendingCount--;
                }
            }
        }

        private void DrainQueue()
        {
            while (_queue.Count > 0 && AliveCount < MaxAlive)
            {
                WaveSpawnRequest request = _queue.Dequeue();
                if (_spawn(request))
                {
                    AliveCount++;
                    continue;
                }

                DroppedCount++;
                RemainingChanged?.Invoke();
            }
        }
    }
}
