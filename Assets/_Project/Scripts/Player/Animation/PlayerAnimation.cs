using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Player
{
    /// <summary>
    /// Oyuncunun durumunu <see cref="AnimationDriver"/>'a aktarır: yürüme hızı, atılma, ateş, doldurma, vuruş ve ölüm.
    /// Animator yokken vuruşta sarsılma ve ölümde devrilme yedeği oynar.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AnimationDriver), typeof(PlayerMotor), typeof(Health))]
    public sealed class PlayerAnimation : MonoBehaviour
    {
        private AnimationDriver _driver;
        private PlayerMotor _motor;
        private Health _health;

        private void Awake()
        {
            _driver = GetComponent<AnimationDriver>();
            _motor = GetComponent<PlayerMotor>();
            _health = GetComponent<Health>();
        }

        private void OnEnable()
        {
            _health.Damaged += OnDamaged;
            _health.Died += OnDied;
            EventBus<WeaponFiredEvent>.Subscribe(OnWeaponFired);
            EventBus<ReloadStartedEvent>.Subscribe(OnReloadStarted);
        }

        private void OnDisable()
        {
            _health.Damaged -= OnDamaged;
            _health.Died -= OnDied;
            EventBus<WeaponFiredEvent>.Unsubscribe(OnWeaponFired);
            EventBus<ReloadStartedEvent>.Unsubscribe(OnReloadStarted);
        }

        private void Update()
        {
            _driver.SetSpeed(_motor.Speed);
            _driver.SetDashing(_motor.IsDashing);
        }

        private void OnDamaged(DamagedEvent damaged)
        {
            // Öldüren vuruşta sadece ölüm oynar.
            if (_health.IsAlive)
            {
                _driver.TriggerHit();
            }
        }

        private void OnDied(DiedEvent died)
        {
            _driver.TriggerDie();
        }

        private void OnWeaponFired(WeaponFiredEvent fired)
        {
            if (fired.Source == gameObject)
            {
                _driver.TriggerFire();
            }
        }

        // Olayda kaynak yok; şimdilik sadece oyuncunun silahları doldurur.
        private void OnReloadStarted(ReloadStartedEvent started)
        {
            _driver.TriggerReload();
        }
    }
}
