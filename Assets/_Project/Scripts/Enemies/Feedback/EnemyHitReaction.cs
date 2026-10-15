using LastHaven.Core;
using UnityEngine;
using UnityEngine.AI;

namespace LastHaven.Enemies
{
    /// <summary>
    /// Düşmanın vuruş tepkisi (GDD §7 "Geri bildirim"): beyaz flaş (<see cref="HitFlash"/>), vuruş animasyonu
    /// (<see cref="AnimationDriver"/>) ve hasar yönünde geri itilme (<see cref="EnemyData.KnockbackDistance"/> × (1 − direnç),
    /// kısa süreli <c>NavMeshAgent.Move</c>). Öldüren vuruş sadece flaşlar; devrilme <see cref="Enemy"/>'de.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Enemy), typeof(Health), typeof(NavMeshAgent))]
    public sealed class EnemyHitReaction : MonoBehaviour, IPoolable
    {
        private readonly Knockback _knockback = new Knockback();
        private Enemy _enemy;
        private Health _health;
        private NavMeshAgent _agent;
        private HitFlash _flash;
        private AnimationDriver _animation;

        /// <summary>Geri itilme durumu (hata ayıklama ve testler için).</summary>
        public Knockback Knockback => _knockback;

        public void OnSpawned()
        {
            _knockback.Stop();
        }

        public void OnDespawned()
        {
            _knockback.Stop();
        }

        private void Awake()
        {
            _enemy = GetComponent<Enemy>();
            _health = GetComponent<Health>();
            _agent = GetComponent<NavMeshAgent>();
            _flash = GetComponent<HitFlash>();
            _animation = GetComponent<AnimationDriver>();
        }

        private void OnEnable()
        {
            _health.Damaged += OnDamaged;
        }

        private void OnDisable()
        {
            _health.Damaged -= OnDamaged;
            _knockback.Stop();
        }

        private void Update()
        {
            if (!_knockback.IsActive)
            {
                return;
            }

            if (!CanBePushed())
            {
                _knockback.Stop();
                return;
            }

            _agent.Move(_knockback.Advance(Time.deltaTime));
            // Ajan itmeye karşı yürümesin; itme bitince kendi hızıyla yeniden hızlanır.
            _agent.velocity = Vector3.zero;
        }

        private void OnDamaged(DamagedEvent damaged)
        {
            if (_flash != null)
            {
                _flash.Flash();
            }

            if (!_health.IsAlive)
            {
                return;
            }

            if (_animation != null)
            {
                _animation.TriggerHit();
            }

            EnemyData data = _enemy.Data;
            if (data == null || !CanBePushed())
            {
                return;
            }

            float distance = Knockback.ResistedDistance(data.KnockbackDistance, data.KnockbackResistance);
            _knockback.Start(damaged.Damage.HitDirection, distance, data.KnockbackDuration);
        }

        // Yerden çıkarken ve ölüyken itilmez; ajan NavMesh üzerinde ve transform'u taşıyor olmalı.
        private bool CanBePushed()
        {
            return _enemy.IsAlive
                   && _enemy.State != EnemyStateId.Spawning
                   && _agent.enabled
                   && _agent.isOnNavMesh
                   && _agent.updatePosition;
        }
    }
}
