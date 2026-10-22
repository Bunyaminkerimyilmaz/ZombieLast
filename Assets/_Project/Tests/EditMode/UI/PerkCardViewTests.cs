using LastHaven.Core;
using LastHaven.Editor;
using LastHaven.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.UIElements;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// Seviye kartı ve HUD perk rozeti (13.3), gerçek <c>PerkCard.uxml</c> şablonuyla: metinler, kısayol, yığın "0/3", nadirlik sınıfı,
    /// baş harfler ve animasyon sınıfları; rozette yığın sayısı. Tıklama (panel gerektirir) duman testinde.
    /// </summary>
    public class PerkCardViewTests
    {
        private VisualTreeAsset _template;

        [SetUp]
        public void SetUp()
        {
            _template = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UIGenerator.PerkCardPath);
            Assert.IsNotNull(_template, UIGenerator.PerkCardPath);
        }

        [Test]
        public void Create_WritesTextsStacksAndRarity()
        {
            var perk = new FakePerk("thick_skin", 3, PerkRarity.Rare) { DisplayName = "Kalın Deri" };

            PerkCardView card = PerkCardView.Create(_template, 1, perk, 2, null);

            Assert.IsNotNull(card);
            Assert.IsNull(card.Root.parent, "Kart şablon sarmalayıcısından ayrılmalı.");
            Assert.AreEqual("2", card.Root.Q<Label>(LevelUpElementNames.CardKey).text, "İkinci kart: 2 tuşu.");
            Assert.AreEqual("Kalın Deri", card.Root.Q<Label>(LevelUpElementNames.CardName).text);
            Assert.AreEqual("thick_skin açıklama", card.Root.Q<Label>(LevelUpElementNames.CardDescription).text);
            Assert.AreEqual("2/3", card.Root.Q<Label>(LevelUpElementNames.CardStacks).text, "Mevcut yığın.");
            Assert.AreEqual("KD", card.Root.Q<Label>(LevelUpElementNames.CardInitials).text, "İkon yok: baş harfler.");
            Assert.IsTrue(card.Root.ClassListContains("perk-card--rare"));
            Assert.IsTrue(card.Root.ClassListContains(PerkCardView.EnterClass));
        }

        [Test]
        public void PlayChosen_ShowsNewStacks_OthersDismissed()
        {
            PerkCardView chosen = PerkCardView.Create(_template, 0, new FakePerk("quick_trigger", 3, PerkRarity.Common), 0, null);
            PerkCardView other = PerkCardView.Create(_template, 1, new FakePerk("greedy", 2, PerkRarity.Rare), 0, null);

            chosen.PlayChosen(1, 3);
            other.PlayDismissed();

            Assert.AreEqual("1/3", chosen.Root.Q<Label>(LevelUpElementNames.CardStacks).text);
            Assert.IsTrue(chosen.Root.ClassListContains(PerkCardView.ChosenClass));
            Assert.IsFalse(chosen.Root.ClassListContains(PerkCardView.EnterClass));
            Assert.IsTrue(other.Root.ClassListContains(PerkCardView.DismissedClass));
        }

        [Test]
        public void Badge_ShowsInitials_StacksOnlyFromTwo()
        {
            PerkBadgeView badge = PerkBadgeView.Create(new FakePerk("Hızlı Tetik", 3, PerkRarity.Common), null, null);
            Label stacks = badge.Root.Q<Label>(className: PerkBadgeView.StacksClass);

            badge.SetStacks(1, 3);
            Assert.AreEqual("HT", badge.Root.Q<Label>(className: PerkBadgeView.InitialsClass).text);
            Assert.IsTrue(stacks.ClassListContains(UIClassNames.Hidden), "Tek yığında sayı yok.");
            Assert.IsTrue(badge.Root.ClassListContains("hud-perk--common"));

            badge.SetStacks(2, 3);
            Assert.AreEqual("2", stacks.text);
            Assert.IsFalse(stacks.ClassListContains(UIClassNames.Hidden));
        }
    }
}
