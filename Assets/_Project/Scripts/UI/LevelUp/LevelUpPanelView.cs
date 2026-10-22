using System;
using System.Collections.Generic;
using LastHaven.Core;
using UnityEngine.UIElements;

namespace LastHaven.UI
{
    /// <summary>
    /// Seviye kartları panelinin UI Toolkit tarafı (<c>UI/LevelUp/LevelUpPanel.uxml</c>): seviye numarası, kart satırı
    /// (<see cref="PerkCardView"/>), sıradaki seviye sayısı ve odak. Her açılışta (ve sıradaki seviyede) kartlar yeniden kurulur.
    /// Kapanınca odak bırakılır (gizli kartta kalırsa oyundaki Enter / A onu tıklardı). UIDocument her açılışta ağacı yeniden kurduğu
    /// için denetleyici her <c>OnEnable</c>'da yenisini oluşturur.
    /// </summary>
    public sealed class LevelUpPanelView
    {
        private readonly VisualElement _screen;
        private readonly Label _levelValue;
        private readonly VisualElement _cardRow;
        private readonly VisualElement _pending;
        private readonly Label _pendingValue;
        private readonly List<PerkCardView> _cards = new List<PerkCardView>();

        public LevelUpPanelView(VisualElement screen, Label levelValue, VisualElement cardRow, VisualElement pending, Label pendingValue)
        {
            _screen = screen;
            _levelValue = levelValue;
            _cardRow = cardRow;
            _pending = pending;
            _pendingValue = pendingValue;
        }

        public int CardCount => _cards.Count;

        /// <summary>
        /// Seviyeyi, kalan bekleyen seviye sayısını (0 → satır gizli) ve kartları yazar. Önceki kartlar kaldırılır. Bilinmeyen perk
        /// id'si atlanır. Kurulan kart sayısını döndürür.
        /// </summary>
        public int Show(int level, int pendingAfter, IReadOnlyList<string> offer, IPerkService perks, VisualTreeAsset template,
            Action<int> clicked)
        {
            Clear();
            _levelValue.text = HudFormat.Count(level);
            _pending.SetShown(pendingAfter > 0);
            _pendingValue.text = HudFormat.Gain(pendingAfter);
            for (int i = 0; i < offer.Count; i++)
            {
                IPerkInfo perk = perks.GetPerk(offer[i]);
                PerkCardView card = perk != null ? PerkCardView.Create(template, i, perk, perks.GetStacks(perk.Id), clicked) : null;
                if (card != null)
                {
                    _cardRow.Add(card.Root);
                    _cards.Add(card);
                }
            }

            return _cards.Count;
        }

        /// <summary>
        /// Açıkken her kare: tuş ipuçları son kullanılan cihaza göre (<see cref="UIClassNames.Gamepad"/>), giriş animasyonu
        /// (<paramref name="enterReady"/>: açılıştan en az bir kare sonra) ve gamepad'de seçim sürerken ilk karta odak.
        /// </summary>
        public void Tick(bool gamepad, bool choosing, bool enterReady)
        {
            _screen.EnableInClassList(UIClassNames.Gamepad, gamepad);
            if (enterReady)
            {
                FinishEnter();
            }

            if (gamepad && choosing)
            {
                EnsureFocus();
            }
        }

        /// <summary>Giriş animasyonunu başlatır (açılıştan en az bir kare sonra: başlangıç durumu çizilmiş olmalı).</summary>
        public void FinishEnter()
        {
            for (int i = 0; i < _cards.Count; i++)
            {
                _cards[i].FinishEnter();
            }
        }

        /// <summary>Seçilen kart büyür (yeni yığınla), diğerleri söner.</summary>
        public void PlayChoice(int chosenIndex, int stacks, int maxStacks)
        {
            for (int i = 0; i < _cards.Count; i++)
            {
                if (_cards[i].Index == chosenIndex)
                {
                    _cards[i].PlayChosen(stacks, maxStacks);
                }
                else
                {
                    _cards[i].PlayDismissed();
                }
            }
        }

        /// <summary>Odak panelde değilse ilk karta odaklanır (gamepad: D-pad ile gezilir, A seçer).</summary>
        public void EnsureFocus()
        {
            if (_cards.Count > 0 && !HasFocus())
            {
                _cards[0].Root.Focus();
            }
        }

        /// <summary>Odak paneldeyse bırakılır (kapanırken).</summary>
        public void ReleaseFocus()
        {
            if (_screen.panel?.focusController?.focusedElement is VisualElement focused && _screen.Contains(focused))
            {
                focused.Blur();
            }
        }

        /// <summary>Kartları kaldırır.</summary>
        public void Clear()
        {
            for (int i = 0; i < _cards.Count; i++)
            {
                _cards[i].Remove();
            }

            _cards.Clear();
        }

        private bool HasFocus()
        {
            return _screen.panel?.focusController?.focusedElement is VisualElement focused && _screen.Contains(focused);
        }
    }
}
