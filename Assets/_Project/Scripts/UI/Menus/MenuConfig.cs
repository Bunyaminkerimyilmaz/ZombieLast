using UnityEngine;

namespace LastHaven.UI
{
    /// <summary>
    /// Menülerin ayarları (<c>Data/UI/MenuConfig.asset</c>; üretici oluşturur): ayarlar ekranındaki ses kaydırıcılarının klavye / gamepad
    /// adımı ve ana menünün arka planda dönen kamerası (GDD'de yok; görsel tercih).
    /// </summary>
    public sealed class MenuConfig : ScriptableObject
    {
        [Header("Ayarlar ekranı")]
        [SerializeField, Range(1, 50), Tooltip("Ses kaydırıcısı odaktayken sol / sağ (ok tuşları, D-pad, çubuk) bu kadar yüzde değiştirir.")]
        private int _volumeStepPercent = 5;

        [Header("Ana menü kamerası (yörünge)")]
        [SerializeField, Tooltip("Kameranın etrafında döndüğü nokta (dünya).")]
        private Vector3 _orbitCenter = Vector3.zero;

        [SerializeField, Min(1f), Tooltip("Merkeze yatay uzaklık (m).")]
        private float _orbitRadius = 19f;

        [SerializeField, Min(0f), Tooltip("Kameranın yerden yüksekliği (m).")]
        private float _orbitHeight = 9f;

        [SerializeField, Min(0f), Tooltip("Kameranın baktığı noktanın merkezden yüksekliği (m).")]
        private float _lookHeight = 1.5f;

        [SerializeField, Range(-45f, 45f), Tooltip("Bakışın yatay kayması (derece). Negatif = kamera sola döner, üs ekranın sağında, menü şeridinin dışında görünür.")]
        private float _lookYawOffset = -12f;

        [SerializeField, Tooltip("Dönüş hızı (derece/sn, gerçek zaman). Negatif = ters yön.")]
        private float _orbitSpeed = 4f;

        [SerializeField, Tooltip("Başlangıç açısı (derece; 0 = merkezin güneyi).")]
        private float _startAngle = 35f;

        [SerializeField, Range(10f, 90f), Tooltip("Görüş açısı (derece).")]
        private float _fieldOfView = 40f;

        public int VolumeStepPercent => Mathf.Clamp(_volumeStepPercent, 1, 100);

        public Vector3 OrbitCenter => _orbitCenter;

        public float OrbitRadius => Mathf.Max(1f, _orbitRadius);

        public float OrbitHeight => Mathf.Max(0f, _orbitHeight);

        public float LookHeight => Mathf.Max(0f, _lookHeight);

        public float LookYawOffset => Mathf.Clamp(_lookYawOffset, -45f, 45f);

        public float OrbitSpeed => _orbitSpeed;

        public float StartAngle => _startAngle;

        public float FieldOfView => Mathf.Clamp(_fieldOfView, 10f, 90f);
    }
}
