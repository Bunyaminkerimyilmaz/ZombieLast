using System;
using LastHaven.Core;
using UnityEngine.UIElements;

namespace LastHaven.UI
{
    /// <summary>
    /// Market sekmeleri (Silahlar / Yükseltmeler / Malzeme): her <see cref="ShopCategory"/> için bir buton ve bir kart sayfası.
    /// Seçili sekmenin butonu <see cref="SelectedClass"/> alır, sadece onun sayfası görünür. Sekme tıklanınca veya odaktayken
    /// Enter / A (Submit) ile seçilir; sadece odaklanmak seçmez (kartlardan yukarı çıkarken sayfa değişmesin).
    /// </summary>
    public sealed class ShopTabBar
    {
        public const string SelectedClass = "shop-tab--selected";

        private readonly Button[] _tabs;
        private readonly VisualElement[] _pages;
        private readonly Action[] _clickHandlers;

        /// <param name="tabs">Sekme butonları, <see cref="ShopCategory"/> değeriyle sıralı.</param>
        /// <param name="pages">Kart sayfaları, aynı sırayla.</param>
        public ShopTabBar(Button[] tabs, VisualElement[] pages)
        {
            if (tabs.Length != pages.Length)
            {
                throw new ArgumentException("Sekme ve sayfa sayısı aynı olmalı.", nameof(pages));
            }

            _tabs = tabs;
            _pages = pages;
            _clickHandlers = new Action[tabs.Length];
            for (int i = 0; i < tabs.Length; i++)
            {
                var category = (ShopCategory)i;
                _clickHandlers[i] = () => Select(category);
            }

            Select(Selected);
        }

        /// <summary>Seçili sekme.</summary>
        public ShopCategory Selected { get; private set; }

        /// <summary>Seçili sekmenin butonu.</summary>
        public Button SelectedTab => _tabs[(int)Selected];

        public void Bind()
        {
            for (int i = 0; i < _tabs.Length; i++)
            {
                _tabs[i].clicked += _clickHandlers[i];
            }
        }

        public void Unbind()
        {
            for (int i = 0; i < _tabs.Length; i++)
            {
                _tabs[i].clicked -= _clickHandlers[i];
            }
        }

        /// <summary>Kategorinin kart sayfası; tanımsız kategoride null.</summary>
        public VisualElement GetPage(ShopCategory category)
        {
            int index = (int)category;
            return index >= 0 && index < _pages.Length ? _pages[index] : null;
        }

        public void Select(ShopCategory category)
        {
            int index = (int)category;
            if (index < 0 || index >= _tabs.Length)
            {
                return;
            }

            Selected = category;
            for (int i = 0; i < _tabs.Length; i++)
            {
                _tabs[i].EnableInClassList(SelectedClass, i == index);
                _pages[i].SetShown(i == index);
            }
        }
    }
}
