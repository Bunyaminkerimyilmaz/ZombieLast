using UnityEngine;
using UnityEngine.AI;

namespace LastHaven.Enemies
{
    /// <summary>
    /// <see cref="Enemy"/>'nin NavMeshAgent ve transform işleri: yürüme (yol yeniden hesaplamayı seyreltir), durma,
    /// yerinde dönme, yerden çıkma ve ölünce yere batma. Kapalı ajana dokunmaz.
    /// </summary>
    internal sealed class EnemyNavigator
    {
        private const float MinFacingSqrDistance = 0.0001f;

        private readonly NavMeshAgent _agent;
        private readonly Transform _transform;
        private float _burrowDepth;
        private float _repathSqrDistance;
        private Vector3 _lastDestination;
        private bool _hasDestination;
        private float _sinkStartHeight;

        public EnemyNavigator(NavMeshAgent agent)
        {
            _agent = agent;
            _transform = agent.transform;
        }

        /// <summary>Ajan açık ve NavMesh üzerinde.</summary>
        public bool IsActive => _agent.enabled && _agent.isOnNavMesh;

        /// <summary>Ajanın hızı (m/sn); ajan kapalıyken 0. Animasyonun yürüme hızı.</summary>
        public float Speed => IsActive ? _agent.velocity.magnitude : 0f;

        /// <summary>Havuzdan çıkışta: ajanı açar, bulunduğu noktaya yerleştirir ve durdurur.</summary>
        public void Activate(float speed, float burrowDepth, float repathDistance)
        {
            _burrowDepth = burrowDepth;
            _repathSqrDistance = repathDistance * repathDistance;
            _hasDestination = false;

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

        /// <summary>Ajanı kapatır (havuza dönüş).</summary>
        public void Deactivate()
        {
            _agent.enabled = false;
            _hasDestination = false;
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

        /// <summary>
        /// Yerden çıkma: 0 = gömülme derinliğinde, 1 = ajanın yüzeydeki konumunda. Çıkarken ajan transform'u taşımaz.
        /// </summary>
        public void SetEmergence(float progress)
        {
            if (!IsActive)
            {
                return;
            }

            Vector3 surface = _agent.nextPosition;
            if (progress >= 1f)
            {
                _transform.position = surface;
                _agent.updatePosition = true;
                return;
            }

            _agent.updatePosition = false;
            _transform.position = surface + Vector3.down * (_burrowDepth * (1f - progress));
        }

        /// <summary>Ölüm: ajanı kapatır; batma o anki yükseklikten başlar.</summary>
        public void BeginDeath()
        {
            if (IsActive)
            {
                _agent.isStopped = true;
                _agent.ResetPath();
            }

            _agent.enabled = false;
            _hasDestination = false;
            _sinkStartHeight = _transform.position.y;
        }

        /// <summary>Yere batma: 0 = ölüm yüksekliği, 1 = gömülme derinliği kadar aşağıda. Ajan kapalıyken çağrılır.</summary>
        public void SetSink(float progress)
        {
            Vector3 position = _transform.position;
            position.y = _sinkStartHeight - _burrowDepth * progress;
            _transform.position = position;
        }
    }
}
