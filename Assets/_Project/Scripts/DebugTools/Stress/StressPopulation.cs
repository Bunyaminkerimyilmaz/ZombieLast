using System.Collections.Generic;
using LastHaven.Enemies;
using LastHaven.Waves;

namespace LastHaven.DebugTools
{
    /// <summary>
    /// Stres testinin düşmanları: hedef sayıya kadar karede en çok verilen kadar doğurur (karışım <see cref="StressMix"/>), ölenleri
    /// hemen listeden çıkarır (havuza dönen örnek başka biri için yeniden kullanılabilir) ve en yüksek canlı sayısını tutar.
    /// Bellek ayırmaz (liste baştan 256 kapasiteli).
    /// </summary>
    public sealed class StressPopulation
    {
        private readonly List<Enemy> _enemies = new List<Enemy>(256);
        private StressEnemy[] _mix = new StressEnemy[0];
        private StressMix _plan = new StressMix(new int[0]);
        private int _spawnIndex;

        /// <summary>Testin doğurduğu ve hâlâ canlı düşmanlar.</summary>
        public IReadOnlyList<Enemy> Enemies => _enemies;

        public int AliveCount => _enemies.Count;

        public int PeakAlive { get; private set; }

        /// <summary>Yeni test: karışımı kurar, sayaçları sıfırlar.</summary>
        public void Begin(StressEnemy[] mix)
        {
            _mix = mix ?? new StressEnemy[0];
            var weights = new int[_mix.Length];
            for (int i = 0; i < _mix.Length; i++)
            {
                weights[i] = _mix[i].Enemy != null ? _mix[i].Weight : 0;
            }

            _plan = new StressMix(weights);
            _enemies.Clear();
            _spawnIndex = 0;
            PeakAlive = 0;
        }

        /// <summary><paramref name="total"/> düşmanda tipin en çok adedi (havuz ön ısıtması).</summary>
        public int ShareOf(int typeIndex, int total)
        {
            return _plan.Share(typeIndex, total);
        }

        /// <summary>Ölenleri çıkarır, eksikleri doğurur (karede en çok <paramref name="perFrame"/>).</summary>
        public void Tick(EnemySpawner spawner, SpawnZone[] zones, int target, int perFrame)
        {
            RemoveDead();
            int budget = StressMix.SpawnBudget(_enemies.Count, target, perFrame);
            for (int i = 0; i < budget; i++)
            {
                if (!SpawnNext(spawner, zones))
                {
                    break;
                }
            }

            if (_enemies.Count > PeakAlive)
            {
                PeakAlive = _enemies.Count;
            }
        }

        public void Clear()
        {
            _enemies.Clear();
        }

        /// <summary>Sıradaki tipi rastgele bir bölgede doğurur. Doğurulamadıysa (NavMesh yok) false: bu karede bırakılır.</summary>
        private bool SpawnNext(EnemySpawner spawner, SpawnZone[] zones)
        {
            int type = _plan.PickIndex(_spawnIndex++);
            SpawnZone zone = zones[UnityEngine.Random.Range(0, zones.Length)];
            if (type < 0 || zone == null)
            {
                return false;
            }

            Enemy enemy = spawner.Spawn(_mix[type].Enemy, zone.GetRandomPoint());
            if (enemy == null)
            {
                return false;
            }

            _enemies.Add(enemy);
            return true;
        }

        private void RemoveDead()
        {
            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                Enemy enemy = _enemies[i];
                if (enemy == null || !enemy.IsAlive)
                {
                    int last = _enemies.Count - 1;
                    _enemies[i] = _enemies[last];
                    _enemies.RemoveAt(last);
                }
            }
        }
    }
}
