using UnityEngine;
using UnityEngine.AI;

namespace LastHaven.Enemies
{
    /// <summary>
    /// <see cref="Enemy"/>'nin NavMeshAgent ve transform işleri: yürüme (yol yeniden hesaplamayı seyreltir), durma, yerinde
    /// dönme ve ölümde ajanı kapatma. Yerden çıkma ve yere batma <see cref="Burrow"/>'da. Kapalı ajana dokunmaz.
    /// </summary>
    internal sealed class EnemyNavigator
    {
        private const float MinFacingSqrDistance = 0.0001f;

        private readonly NavMeshAgent _agent;
        private readonly Transform _transform;
        private float _repathSqrDistance;
        private Vector3 _lastDestination;
        private bool _hasDestination;
        private readonly SpeedMultipliers _speedMultipliers = new SpeedMultipliers();
        private float _baseSpeed;

        public EnemyNavigator(NavMeshAgent agent)
        {
            _agent = agent;
            _transform = agent.transform;
            Burrow = new EnemyBurrow(agent);
        }

        /// <summary>Yerden çıkma ve yere batma (aynı ajan ve transform).</summary>
        public EnemyBurrow Burrow { get; }

        /// <summary>Ajan açık ve NavMesh üzerinde.</summary>
        public bool IsActive => _agent.enabled && _agent.isOnNavMesh;

        /// <summary>
        /// Ayakların zemindeki konumu: ajan açıkken ajanın yüzey konumu (yerden çıkarken transform gömülüdür), değilse transform.
        /// </summary>
        public Vector3 SurfacePosition => IsActive ? _agent.nextPosition : _transform.position;

        /// <summary>Ajanın hızı (m/sn); ajan kapalıyken 0. Animasyonun yürüme hızı.</summary>
        public float Speed => IsActive ? _agent.velocity.magnitude : 0f;

        /// <summary>Yürüme hızı sınırı: taban hız × kaynakların çarpımı (<see cref="SetSpeedMultiplier"/>, m/sn).</summary>
        public float MaxSpeed => _baseSpeed * _speedMultipliers.Product;

        /// <summary>
        /// Havuzdan çıkışta: ajanı açar, bulunduğu noktaya yerleştirir ve durdurur. <paramref name="avoidancePriority"/> (0–99)
        /// kalabalıkta kimin yol vereceğini belirler. Hız çarpanları 1'e döner.
        /// </summary>
        public void Activate(float speed, float burrowDepth, float repathDistance, int avoidancePriority)
        {
            Burrow.Depth = burrowDepth;
            _repathSqrDistance = repathDistance * repathDistance;
            _hasDestination = false;
            _baseSpeed = speed;
            _speedMultipliers.Reset();

            _agent.avoidancePriority = avoidancePriority;
            _agent.enabled = true;
            if (!_agent.Warp(_transform.position))
            {
                Debug.LogWarning($"[Enemy] {_transform.name} NavMesh'e yerleştirilemedi ({_transform.position}). Spawn noktası NavMesh üzerinde olmalı.", _transform);
                return;
            }

            _agent.speed = speed;
            _agent.updatePosition = true;
            _agent.updateRotation = false;
            _agent.isStopped = true;
        }

        /// <summary>
        /// Kaynağın hız çarpanını yazar (yavaşlatma: 1 − oran, öfke ×1.4); etkili hız taban × tüm kaynakların çarpımı. Negatif 0'a
        /// kırpılır. Ajan kapalıysa sadece saklanır.
        /// </summary>
        public void SetSpeedMultiplier(SpeedSource source, float multiplier)
        {
            _speedMultipliers.Set(source, multiplier);
            if (IsActive)
            {
                _agent.speed = MaxSpeed;
            }
        }

        /// <summary>Ajanı kapatır (havuza dönüş).</summary>
        public void Deactivate()
        {
            _agent.enabled = false;
            _hasDestination = false;
        }

        /// <summary>
        /// Şu anki yol yarım mı (<see cref="NavMeshPathStatus.PathPartial"/>: hedefe ulaşmıyor, ajan gidebildiği en yakın yere
        /// gider). Öyleyse yolun bittiği nokta. Yol hesaplanıyorsa veya ajan kapalıysa false.
        /// </summary>
        public bool TryGetPartialPathEnd(out Vector3 end)
        {
            end = default;
            if (!IsActive || !_hasDestination || _agent.pathPending || _agent.pathStatus != NavMeshPathStatus.PathPartial)
            {
                return false;
            }

            end = _agent.pathEndPosition;
            return true;
        }

        /// <summary>Yolu bir sonraki <see cref="MoveTo"/>'da yeniden hesaplatır (hedef değişince).</summary>
        public void InvalidatePath()
        {
            _hasDestination = false;
        }

        /// <summary>Hedefe yürür. Hedef son yol hedefinden yeniden hesaplama mesafesinden az kaydıysa yol korunur.</summary>
        public void MoveTo(Vector3 destination)
        {
            if (!IsActive)
            {
                return;
            }

            _agent.isStopped = false;
            _agent.updateRotation = true;

            bool closeToLast = _hasDestination && (destination - _lastDestination).sqrMagnitude < _repathSqrDistance;
            if (closeToLast && (_agent.hasPath || _agent.pathPending))
            {
                return;
            }

            _agent.SetDestination(destination);
            _lastDestination = destination;
            _hasDestination = true;
        }

        /// <summary>
        /// Son yürüme hedefine vardı mı: yol hesaplanmış ve kalan mesafe durma mesafesi + <paramref name="tolerance"/> içinde.
        /// Yol yoksa (ulaşılamayan nokta) veya ajan kapalıysa da true; yol hesaplanırken false.
        /// </summary>
        public bool HasArrived(float tolerance)
        {
            if (!IsActive)
            {
                return true;
            }

            if (_agent.pathPending)
            {
                return false;
            }

            return !_agent.hasPath || _agent.remainingDistance <= _agent.stoppingDistance + tolerance;
        }

        /// <summary>Hemen durur (yol korunur, kaymadan).</summary>
        public void Halt()
        {
            if (!IsActive || _agent.isStopped)
            {
                return;
            }

            _agent.isStopped = true;
            _agent.velocity = Vector3.zero;
            _agent.updateRotation = false;
        }

        /// <summary>Yerinde, ajanın dönüş hızıyla <paramref name="point"/>'e döner (sadece Y ekseni).</summary>
        public void FaceTowards(Vector3 point, float deltaTime)
        {
            Vector3 direction = point - _transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < MinFacingSqrDistance)
            {
                return;
            }

            Quaternion look = Quaternion.LookRotation(direction);
            _transform.rotation = Quaternion.RotateTowards(_transform.rotation, look, _agent.angularSpeed * deltaTime);
        }

        /// <summary>Ölüm: ajanı kapatır; batma (<see cref="Burrow"/>) o anki yükseklikten başlar.</summary>
        public void BeginDeath()
        {
            if (IsActive)
            {
                _agent.isStopped = true;
                _agent.ResetPath();
            }

            _agent.enabled = false;
            _hasDestination = false;
            Burrow.BeginSink();
        }
    }
}
