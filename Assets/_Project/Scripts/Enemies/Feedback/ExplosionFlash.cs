using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Enemies
{
    /// <summary>
    /// Yer tutucu patlama efekti: yarı saydam küre <see cref="PooledLifetime"/> ömrü boyunca küçük yarıçaptan patlama yarıçapına
    /// hızla genişleyip solar, nokta ışık söner; sonra havuza döner. Renk MaterialPropertyBlock ile yazılır (materyale dokunulmaz).
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PooledLifetime))]
    public sealed class ExplosionFlash : MonoBehaviour, IPoolable
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField, Tooltip("Genişleyen küre (Visual altında). Üretici bağlar.")]
        private Renderer _shell;

        [SerializeField, Tooltip("Opsiyonel parlama ışığı. Boşsa atlanır.")]
        private Light _light;

        [SerializeField, Min(0.01f), Tooltip("Kürenin ulaştığı yarıçap (m). Patlama yarıçapıyla aynı olmalı (GDD §7: 3).")]
        private float _radius = 3f;

        [SerializeField, Min(0f), Tooltip("Başlangıç yarıçapı (m).")]
        private float _startRadius = 0.3f;

        [SerializeField, Tooltip("Kürenin rengi; alfa ömür boyunca 0'a iner.")]
        private Color _color = new Color(1f, 0.55f, 0.1f, 0.6f);

        [SerializeField, Min(0f), Tooltip("Işığın başlangıç şiddeti; ömür boyunca 0'a iner.")]
        private float _lightIntensity = 8f;

        private PooledLifetime _lifetime;
        private MaterialPropertyBlock _block;

        public float Radius => _radius;

        public void OnSpawned()
        {
            Apply(0f);
        }

        public void OnDespawned()
        {
        }

        /// <summary>Ömrün <paramref name="progress"/> (0 → 1) anındaki görünüm. Testler doğrudan çağırır.</summary>
        internal void Apply(float progress)
        {
            float t = Mathf.Clamp01(progress);
            float remaining = 1f - t;
            float grow = 1f - remaining * remaining * remaining; // Hızlı başlar, yavaşlar.
            if (_shell != null)
            {
                _shell.transform.localScale = Vector3.one * (2f * Mathf.Lerp(_startRadius, _radius, grow));
                Color color = _color;
                color.a *= remaining * remaining;
                _block ??= new MaterialPropertyBlock();
                _block.SetColor(BaseColorId, color);
                _shell.SetPropertyBlock(_block);
            }

            if (_light != null)
            {
                _light.intensity = _lightIntensity * remaining;
            }
        }

        private void Awake()
        {
            _lifetime = GetComponent<PooledLifetime>();
        }

        private void LateUpdate()
        {
            Apply(_lifetime.Progress);
        }
    }
}
