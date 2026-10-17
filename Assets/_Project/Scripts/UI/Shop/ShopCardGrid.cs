using System;
using System.Collections.Generic;
using LastHaven.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace LastHaven.UI
{
    /// <summary>
    /// Market kartlarının hepsi: katalog sırasıyla her öğe için bir <see cref="ShopCardView"/>, kendi sekmesinin sayfasında.
    /// Kartlar bir kez kurulur; <see cref="Refresh"/> durumları servisten okur (kart sadece değişince yeniden çizilir).
    /// </summary>
    public sealed class ShopCardGrid
    {
        private readonly List<ShopCardView> _cards = new List<ShopCardView>();

        public int Count => _cards.Count;

        /// <summary>Kartları kurar ve <paramref name="tabs"/>'ın sayfalarına ekler. Sayfası olmayan kategorideki öğe atlanır.</summary>
        public void Build(IReadOnlyList<IShopItem> items, VisualTreeAsset template, ShopTabBar tabs, Action<ShopCardView> buyClicked)
        {
            for (int i = 0; i < items.Count; i++)
            {
                IShopItem item = items[i];
                VisualElement page = tabs.GetPage(item.Category);
                if (page == null)
                {
                    Debug.LogWarning($"ShopCardGrid: '{item.Id}' öğesinin sekmesi ({item.Category}) arayüzde yok; kart kurulmadı.");
                    continue;
                }

                ShopCardView card = ShopCardView.Create(template, item, FindDisplayName(items, item.RequiredItemId), buyClicked);
                if (card == null)
                {
                    return;
                }

                page.Add(card.Root);
                _cards.Add(card);
            }
        }

        /// <summary>Tüm kartların durumunu servisten okuyup uygular.</summary>
        public void Refresh(IShopService shop)
        {
            for (int i = 0; i < _cards.Count; i++)
            {
                ShopCardView card = _cards[i];
                string id = card.Item.Id;
                card.Apply(ShopCardState.From(shop.GetStatus(id), shop.GetLevel(id), card.Item.MaxLevel, shop.GetPrice(id)));
            }
        }

        /// <summary>Id'si verilen kart; yoksa null (ordinal karşılaştırma).</summary>
        public ShopCardView Find(string itemId)
        {
            for (int i = 0; i < _cards.Count; i++)
            {
                if (string.Equals(_cards[i].Item.Id, itemId, StringComparison.Ordinal))
                {
                    return _cards[i];
                }
            }

            return null;
        }

        /// <summary>Kategorideki ilk kartın Satın Al butonu; kart yoksa null.</summary>
        public Button FirstButton(ShopCategory category)
        {
            for (int i = 0; i < _cards.Count; i++)
            {
                if (_cards[i].Item.Category == category)
                {
                    return _cards[i].BuyButton;
                }
            }

            return null;
        }

        public void StopAnimations()
        {
            for (int i = 0; i < _cards.Count; i++)
            {
                _cards[i].StopAnimations();
            }
        }

        private static string FindDisplayName(IReadOnlyList<IShopItem> items, string itemId)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                return string.Empty;
            }

            for (int i = 0; i < items.Count; i++)
            {
                if (string.Equals(items[i].Id, itemId, StringComparison.Ordinal))
                {
                    return items[i].DisplayName;
                }
            }

            return itemId;
        }
    }
}
