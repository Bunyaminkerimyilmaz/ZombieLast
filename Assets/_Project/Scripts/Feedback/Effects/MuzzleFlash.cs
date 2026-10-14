using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Feedback
{
    /// <summary>
    /// Namlu ışığı: kısa bir nokta ışık. Şiddet <see cref="PooledLifetime"/> ömrü boyunca 0'a iner, sonra havuza döner.
    /// Renk ve menzil Light bileşeninde.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Light), typeof(PooledLifetime))]
    public sealed class MuzzleFlash : MonoBehaviour
    {
        [SerializeField, Min(0f), Tooltip("Başlangıç ışık şiddeti; ömür boyunca 0'a iner.")]
        private float _intensity = 6f;

        private Light _light;
        private PooledLifetime _lifetime;

        private void Awake()
        {
            _light = GetComponent<Light>();
            _lifetime = GetComponent<PooledLifetime>();
        }

        private void OnEnable()
        {
            _light.intensity = _intensity;
        }

        private void LateUpdate()
        {
            _light.intensity = _intensity * (1f - _lifetime.Progress);
        }
    }
}
