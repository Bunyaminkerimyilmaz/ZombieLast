using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Building
{
    /// <summary>
    /// Taretin mantıktan görsele köprüsü (CLAUDE.md "Görsel ayrımı"; kökte durur). <see cref="Turret"/> her kare yaw'ı ve güç
    /// durumunu verir; bu bileşen <c>Visual</c> içindeki kafayı döndürür, güç yokken aşağı eğer ve gösterge ışığını söndürür
    /// (MaterialPropertyBlock <c>_BaseColor</c>). Kafa veya gösterge atanmamışsa (kullanıcı modeli) o kısım sessizce atlanır;
    /// mantık etkilenmez.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TurretVisual : MonoBehaviour, IPoolable
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [Header("Görsel parçalar (Visual içinde, opsiyonel)")]
        [SerializeField, Tooltip("Dönen kafa (Visual içinde). Pivotu dönüş merkezi olmalı. Boşsa kafa dönmez.")]
        private Transform _head;

        [SerializeField, Tooltip("Güç göstergesi (Visual içinde). Boşsa gösterge yok.")]
        private Renderer _indicator;

        [Header("Güç")]
        [SerializeField, Tooltip("Güç varken gösterge rengi.")]
        private Color _poweredColor = new Color(0.3f, 1f, 0.45f);

        [SerializeField, Tooltip("Güç yokken gösterge rengi (sönük).")]
        private Color _unpoweredColor = new Color(0.12f, 0.12f, 0.12f);

        [SerializeField, Range(0f, 80f), Tooltip("Güç yokken kafanın aşağı eğilme açısı (derece).")]
        private float _unpoweredPitch = 30f;

        [SerializeField, Min(0f), Tooltip("Eğilme/kalkma hızı (derece/sn).")]
        private float _pitchSpeed = 90f;

        private MaterialPropertyBlock _block;
        private Quaternion _headRestLocal = Quaternion.identity;
        private Quaternion _headRestRelative = Quaternion.identity;
        private bool _initialized;
        private float _pitch;
        private bool _hasPowerState;
        private bool _isPowered;

        /// <summary>Kafanın şu anki eğimi (derece; 0 = düz, pozitif = aşağı).</summary>
        public float Pitch => _pitch;

        /// <summary>Gösterge şu an yanıyor mu.</summary>
        public bool IsIndicatorLit => _hasPowerState && _isPowered;

        /// <summary>Kafayı yaw'a çevirir, güç durumuna göre eğimi ilerletir ve göstergeyi ayarlar.</summary>
        /// <param name="yaw">Dünya yaw'ı (derece).</param>
        public void Apply(float yaw, bool isPowered, float deltaTime)
        {
            EnsureInitialized();
            SetPowered(isPowered);
            float targetPitch = isPowered ? 0f : _unpoweredPitch;
            _pitch = Mathf.MoveTowards(_pitch, targetPitch, _pitchSpeed * Mathf.Max(0f, deltaTime));
            if (_head != null)
            {
                // Modelin dinlenme dönüşü (köke göre) korunur; üstüne sadece yaw ve eğim eklenir.
                _head.rotation = Quaternion.Euler(_pitch, yaw, 0f) * _headRestRelative;
            }
        }

        /// <summary>Kafayı anında yaw'a ve güç durumunun eğimine oturtur (yerleşme, havuzdan çıkış).</summary>
        public void Snap(float yaw, bool isPowered)
        {
            EnsureInitialized();
            _pitch = isPowered ? 0f : _unpoweredPitch;
            _hasPowerState = false;
            Apply(yaw, isPowered, 0f);
        }

        public void OnSpawned()
        {
        }

        public void OnDespawned()
        {
            EnsureInitialized();
            _pitch = 0f;
            _hasPowerState = false;
            if (_head != null)
            {
                _head.localRotation = _headRestLocal;
            }
        }

        private void Awake()
        {
            EnsureInitialized();
        }

        // Kafa henüz döndürülmemişken (ilk etkinleşme) dinlenme pozu okunur. Tembel: EditMode testlerinde de çalışır.
        private void EnsureInitialized()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;
            if (_head != null)
            {
                _headRestLocal = _head.localRotation;
                _headRestRelative = Quaternion.Inverse(transform.rotation) * _head.rotation;
            }
        }

        private void SetPowered(bool isPowered)
        {
            if (_hasPowerState && _isPowered == isPowered)
            {
                return;
            }

            _hasPowerState = true;
            _isPowered = isPowered;
            if (_indicator == null)
            {
                return;
            }

            _block ??= new MaterialPropertyBlock();
            _indicator.GetPropertyBlock(_block);
            _block.SetColor(BaseColorId, isPowered ? _poweredColor : _unpoweredColor);
            _indicator.SetPropertyBlock(_block);
        }
    }
}
