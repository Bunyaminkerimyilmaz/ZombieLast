using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Building
{
    /// <summary>
    /// Yapının üstündeki küçük onarım barı (GDD §11; iki quad: arka plan + dolgu, kameraya paralel). Onarım sürerken canın
    /// doluluğunu gösterir; son gösterimden sonra verilen süre kadar görünür kalır. Havuza dönünce gizlenir.
    /// Kökün çocuğudur, Visual'ın dışında (önizleme/vurgu kopyalarına ve seviye materyaline girmez).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class StructureRepairBar : MonoBehaviour, IPoolable
    {
        [SerializeField, Tooltip("Dolgu quad'ı (merkez pivotlu). Doluluk azaldıkça sola yaslı kısalır.")]
        private Transform _fill;

        [SerializeField, Min(0.01f), Tooltip("Dolgunun tam genişliği (m).")]
        private float _width = 1.2f;

        private readonly Countdown _visibility = new Countdown();
        private Renderer[] _renderers;
        private ICameraService _cameraService;
        private Transform _cameraTransform;
        private Vector3 _fillScale;
        private Vector3 _fillPosition;
        private bool _initialized;
        private bool _cameraResolved;

        public bool IsVisible { get; private set; }

        /// <summary>Gösterilen doluluk (0..1).</summary>
        public float Fill { get; private set; } = 1f;

        /// <summary>Barı verilen dolulukla gösterir; <paramref name="visibleFor"/> sn sonra (yeniden gösterilmezse) gizlenir.</summary>
        public void Show(float normalized, float visibleFor)
        {
            EnsureInitialized();
            SetFill(normalized);
            _visibility.Start(visibleFor);
            SetVisible(true);
            FaceCamera();
        }

        public void Hide()
        {
            EnsureInitialized();
            _visibility.Stop();
            SetVisible(false);
        }

        public void OnSpawned()
        {
            Hide();
        }

        public void OnDespawned()
        {
            Hide();
        }

        private void Awake()
        {
            Hide();
        }

        private void LateUpdate()
        {
            if (!IsVisible)
            {
                return;
            }

            if (_visibility.Tick(Time.deltaTime))
            {
                Hide();
                return;
            }

            FaceCamera();
        }

        // Tembel kurulum: EditMode testlerinde (Awake yokken) de çalışır.
        private void EnsureInitialized()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;
            _renderers = GetComponentsInChildren<Renderer>(true);
            if (_fill != null)
            {
                _fillScale = _fill.localScale;
                _fillPosition = _fill.localPosition;
            }
        }

        private void SetFill(float normalized)
        {
            Fill = Mathf.Clamp01(normalized);
            if (_fill == null)
            {
                return;
            }

            // Merkez pivotlu quad sola yaslı kısalır.
            _fill.localScale = new Vector3(_width * Fill, _fillScale.y, _fillScale.z);
            _fill.localPosition = new Vector3(_fillPosition.x - 0.5f * _width * (1f - Fill), _fillPosition.y, _fillPosition.z);
        }

        private void SetVisible(bool visible)
        {
            IsVisible = visible;
            for (int i = 0; i < _renderers.Length; i++)
            {
                _renderers[i].enabled = visible;
            }
        }

        // Kameraya paralel billboard. Servis ilk gösterimde (Start'tan sonra) alınır.
        private void FaceCamera()
        {
            if (!_cameraResolved)
            {
                _cameraResolved = true;
                ServiceLocator.TryGet(out _cameraService);
            }

            if (_cameraTransform == null && _cameraService != null && _cameraService.Camera != null)
            {
                _cameraTransform = _cameraService.Camera.transform;
            }

            if (_cameraTransform != null)
            {
                transform.rotation = _cameraTransform.rotation;
            }
        }
    }
}
