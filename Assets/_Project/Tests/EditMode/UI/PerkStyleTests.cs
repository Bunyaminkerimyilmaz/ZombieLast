using LastHaven.Core;
using LastHaven.UI;
using NUnit.Framework;
using UnityEngine.UIElements;

namespace LastHaven.Tests.EditMode
{
    /// <summary>Perk görünüm kuralları (13.3): ikon yoksa baş harfler, nadirlik sınıfı.</summary>
    public class PerkStyleTests
    {
        [TestCase("Hızlı Tetik", "HT")]
        [TestCase("Şok Taret", "ŞT")]
        [TestCase("Zincirleme Son", "ZS")]
        [TestCase("Mıknatıs", "Mı")]
        [TestCase("Açgözlü", "Aç")]
        [TestCase("Adrenalin", "Ad")]
        [TestCase("Avcı", "Av")]
        [TestCase("  İki   Boşluklu  Ad ", "İB")]
        [TestCase("X", "X")]
        [TestCase("", "?")]
        [TestCase("   ", "?")]
        [TestCase(null, "?")]
        public void Initials(string displayName, string expected)
        {
            Assert.AreEqual(expected, PerkStyle.Initials(displayName));
        }

        [TestCase(PerkRarity.Common, "perk-card--common")]
        [TestCase(PerkRarity.Rare, "perk-card--rare")]
        [TestCase(PerkRarity.Epic, "perk-card--epic")]
        public void RarityClass(PerkRarity rarity, string expected)
        {
            Assert.AreEqual(expected, PerkStyle.RarityClass("perk-card", rarity));
        }

        [Test]
        public void SetRarity_KeepsOnlyCurrentRarityClass()
        {
            var element = new VisualElement();

            PerkStyle.SetRarity(element, "hud-perk", PerkRarity.Epic);
            PerkStyle.SetRarity(element, "hud-perk", PerkRarity.Rare);

            Assert.IsTrue(element.ClassListContains("hud-perk--rare"));
            Assert.IsFalse(element.ClassListContains("hud-perk--epic"));
            Assert.IsFalse(element.ClassListContains("hud-perk--common"));
        }

        [Test]
        public void EveryRarity_HasOwnModifier()
        {
            Assert.AreEqual("common", PerkStyle.RarityModifier(PerkRarity.Common));
            Assert.AreEqual("rare", PerkStyle.RarityModifier(PerkRarity.Rare));
            Assert.AreEqual("epic", PerkStyle.RarityModifier(PerkRarity.Epic));
            Assert.AreEqual("common", PerkStyle.RarityModifier((PerkRarity)99), "Tanımsız değer varsayılana düşer.");
        }
    }
}
