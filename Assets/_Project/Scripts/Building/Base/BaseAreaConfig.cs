using UnityEngine;

namespace LastHaven.Building
{
    /// <summary>
    /// Üs alanı ayarları (GDD §11). Asset'i üretici oluşturur: <c>Data/Base/BaseAreaConfig.asset</c>.
    /// Yarıçapın tek kaynağı budur: zemindeki üs dairesi ve engel yerleşimi de buradan okur.
    /// </summary>
    public sealed class BaseAreaConfig : ScriptableObject
    {
        [Header("Alan")]
        [SerializeField, Min(1f), Tooltip("Jeneratör merkezli inşa alanının yarıçapı (m). Değiştirince Build Game Scene'i çalıştır (zemindeki daire ve engeller yeniden kurulur).")]
        private float _radius = 14f;

        public float Radius => _radius;
    }
}
