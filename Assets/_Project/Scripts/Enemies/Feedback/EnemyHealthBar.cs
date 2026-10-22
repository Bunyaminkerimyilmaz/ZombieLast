using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Enemies
{
    /// <summary>
    /// Düşmanın başının üstündeki küçük billboard can barı (iki quad: arka plan + dolgu). Sadece hasar aldıktan sonra
    /// ayarlanan süre (varsayılan 3 sn) görünür; her vuruşta süre baştan başlar. Ölünce ve havuza dönünce gizlenir.
    /// Kökün <see cref="Health"/>'ini dinler; Visual dışında durur (flaş ve devrilme etkilemez).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class EnemyHealthBar : MonoBehaviour, IPoolable
    {
        [SerializeField, Tooltip("Dolgu quad'ı (merkez pivotlu). Can azaldıkça sola yaslı kısalır.")]
        private Transform _fill;

        [SerializeField, Min(0.01f), Tooltip("Dolgunun tam genişliği (m).")]
        private float _width = 0.9f;

        [SerializeField, Min(0f), Tooltip("Son hasardan sonra görünür kalma süresi (sn).")]
        private float _visibleDuration = 3f;

        private readonly Countdown _visibility = new Countdown();
        private Health _health;
        private Renderer[] _renderers;
        private ICameraService _cameraService;
        private Transform _cameraTransform;
        private Vector3 _fillScale;
        private Vector3 _fillPosition;
        private bool _cameraResolved;

        public bool IsVisible { get; private set; }

        /// <summary>Gösterilen doluluk (0..1).</summary>
        public float Fill { get; private set; } = 1f;

        public void OnSpawned()
        {
            Hide();
        }

        public void OnDespawned()
        {
            Hide();
        }

        /// <summary>Barı verilen dolulukla gösterir ve görünürlük süresini baştan başlatır.</summary>
        public void Show(float normalized)
        {
            SetFill(normalized);
            _visibility.Start(_visibleDuration);
            SetVisible(true);
            FaceCamera();
        }

        public void Hide()
        {
            _visibility.Stop();
            SetVisible(false);
        }

        private void Awake()
        {
            _health = GetComponentInParent<Health>();
            _renderers = GetComponentsInChildren<Renderer>(true);
            if (_fill != null)
            {
                _fillScale = _fill.localScale;
                _fillPosition = _fill.localPosition;
            }

            if (_health == null)
            {
                Debug.LogError($"EnemyHealthBar: '{name}' üstünde Health yok; can barı çalışmayacak.", this);
            }
            else
            {
                // Havuzlanan obje: abonelik ömür boyu bir kez (OnEnable/OnDisable her doğuşta delegate ayırırdı). Health aynı düşmanda.
                _health.Damaged += OnDamaged;
                _health.Died += OnDied;
            }

            Hide();
        }

        private void OnDestroy()
        {
            if (_health != null)
            {
                _health.Damaged -= OnDamaged;
                _health.Died -= OnDied;
            }
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

        private void OnDamaged(DamagedEvent damaged)
        {
            // Öldüren vuruşta Died gizler.
            if (isActiveAndEnabled && _health.IsAlive && damaged.MaxHealth > 0f)
            {
                Show(damaged.CurrentHealth / damaged.MaxHealth);
            }
        }

        private void OnDied(DiedEvent died)
        {
            Hide();
        }

        private void SetFill(float normalized)
        {
            Fill = Mathf.Clamp01(normalized);
            if (_fill == null)
            {
                return;
            }

            HealthBarLayout.GetFill(Fill, _width, out float scaleX, out float offsetX);
            _fill.localScale = new Vector3(scaleX, _fillScale.y, _fillScale.z);
            _fill.localPosition = new Vector3(_fillPosition.x + offsetX, _fillPosition.y, _fillPosition.z);
        }

        private void SetVisible(bool visible)
        {
            IsVisible = visible;
            if (_renderers == null)
            {
                return;
            }

            for (int i = 0; i < _renderers.Length; i++)
            {
                _renderers[i].enabled = visible;
            }
        }

        // Kameraya paralel billboard: bar her zaman ekrana düz bakar. Servis ilk gösterimde (Start'tan sonra) alınır.
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
