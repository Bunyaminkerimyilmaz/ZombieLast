using System.Collections.Generic;
using UnityEngine;

namespace LastHaven.Economy
{
    /// <summary>
    /// Marketteki öğeler (GDD §6, §10). Asset'i üretici oluşturur: <c>Data/Shop/ShopCatalog.asset</c>; eksik öğeleri ekler.
    /// <see cref="ShopService"/> kullanır. Arayüz öğeleri bu sırayla gösterir.
    /// </summary>
    public sealed class ShopCatalog : ScriptableObject
    {
        [SerializeField, Tooltip("Market öğeleri, arayüz sırasıyla. Id'ler benzersiz olmalı.")]
        private List<ShopItemData> _items = new List<ShopItemData>();

        public IReadOnlyList<ShopItemData> Items => _items;
    }
}
