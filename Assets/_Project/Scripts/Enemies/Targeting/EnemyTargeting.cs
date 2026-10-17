using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Enemies
{
    /// <summary>
    /// <see cref="Enemy"/>'nin hedefi: seçim (<see cref="TargetSelector"/>, GDD §7 kural 1–4) ve seçilen hedefin bileşenleri
    /// (<see cref="TrackedTarget"/>). Kural 4 için yolun durumunu <see cref="EnemyNavigator"/>'dan alır; hedef değişince yolu
    /// yeniden hesaplatır.
    /// </summary>
    internal sealed class EnemyTargeting
    {
        private readonly TrackedTarget _target = new TrackedTarget();
        private readonly EnemyNavigator _navigator;
        private TargetSelector _selector;
        private float _attackRange;

        public EnemyTargeting(EnemyNavigator navigator)
        {
            _navigator = navigator;
        }

        /// <summary>Seçili hedef ve bileşenleri.</summary>
        public TrackedTarget Target => _target;

        /// <summary>Kural 4 etkinse yolu tıkalı olan asıl hedef; değilse null.</summary>
        public ITargetable BlockedTarget => _selector?.BlockedTarget;

        /// <summary>Sadece aggro yarıçapındaki oyuncu hedeflenir (<see cref="TargetSelector.PlayerOnly"/>; gezgin modu).</summary>
        public bool PlayerOnly
        {
            get => _selector != null && _selector.PlayerOnly;
            set
            {
                if (_selector != null)
                {
                    _selector.PlayerOnly = value;
                }
            }
        }

        /// <summary>Hedef kaydını (<see cref="ITargetRegistry"/>) alıp bir kez kurulur; kayıt yoksa uyarır ve hiç hedef seçilmez.</summary>
        /// <param name="context">Uyarının bağlamı.</param>
        public void Initialize(in TargetSelectorSettings settings, Object context)
        {
            if (!ServiceLocator.TryGet(out ITargetRegistry registry))
            {
                Debug.LogWarning("Enemy: ITargetRegistry bulunamadı; düşman hedef seçemeyecek. Sahnede [Systems]/TargetRegistry var mı?", context);
            }

            _selector = new TargetSelector(registry, settings);
        }

        /// <summary>Havuzdan çıkışta: hedef silinir, ilk değerlendirme <paramref name="initialDelay"/> sn sonra.</summary>
        /// <param name="attackRange">Yarım yol bu menzil içinde biterse tıkalı sayılmaz (<see cref="PathInfo"/>).</param>
        public void Reset(in TargetSelectorSettings settings, float initialDelay, float attackRange)
        {
            _attackRange = attackRange;
            _selector.Reset(settings, initialDelay);
            Clear();
        }

        /// <summary>Seçiciyi ilerletir ve seçimi uygular.</summary>
        public void Tick(float deltaTime, Vector3 position)
        {
            PathInfo path = _navigator.TryGetPartialPathEnd(out Vector3 end)
                ? PathInfo.FromPartialPath(end, _target.DistanceFrom(end), _attackRange)
                : PathInfo.None;
            _selector.Tick(deltaTime, position, path);
            Set(_selector.Current);
        }

        /// <summary>
        /// Vurulabilir canlı hedef varsa ona yakın dövüş vuruşu yapar (yapılara <see cref="EnemyData.StructureDamageMultiplier"/>);
        /// vuruş noktası hedefin yüzeyinde. Vurduysa true.
        /// </summary>
        public bool TryStrike(Vector3 from, Vector3 forward, EnemyData data, GameObject attacker)
        {
            if (!_target.TryGetHit(from, forward, out IDamageable damageable, out Vector3 hitPoint, out Vector3 direction))
            {
                return false;
            }

            float amount = MeleeDamage.Calculate(data.Damage, _target.Target.TargetType, data.StructureDamageMultiplier);
            damageable.TakeDamage(new DamageInfo(amount, DamageType.Melee, attacker, hitPoint, direction));
            return true;
        }

        /// <summary>Hedefi bırakır (ölüm, havuza dönüş).</summary>
        public void Clear()
        {
            Set(null);
        }

        private void Set(ITargetable target)
        {
            if (_target.Set(target))
            {
                _navigator.InvalidatePath();
            }
        }
    }
}
