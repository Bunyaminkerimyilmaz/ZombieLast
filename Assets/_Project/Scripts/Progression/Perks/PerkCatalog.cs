using System.Collections.Generic;
using UnityEngine;

namespace LastHaven.Progression
{
    /// <summary>
    /// Run'da çıkabilecek perk'ler (GDD §13). Asset'i üretici oluşturur: <c>Data/Progression/PerkCatalog.asset</c>; eksik perk'leri
    /// sona ekler. <see cref="PerkService"/> kullanır. Listeden çıkarılan perk çekilişe girmez.
    /// </summary>
    public sealed class PerkCatalog : ScriptableObject
    {
        [SerializeField, Tooltip("Çekilişteki perk'ler. Id'ler benzersiz olmalı; tekrar edenler ve boş elemanlar atlanır.")]
        private List<PerkData> _perks = new List<PerkData>();

        public IReadOnlyList<PerkData> Perks => _perks;
    }
}
