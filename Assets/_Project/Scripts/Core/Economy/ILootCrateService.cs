using System.Collections.Generic;
using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>
    /// Haritadaki ganimet sandıkları (GDD §9; [Systems]/Loot). Gündüz başında dış halkaya dağılır, açılınca veya gece başında
    /// kalkar. HUD'un kenar göstergeleri buradan okur.
    /// </summary>
    public interface ILootCrateService
    {
        /// <summary>Haritadaki açılmamış sandık sayısı.</summary>
        int CrateCount { get; }

        /// <summary>Açılmamış sandıkların konumlarını (zemin, dünya) <paramref name="results"/>'a yazar; liste önce temizlenir.</summary>
        void GetCratePositions(List<Vector3> results);
    }
}
