using System;
using LastHaven.Core;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace LastHaven.Enemies
{
    /// <summary>
    /// <see cref="Enemy"/>'nin Unity tarafı ve durum makinesine gövdesi (<see cref="IEnemyBody"/>): yürüme, dönme, yerden çıkma ve yere
    /// batma <see cref="EnemyNavigator"/>'da; hedef <see cref="EnemyTargeting"/>'te; saldırı stratejide (<see cref="IEnemyAttackRuntime"/>);
    /// saldırı ve ölüm animasyonu <see cref="AnimationDriver"/>'da (yoksa atlanır). Doğuşta değerleri data'dan kurar
    /// (<see cref="Activate"/>), ölümde collider'ı ve ajanı kapatır (<see cref="BeginDeath"/>). Gezinme ayağının noktası
    /// <see cref="WanderSettings.PickLegDestination"/>'dan seçilir, en yakın NavMesh noktasına yürünür. Ceset <see cref="Pool"/>'a
    /// döner (havuz yoksa yok edilir).
    /// </summary>
    internal sealed class EnemyBody : IEnemyBody
    {
        private static readonly Func<float> UnityRandom = () => Random.value;

        private readonly GameObject _gameObject;
        private readonly Transform _transform;
        private readonly EnemyNavigator _navigator;
        private readonly EnemyTargeting _targeting;
        private readonly AnimationDriver _animation;
        private readonly IEnemyAttackRuntime _attack;
        private readonly Health _health;
        private readonly Collider _collider;

        /// <param name="animation">Null olabilir (bileşen yoksa animasyon tetikleri atlanır).</param>
        /// <param name="collider">Null olabilir.</param>
        public EnemyBody(GameObject gameObject, EnemyNavigator navigator, EnemyTargeting targeting, AnimationDriver animation,
            IEnemyAttackRuntime attack, Health health, Collider collider)
        {
            _gameObject = gameObject;
            _transform = gameObject.transform;
            _navigator = navigator;
            _targeting = targeting;
            _animation = animation;
            _attack = attack;
            _health = health;
            _collider = collider;
        }

        /// <summary>Cesedin döneceği havuz; null ise ceset yok edilir.</summary>
        public IPoolService Pool { get; set; }

        public bool HasTarget => _targeting.Target.IsValid;

        public float DistanceToTarget => _targeting.Target.DistanceFrom(_transform.position);

        public bool IsAttackBusy => _attack.IsBusy;

        /// <summary>Havuzdan çıkış: ölçek (<paramref name="baseScale"/> × data), tam can, collider ve ajan data'dan.</summary>
        public void Activate(EnemyData data, Vector3 baseScale)
        {
            _transform.localScale = baseScale * data.Scale;
            _health.SetMax(data.MaxHealth, refill: true);
            if (_collider != null)
            {
                _collider.enabled = true;
            }

            _navigator.Activate(data.MoveSpeed, data.BurrowDepth * data.Scale, data.RepathDistance, data.PickAvoidancePriority());
        }

        /// <summary>
        /// Ölüm: collider ve ajan kapanır, hedef bırakılır, devrilme oynar. Ölüm noktasını zemin yüzeyinde döndürür (yerden çıkarken
        /// öldüyse transform gömülüdür; jetonlar yüzeye düşsün). Batma o anki yükseklikten başlar.
        /// </summary>
        public Vector3 BeginDeath()
        {
            if (_collider != null)
            {
                _collider.enabled = false;
            }

            Vector3 surface = _navigator.SurfacePosition; // Ajan kapanmadan önce alınır.
            _navigator.BeginDeath();
            _targeting.Clear();
            if (_animation != null)
            {
                _animation.TriggerDie();
            }

            return surface;
        }

        public void SetEmergence(float progress) => _navigator.Burrow.SetEmergence(progress);

        public void MoveToTarget() => _navigator.MoveTo(_targeting.Target.Position);

        public void Halt() => _navigator.Halt();

        public void FaceTarget(float deltaTime) => _navigator.FaceTowards(_targeting.Target.Position, deltaTime);

        public void SetSink(float progress) => _navigator.Burrow.SetSink(progress);

        public bool HasArrived(float tolerance) => _navigator.HasArrived(tolerance);

        public void Attack()
        {
            if (_animation != null)
            {
                _animation.TriggerAttack();
            }

            _attack.Perform();
        }

        public bool StartWanderLeg(in WanderSettings settings)
        {
            if (!_navigator.IsActive)
            {
                return false;
            }

            Vector3 point = settings.PickLegDestination(_navigator.SurfacePosition, UnityRandom);
            if (!NavMesh.SamplePosition(point, out NavMeshHit hit, settings.NavMeshSampleDistance, NavMesh.AllAreas))
            {
                return false;
            }

            _navigator.MoveTo(hit.position);
            return true;
        }

        public void Despawn()
        {
            if (Pool != null)
            {
                Pool.Release(_gameObject);
                return;
            }

            Object.Destroy(_gameObject);
        }
    }
}
