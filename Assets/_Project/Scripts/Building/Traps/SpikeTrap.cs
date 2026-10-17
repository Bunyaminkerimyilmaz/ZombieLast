using System;
using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Building
{
    /// <summary>
    /// Diken tuzağı (GDD §11): yapının (<see cref="Structure"/>, hedef türü <see cref="TargetType.Trap"/>) kökünde. Üstünden geçilir
    /// (NavMesh'i kesmez, NavMeshObstacle yok); alan kökteki trigger kutusu. Üstünde düşman varken her tik aralığında
    /// (<see cref="TrapTickTimer"/>) üstündeki her canlıya tik hasarı (hasar/sn × aralık, <see cref="DamageType.Trap"/>) ve
    /// yavaşlatma (<see cref="IStatusEffectReceiver"/>) verir. Dayanıklılık = yapının canı: her tikte tuzak kendine
    /// <see cref="SpikeTrapConfig.WearPerTick"/> hasar verir; bitince normal yıkılma yolu çalışır (moloz, hücreler boşalır).
    /// Evreye ve güce bakmaz.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Structure), typeof(BoxCollider))]
    public sealed class SpikeTrap : MonoBehaviour, IPoolable
    {
        private const string LogPrefix = "[Building] ";

        [SerializeField, Tooltip("Algılama, yavaşlatma süresi ve aşınma ayarları (tüm tuzaklar). Üretici bağlar.")]
        private SpikeTrapConfig _config;

        private readonly TrapTickTimer _timer = new TrapTickTimer();
        private Structure _structure;
        private BoxCollider _area;
        private TrapOccupants _occupants;
        private int _tickCount;

        /// <summary>Bu yerleşmede atılan tik sayısı (hata ayıklama ve testler).</summary>
        public int TickCount => _tickCount;

        /// <summary>Şu anki seviyenin değerleri; yerleşmeden önce geçersiz.</summary>
        public SpikeTrapStats Stats => _structure != null ? SpikeTrapStats.From(_structure.CurrentLevel) : default;

        public void OnSpawned()
        {
            _timer.Reset();
            _tickCount = 0;
        }

        public void OnDespawned()
        {
            _occupants?.Clear();
        }

        private void Awake()
        {
            _structure = GetComponent<Structure>();
            _area = GetComponent<BoxCollider>();
            if (_config == null)
            {
                Debug.LogError("SpikeTrap: SpikeTrapConfig atanmamış; Tools/Last Haven/Generate Prefabs çalıştır.", this);
                enabled = false;
                return;
            }

            _occupants = new TrapOccupants(_config.ScanBufferSize, _config.TargetLayers);
        }

        private void Update()
        {
            if (!_structure.IsPlaced || !_structure.Health.IsAlive || !_timer.Advance(Time.deltaTime))
            {
                return;
            }

            SpikeTrapStats stats = SpikeTrapStats.From(_structure.CurrentLevel);
            if (!stats.IsValid || _occupants.Scan(_area) == 0)
            {
                _timer.MarkIdle(_config.IdleScanInterval);
                return;
            }

            _timer.MarkTick(stats.TickInterval);
            Strike(stats);
            Wear();
        }

        private void Strike(in SpikeTrapStats stats)
        {
            float damage = stats.DamagePerTick;
            for (int i = 0; i < _occupants.Current.Count; i++)
            {
                TrapOccupant occupant = _occupants.Current[i];
                if (damage > 0f)
                {
                    // Yön yok: geri itme olmaz.
                    occupant.Damageable.TakeDamage(new DamageInfo(damage, DamageType.Trap, gameObject, occupant.Position, Vector3.zero));
                }

                if (occupant.Receiver != null && occupant.Damageable.IsAlive && stats.SlowFraction > 0f)
                {
                    occupant.Receiver.ApplySlow(stats.SlowFraction, _config.SlowDuration);
                }
            }
        }

        // Dayanıklılık = can. Son tikte Structure yıkılma yolunu çalıştırır ve tuzak bu çağrının içinde havuza döner.
        private void Wear()
        {
            _tickCount++;
            if (_config.WearPerTick <= 0f)
            {
                return;
            }

            int ticks = _tickCount;
            Health health = _structure.Health;
            health.TakeDamage(new DamageInfo(_config.WearPerTick, DamageType.Trap, gameObject, transform.position, Vector3.zero));
            if (!health.IsAlive)
            {
                Debug.Log(FormattableString.Invariant($"{LogPrefix}Diken tuzağı aşındı ve kırıldı ({ticks} tik)."), this);
            }
        }
    }
}
