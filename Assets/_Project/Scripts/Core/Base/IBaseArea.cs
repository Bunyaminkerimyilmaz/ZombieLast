using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>
    /// Jeneratör merkezli üs alanı (GDD §11: inşa alanı). İnşa ızgarası, yeniden doğma noktası ve "üste mi" kontrolleri bunu kullanır.
    /// </summary>
    public interface IBaseArea
    {
        /// <summary>Üssün merkezi (yer seviyesinde, dünya).</summary>
        Vector3 Center { get; }

        /// <summary>Üs alanının yarıçapı (m).</summary>
        float Radius { get; }

        /// <summary>Nokta üs alanının içinde mi. Mesafe yatay düzlemde (XZ) ölçülür; sınır dahildir.</summary>
        bool Contains(Vector3 position);
    }
}
