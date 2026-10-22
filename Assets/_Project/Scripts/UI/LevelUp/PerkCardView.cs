using System;
using LastHaven.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace LastHaven.UI
{
    /// <summary>
    /// Bir seviye kartı (<c>UI/LevelUp/PerkCard.uxml</c> örneği; kökü odaklanabilir bir <see cref="Button"/>): kısayol tuşu (1–3),
    /// nadirlik etiketi ve çerçeve rengi (<see cref="PerkStyle.SetRarity"/>), ikon ya da baş harfler, ad, açıklama ve mevcut yığın
    /// ("1/3"). Tıklama, Enter / gamepad A (odaktayken) <c>clicked</c> ile seçer. Animasyon sınıfları (süreler <c>LevelUp.uss</c>'te):
    /// açılışta <see cref="EnterClass"/> (bir kare sonra kaldırılır), seçilende <see cref="ChosenClass"/>, diğerlerinde
    /// <see cref="DismissedClass"/>.
    /// </summary>
    public sealed class PerkCardView
    {
        public const string CardClass = "perk-card";
        public const string EnterClass = "perk-card--enter";
        public const string ChosenClass = "perk-card--chosen";
        public const string DismissedClass = "perk-card--dismissed";

        private readonly Label _stacks;
        private readonly Action<int> _clicked;

        private PerkCardView(int index, IPerkInfo perk, Button root, Label stacks, Action<int> clicked)
        {
            Index = index;
            Perk = perk;
            Root = root;
            _stacks = stacks;
            _clicked = clicked;
            Root.clicked += OnClicked;
        }

        /// <summary>Soldan 0 tabanlı sıra (kısayol = sıra + 1).</summary>
        public int Index { get; }

        public IPerkInfo Perk { get; }

        /// <summary>Kartın kökü (şablonun TemplateContainer'ından ayrılmış buton).</summary>
        public Button Root { get; }

        /// <summary>
        /// Şablonu örnekler ve doldurur. Şablonda element eksikse hata loglar ve null döner.
        /// </summary>
        /// <param name="stacks">Perk'in şu anki yığını (alınmadıysa 0).</param>
        /// <param name="clicked">Seçilince kartın sırasıyla çağrılır.</param>
        public static PerkCardView Create(VisualTreeAsset template, int index, IPerkInfo perk, int stacks, Action<int> clicked)
        {
            TemplateContainer container = template.Instantiate();
            Button root = container.Q<Button>(className: CardClass);
            Label key = root?.Q<Label>(LevelUpElementNames.CardKey);
            VisualElement icon = root?.Q(LevelUpElementNames.CardIcon);
            Label initials = root?.Q<Label>(LevelUpElementNames.CardInitials);
            Label name = root?.Q<Label>(LevelUpElementNames.CardName);
            Label description = root?.Q<Label>(LevelUpElementNames.CardDescription);
            Label stackLabel = root?.Q<Label>(LevelUpElementNames.CardStacks);
            if (key == null || icon == null || initials == null || name == null || description == null || stackLabel == null)
            {
                Debug.LogError($"PerkCardView: kart şablonunda '{CardClass}' sınıflı buton veya bir element eksik (LevelUpElementNames.Card*).");
                return null;
            }

            key.text = HudFormat.Count(index + 1);
            name.text = perk.DisplayName;
            description.text = perk.Description;
            stackLabel.text = HudFormat.OfTotal(stacks, perk.MaxStacks);
            PerkStyle.ApplyIcon(icon, initials, perk);
            PerkStyle.SetRarity(root, CardClass, perk.Rarity);
            root.AddToClassList(EnterClass);

            // Şablonun TemplateContainer sarmalayıcısı yerleşimi bozmasın: kartın kendisi satıra eklenir.
            root.RemoveFromHierarchy();
            return new PerkCardView(index, perk, root, stackLabel, clicked);
        }

        /// <summary>Giriş animasyonunu başlatır (giriş sınıfı kaldırılır, transition kartı yerine getirir).</summary>
        public void FinishEnter()
        {
            Root.RemoveFromClassList(EnterClass);
        }

        /// <summary>Seçildi: büyür ve parlar; yığın yeni değeri gösterir.</summary>
        public void PlayChosen(int stacks, int maxStacks)
        {
            FinishEnter();
            _stacks.text = HudFormat.OfTotal(stacks, maxStacks);
            Root.AddToClassList(ChosenClass);
        }

        /// <summary>Başka kart seçildi: söner.</summary>
        public void PlayDismissed()
        {
            FinishEnter();
            Root.AddToClassList(DismissedClass);
        }

        /// <summary>Kartı panelden kaldırır ve tıklama dinleyicisini çıkarır.</summary>
        public void Remove()
        {
            Root.clicked -= OnClicked;
            Root.RemoveFromHierarchy();
        }

        private void OnClicked()
        {
            _clicked?.Invoke(Index);
        }
    }
}
