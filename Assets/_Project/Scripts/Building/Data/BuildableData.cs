using System.Collections.Generic;
using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Building
{
    /// <summary>
    /// İnşa edilebilir yapı tipi (GDD §11): kimlik, ızgara ayak izi, seviyeler, prefab. Asset'leri üretici oluşturur
    /// (<c>Data/Building/</c>). Modüller arası yapıya id ile başvurulur (<see cref="Id"/>).
    /// </summary>
    /// <remarks>
    /// Prefab'ın pivotu ayak izinin merkezinde ve yer seviyesinde olmalı; döndürülmemiş halde ayak izinin X'i yerel X,
    /// Y'si yerel Z boyunca uzanır. İnşa önizlemesi prefab'ın "Visual" alt objesinin kopyasıdır.
    /// </remarks>
    public sealed class BuildableData : ScriptableObject
    {
        [Header("Kimlik")]
        [SerializeField, Tooltip("Benzersiz id (ordinal karşılaştırılır). Olaylar ve market bu id'yi kullanır.")]
        private string _id;

        [SerializeField, Tooltip("Görünen ad (UI).")]
        private string _displayName;

        [SerializeField, TextArea, Tooltip("Kısa açıklama (inşa çubuğu ipucu, Gün 10).")]
        private string _description;

        [SerializeField, Tooltip("Opsiyonel ikon (inşa çubuğu). Boş olabilir.")]
        private Sprite _icon;

        [Header("Izgara")]
        [SerializeField, Tooltip("Hücre cinsinden ayak izi (X × Y), döndürülmemiş halde. Q her basışta 90° döndürür.")]
        private Vector2Int _footprint = Vector2Int.one;

        [Header("Seviyeler")]
        [SerializeField, Tooltip("1. seviye inşa fiyatı ve canı; sonrakiler yükseltmeler (GDD §11).")]
        private List<BuildableLevel> _levels = new List<BuildableLevel>();

        [Header("Prefab")]
        [SerializeField, Tooltip("Yerleştirilen yapı. Kök: mantık ve collider; 'Visual': mesh (önizleme bunu kopyalar).")]
        private GameObject _prefab;

        [SerializeField, Tooltip("NavMesh'i keser mi (barikat). Prefab'da NavMeshObstacle varsa yerleştirmede carving buna göre ayarlanır.")]
        private bool _carvesNavMesh;

        [Header("Hedeflenme")]
        [SerializeField, Tooltip("Düşmanlar bu yapıyı hangi tür olarak görür (GDD §7: Tank barikatı, Tükürücü tareti tercih eder).")]
        private TargetType _targetType = TargetType.Barricade;

        public string Id => _id;

        public string DisplayName => _displayName;

        public string Description => _description;

        public Sprite Icon => _icon;

        /// <summary>Döndürülmemiş ayak izi (hücre); iki kenar da en az 1.</summary>
        public Vector2Int Footprint => new Vector2Int(Mathf.Max(1, _footprint.x), Mathf.Max(1, _footprint.y));

        public IReadOnlyList<BuildableLevel> Levels => _levels;

        public int LevelCount => _levels.Count;

        /// <summary>İnşa fiyatı (1. seviyenin fiyatı); seviye yoksa 0.</summary>
        public int BuildCost => _levels.Count > 0 && _levels[0] != null ? _levels[0].Cost : 0;

        public GameObject Prefab => _prefab;

        public bool CarvesNavMesh => _carvesNavMesh;

        public TargetType TargetType => _targetType;

        /// <summary>0 tabanlı seviye (0 = 1. seviye); aralık dışında null.</summary>
        public BuildableLevel GetLevel(int levelIndex)
        {
            return levelIndex >= 0 && levelIndex < _levels.Count ? _levels[levelIndex] : null;
        }

        private void OnValidate()
        {
            _footprint = Footprint;
        }
    }
}
