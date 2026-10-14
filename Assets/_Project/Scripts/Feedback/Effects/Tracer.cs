using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Feedback
{
    /// <summary>
    /// Mermi izi: namludan isabet noktasına (veya menzil sonuna) çizgi. Ömrü <see cref="PooledLifetime"/>'ta (~0.05 sn);
    /// bu sürede saydamlaşır ve havuza döner. Baş (hedef tarafı) parlak, kuyruk (namlu tarafı) soluk.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(LineRenderer), typeof(PooledLifetime))]
    public sealed class Tracer : MonoBehaviour
    {
        [SerializeField, Min(0f), Tooltip("Çizgi kalınlığı (m).")]
        private float _width = 0.08f;

        [SerializeField, Range(0f, 1f), Tooltip("Namlu tarafının saydamlığı (baş tarafına göre).")]
        private float _tailAlpha = 0.2f;

        private LineRenderer _line;
        private PooledLifetime _lifetime;
        private Color _color;

        /// <summary>İzi verilen uçlar ve renkle gösterir. Havuzdan alındıktan hemen sonra çağrılır.</summary>
        public void Show(Vector3 start, Vector3 end, Color color)
        {
            _line.SetPosition(0, start);
            _line.SetPosition(1, end);
            _line.widthMultiplier = _width;
            _color = color;
            Apply(0f);
        }

        private void Awake()
        {
            _line = GetComponent<LineRenderer>();
            _lifetime = GetComponent<PooledLifetime>();
            _line.useWorldSpace = true;
            _line.positionCount = 2;
        }

        private void LateUpdate()
        {
            Apply(_lifetime.Progress);
        }

        private void Apply(float progress)
        {
            float fade = 1f - progress;
            Color head = _color;
            head.a *= fade;
            Color tail = _color;
            tail.a *= _tailAlpha * fade;
            _line.startColor = tail;
            _line.endColor = head;
        }
    }
}
