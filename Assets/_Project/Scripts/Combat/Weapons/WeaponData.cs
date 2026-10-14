using UnityEngine;

namespace LastHaven.Combat
{
    /// <summary>
    /// Bir silahın taban değerleri (GDD §6). Asset'leri üretici oluşturur: <c>Data/Weapons/</c>.
    /// Yükseltmeler ve run modifier'ları bu değerlerin üstüne çalışma anında uygulanır.
    /// </summary>
    public sealed class WeaponData : ScriptableObject
    {
        /// <summary><see cref="ReserveAmmo"/> bu değerse yedek mermi sonsuzdur.</summary>
        public const int InfiniteReserve = -1;

        [Header("Kimlik")]
        [SerializeField, Tooltip("Modüller arası referans için sabit id (ör. market → silah açma). Değiştirme.")]
        private string _id = "weapon";

        [SerializeField, Tooltip("Arayüzde görünen ad (Türkçe).")]
        private string _displayName = "Silah";

        [Header("Atış")]
        [SerializeField, Min(0f), Tooltip("Mermi/saçma başına hasar.")]
        private float _damage = 20f;

        [SerializeField, Min(1), Tooltip("Bir atıştaki saçma sayısı (pompalı 8, diğerleri 1).")]
        private int _pelletCount = 1;

        [SerializeField, Min(0.01f), Tooltip("Saniyedeki atış sayısı.")]
        private float _fireRate = 4f;

        [SerializeField, Min(0f), Tooltip("Saçılma konisinin toplam açısı (derece). Her mermi ±yarısı kadar sapar.")]
        private float _spreadAngle = 2f;

        [SerializeField, Min(0f), Tooltip("Menzil (m). Hasar mesafeyle düşmez.")]
        private float _range = 25f;

        [Header("Mermi")]
        [SerializeField, Min(1), Tooltip("Şarjör kapasitesi.")]
        private int _magSize = 12;

        [SerializeField, Min(InfiniteReserve), Tooltip("Başlangıç yedek mermisi. -1 = sonsuz.")]
        private int _reserveAmmo = InfiniteReserve;

        [SerializeField, Min(0f), Tooltip("Şarjör doldurma süresi (sn).")]
        private float _reloadTime = 1.2f;

        [Header("Görsel ve ses (opsiyonel)")]
        [SerializeField, Tooltip("Mermi izinin rengi.")]
        private Color _tracerColor = new Color(1f, 0.88f, 0.54f);

        [SerializeField, Tooltip("Opsiyonel atış sesi. Boşsa sessiz.")]
        private AudioClip _fireSound;

        [SerializeField, Tooltip("Opsiyonel namlu efekti prefab'ı. Boşsa atlanır.")]
        private GameObject _muzzleVfx;

        public string Id => _id;

        public string DisplayName => _displayName;

        public float Damage => _damage;

        public int PelletCount => _pelletCount;

        public float FireRate => _fireRate;

        /// <summary>İki atış arası süre (sn).</summary>
        public float FireInterval => 1f / _fireRate;

        public float SpreadAngle => _spreadAngle;

        public float Range => _range;

        public int MagSize => _magSize;

        /// <summary>Başlangıç yedek mermisi; <see cref="InfiniteReserve"/> (-1) ise sonsuz.</summary>
        public int ReserveAmmo => _reserveAmmo;

        public bool HasInfiniteReserve => _reserveAmmo < 0;

        public float ReloadTime => _reloadTime;

        public Color TracerColor => _tracerColor;

        /// <summary>Null olabilir.</summary>
        public AudioClip FireSound => _fireSound;

        /// <summary>Null olabilir.</summary>
        public GameObject MuzzleVfx => _muzzleVfx;
    }
}
