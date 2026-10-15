using UnityEngine;
using UnityEngine.Rendering;

namespace LastHaven.GameFlow
{
    /// <summary>
    /// Gündüz veya gece ışığı (GDD §18): directional light, düz ambiyans, sis ve post-process profili. Asset'leri üretici
    /// oluşturur: <c>Data/Lighting/DayLighting.asset</c>, <c>NightLighting.asset</c>. Varsayılan değerler gündüzündür.
    /// <see cref="DayNightLighting"/> ikisi arasında geçiş yapar.
    /// </summary>
    public sealed class LightingProfile : ScriptableObject
    {
        [Header("Güneş / ay (directional light)")]
        [SerializeField, Tooltip("Dünya rotasyonu (Euler, derece). X = yerden yükseklik açısı, Y = geldiği yön. Geçişte küresel harmanlanır.")]
        private Vector3 _sunRotation = new Vector3(50f, -30f, 0f);

        [SerializeField, Tooltip("Işık rengi.")]
        private Color _sunColor = new Color(1f, 0.96f, 0.88f);

        [SerializeField, Min(0f), Tooltip("Işık yoğunluğu.")]
        private float _sunIntensity = 1.2f;

        [Header("Ambiyans")]
        [SerializeField, Tooltip("Düz (Flat) ortam ışığı: gölgedeki yüzeylerin aydınlığı ve rengi.")]
        private Color _ambientColor = new Color(0.5f, 0.5f, 0.52f);

        [Header("Sis (Exponential Squared)")]
        [SerializeField, Tooltip("Sis açık mı. Kapalıysa geçişte yoğunluk 0'dan kayar.")]
        private bool _fogEnabled = true;

        [SerializeField, Tooltip("Sis rengi.")]
        private Color _fogColor = new Color(0.75f, 0.76f, 0.78f);

        [SerializeField, Min(0f), Tooltip("Sis yoğunluğu. Kamera oyuncudan ~22 m uzakta: 0.006 ≈ %2, 0.02 ≈ %18 sis.")]
        private float _fogDensity = 0.006f;

        [Header("Post-process")]
        [SerializeField, Tooltip("Vignette, Color Adjustments, Bloom. Sahne kurulurken bu ışığın Global Volume'üne atanır; geçişte weight'ler harmanlanır.")]
        private VolumeProfile _volumeProfile;

        /// <summary>Post-process profili (null olabilir).</summary>
        public VolumeProfile VolumeProfile => _volumeProfile;

        /// <summary>Directional light, ambiyans ve sis değerleri (o anki inspector değerleriyle).</summary>
        public LightingValues Values => new LightingValues(
            Quaternion.Euler(_sunRotation), _sunColor, _sunIntensity, _ambientColor, _fogEnabled, _fogColor, _fogDensity);
    }
}
