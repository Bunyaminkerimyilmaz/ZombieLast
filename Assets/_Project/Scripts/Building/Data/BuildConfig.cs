using System.Collections.Generic;
using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Building
{
    /// <summary>
    /// İnşa modu ayarları (GDD §11): yapı yuvaları (1–5), ızgara, jeneratör yasak bölgesi, önizleme, vurgulama ve çakışma kontrolü.
    /// Asset'i üretici oluşturur: <c>Data/Building/BuildConfig.asset</c>. Üs yarıçapı burada değil, <see cref="BaseAreaConfig"/>'te.
    /// </summary>
    public sealed class BuildConfig : ScriptableObject
    {
        [Header("Yapılar")]
        [SerializeField, Tooltip("Sırayla 1–5 tuşları (gamepad'de D-pad sol/sağ ile sırada gezilir). Boş yuva seçilemez.")]
        private List<BuildableData> _slots = new List<BuildableData>();

        [SerializeField, Min(0), Tooltip("Oyun başında her yapı tipi için havuzda hazırlanan örnek sayısı. Aşılırsa havuz uyarır.")]
        private int _structurePrewarm = 12;

        [Header("Izgara (GDD §11)")]
        [SerializeField, Min(0.1f), Tooltip("Hücre kenarı (m).")]
        private float _cellSize = 1f;

        [SerializeField, Min(0f), Tooltip("Jeneratörün yarı boyu (m): 2×2 gövde → 1.")]
        private float _generatorHalfSize = 1f;

        [SerializeField, Min(0f), Tooltip("Jeneratörün çevresinde inşa yasak mesafe (m).")]
        private float _generatorClearance = 2f;

        [Header("Önizleme")]
        [SerializeField, Tooltip("Hayalet önizlemenin yarı saydam materyali (_BaseColor ile renklenir). Üretici bağlar.")]
        private Material _ghostMaterial;

        [SerializeField, Tooltip("Geçerli yerleşim rengi (alfa = saydamlık).")]
        private Color _validColor = new Color(0.25f, 1f, 0.4f, 0.45f);

        [SerializeField, Tooltip("Geçersiz yerleşim rengi: alan dışı, çakışma veya para yetersiz.")]
        private Color _invalidColor = new Color(1f, 0.25f, 0.2f, 0.45f);

        [SerializeField, Min(0f), Tooltip("Gamepad'de yerleşim noktası: oyuncunun baktığı yönde bu kadar ileri (m).")]
        private float _gamepadPlacementDistance = 3f;

        [Header("Vurgulama (inşa modunda var olan yapının üzerine gelince)")]
        [SerializeField, Tooltip("Yapının üstüne binen yarı saydam kabuğun rengi (alfa = saydamlık). U yükseltir, X satar.")]
        private Color _highlightColor = new Color(1f, 0.85f, 0.3f, 0.35f);

        [SerializeField, Min(1f), Tooltip("Kabuğun yapıya göre ölçeği: yüzeyleri yapının hemen dışında kalsın (z-fighting olmasın).")]
        private float _highlightScale = 1.04f;

        [Header("Çakışma")]
        [SerializeField, Tooltip("Ayak izinde bu layer'lardan bir collider varsa yerleştirilemez (oyuncu, düşman, engel).")]
        private LayerMask _blockingLayers = GameLayers.PlayerMask | GameLayers.EnemyMask | GameLayers.ObstacleMask;

        [SerializeField, Min(0.1f), Tooltip("Çakışma kutusunun yerden yüksekliği (m).")]
        private float _blockingCheckHeight = 2f;

        public IReadOnlyList<BuildableData> Slots => _slots;

        public int StructurePrewarm => _structurePrewarm;

        public float CellSize => _cellSize;

        /// <summary>Jeneratör merkezli kare yasak bölgenin yarı kenarı (m): yarı boy + yasak mesafe.</summary>
        public float GeneratorKeepOutHalfExtent => _generatorHalfSize + _generatorClearance;

        public Material GhostMaterial => _ghostMaterial;

        public Color ValidColor => _validColor;

        public Color InvalidColor => _invalidColor;

        public float GamepadPlacementDistance => _gamepadPlacementDistance;

        public Color HighlightColor => _highlightColor;

        public float HighlightScale => _highlightScale;

        public LayerMask BlockingLayers => _blockingLayers;

        public float BlockingCheckHeight => _blockingCheckHeight;
    }
}
