using UnityEngine;
using UnityEngine.AI;

namespace LastHaven.Enemies
{
    /// <summary>
    /// Düşmanın yerden çıkması ve ölünce yere batması (transform yüksekliği). <see cref="EnemyNavigator"/> derinliği her doğuşta
    /// ayarlar; çıkarken ajan transform'u taşımaz, batarken ajan kapalıdır.
    /// </summary>
    internal sealed class EnemyBurrow
    {
        private readonly NavMeshAgent _agent;
        private readonly Transform _transform;
        private float _sinkStartHeight;

        public EnemyBurrow(NavMeshAgent agent)
        {
            _agent = agent;
            _transform = agent.transform;
        }

        /// <summary>Gömülme derinliği (m; ölçekli). Doğuşta yazılır.</summary>
        public float Depth { get; set; }

        /// <summary>
        /// Yerden çıkma: 0 = gömülme derinliğinde, 1 = ajanın yüzeydeki konumunda. Ajan kapalı veya NavMesh dışındaysa bir şey yapmaz.
        /// </summary>
        public void SetEmergence(float progress)
        {
            if (!_agent.enabled || !_agent.isOnNavMesh)
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
            _transform.position = surface + Vector3.down * (Depth * (1f - progress));
        }

        /// <summary>Batmayı o anki yükseklikten başlatır (ölüm anında, ajan kapatılırken).</summary>
        public void BeginSink()
        {
            _sinkStartHeight = _transform.position.y;
        }

        /// <summary>Yere batma: 0 = ölüm yüksekliği, 1 = gömülme derinliği kadar aşağıda. Ajan kapalıyken çağrılır.</summary>
        public void SetSink(float progress)
        {
            Vector3 position = _transform.position;
            position.y = _sinkStartHeight - Depth * progress;
            _transform.position = position;
        }
    }
}
