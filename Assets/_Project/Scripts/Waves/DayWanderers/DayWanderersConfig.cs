using LastHaven.Enemies;
using UnityEngine;

namespace LastHaven.Waves
{
    /// <summary>
    /// Gündüz gezginleri (GDD §9 gündüz tehlikesi): her gündüz başında dış halkada doğup üsse gitmeden dolaşan düşmanlar.
    /// Asset'i üretici oluşturur: <c>Data/Waves/DayWanderersConfig.asset</c>. <see cref="DayWanderers"/> kullanır.
    /// </summary>
    public sealed class DayWanderersConfig : ScriptableObject
    {
        [Header("Gezginler (GDD §9)")]
        [SerializeField, Tooltip("Gezgin düşman tipi (GDD §9: Yürüyen). Üretici bağlar.")]
        private EnemyData _enemy;

        [SerializeField, Min(0), Tooltip("Her gündüz başında en az bu kadar gezgin doğar.")]
        private int _minCount = 3;

        [SerializeField, Min(0), Tooltip("Her gündüz başında en fazla bu kadar gezgin doğar (dahil).")]
        private int _maxCount = 6;

        [Header("Alan (GDD §17 dış halka)")]
        [SerializeField, Min(0f), Tooltip("Gezginlerin doğup dolaştığı halkanın üs merkezine en kısa uzaklığı (m). GDD'nin dış halkası 18 m'de başlar; 20 m ile aggro yarıçapı (6 m) üs alanına (14 m) taşmaz.")]
        private float _minRadius = 20f;

        [SerializeField, Min(0f), Tooltip("Halkanın dış yarıçapı (m). Harita kenarı 40 m.")]
        private float _maxRadius = 38f;

        [Header("Doğma (GDD'de yok)")]
        [SerializeField, Min(0f), Tooltip("Gezgin oyuncuya bu yatay mesafeden (m) yakın doğmaz.")]
        private float _minPlayerDistance = 15f;

        [SerializeField, Min(1), Tooltip("Gezgin başına doğma noktası deneme sayısı.")]
        private int _spawnAttempts = 20;

        [Header("Dolaşma (GDD'de yok)")]
        [SerializeField, Min(0f), Tooltip("Bir yürüyüş ayağının en kısa uzunluğu (m).")]
        private float _minLegDistance = 4f;

        [SerializeField, Min(0f), Tooltip("Bir yürüyüş ayağının en uzun uzunluğu (m).")]
        private float _maxLegDistance = 10f;

        [SerializeField, Min(0f), Tooltip("Ayak arası en kısa bekleme (sn).")]
        private float _minPause = 1f;

        [SerializeField, Min(0f), Tooltip("Ayak arası en uzun bekleme (sn).")]
        private float _maxPause = 3f;

        [SerializeField, Min(0.1f), Tooltip("Bir ayak en fazla bu kadar sürer (sn); takılan gezgin durup yeni nokta seçer.")]
        private float _maxLegDuration = 12f;

        [SerializeField, Min(0f), Tooltip("Hedefe bu kadar (m) kalınca varılmış sayılır.")]
        private float _arrivalTolerance = 0.5f;

        [SerializeField, Min(0.1f), Tooltip("Seçilen noktanın en yakın NavMesh noktası en fazla bu kadar uzakta aranır (m).")]
        private float _navMeshSampleDistance = 3f;

        /// <summary>Null olabilir (üretici bağlamadıysa).</summary>
        public EnemyData Enemy => _enemy;

        public int MinCount => _minCount;

        public int MaxCount => Mathf.Max(_minCount, _maxCount);

        public float MinPlayerDistance => _minPlayerDistance;

        public int SpawnAttempts => _spawnAttempts;

        /// <summary>Bu gündüzün gezgin sayısı: <see cref="MinCount"/>–<see cref="MaxCount"/> (dahil) arası, <paramref name="random01"/> [0, 1).</summary>
        public int PickCount(float random01)
        {
            int span = MaxCount - _minCount;
            return Mathf.Min(_minCount + (int)(Mathf.Clamp01(random01) * (span + 1)), MaxCount);
        }

        /// <summary>Gezinme ayarları (o anki değerlerle); halka <paramref name="center"/> merkezli (üs merkezi).</summary>
        public WanderSettings CreateWanderSettings(Vector3 center)
        {
            return new WanderSettings(center, _minRadius, _maxRadius, _minLegDistance, _maxLegDistance, _minPause, _maxPause,
                _maxLegDuration, _arrivalTolerance, _navMeshSampleDistance);
        }
    }
}
