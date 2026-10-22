using System.Collections.Generic;
using LastHaven.Core;
using LastHaven.Progression;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>Perk alımı (13.2, GDD §13): yığın limiti, stat/bayrak/iyileşme etkilerinin uygulanması, kayıtlar.</summary>
    public class PerkInventoryTests
    {
        private const float Tolerance = 0.0001f;

        private readonly List<float> _heals = new List<float>();
        private TestPerks _perks;
        private RunModifiers _modifiers;
        private PerkInventory _inventory;

        [SetUp]
        public void SetUp()
        {
            _perks = new TestPerks();
            _modifiers = new RunModifiers();
            _heals.Clear();
            _inventory = new PerkInventory(_modifiers, _heals.Add);
        }

        [TearDown]
        public void TearDown()
        {
            _perks.DestroyAll();
        }

        [Test]
        public void StatPerk_AddsModifierWithPerkSource()
        {
            PerkData quickTrigger = QuickTrigger();

            Assert.IsTrue(_inventory.TryAcquire(quickTrigger));

            Assert.AreEqual(4f * 1.15f, _modifiers.Get(StatType.PlayerFireRate, 4f), Tolerance, "Hızlı Tetik: atış hızı +%15.");
            Assert.AreEqual(1, _modifiers.RemoveBySource("perk:quick_trigger"), "Kaynak id = perk: + id.");
        }

        [Test]
        public void Stacks_PercentagesAdd()
        {
            PerkData quickTrigger = QuickTrigger();

            _inventory.TryAcquire(quickTrigger);
            _inventory.TryAcquire(quickTrigger);

            Assert.AreEqual(2, _inventory.GetStacks("quick_trigger"));
            Assert.AreEqual(10f * 1.3f, _modifiers.Get(StatType.PlayerFireRate, 10f), Tolerance, "İki yığın +%30 (çarpılmaz).");
        }

        [Test]
        public void StackLimit_RejectsBeyondMax_WithoutApplying()
        {
            PerkData quickTrigger = QuickTrigger();
            for (int i = 0; i < 3; i++)
            {
                Assert.IsTrue(_inventory.TryAcquire(quickTrigger));
            }

            Assert.IsFalse(_inventory.CanAcquire(quickTrigger));
            Assert.IsFalse(_inventory.TryAcquire(quickTrigger));

            Assert.AreEqual(3, _inventory.GetStacks("quick_trigger"));
            Assert.AreEqual(3, _inventory.TotalStacks);
            Assert.AreEqual(10f * 1.45f, _modifiers.Get(StatType.PlayerFireRate, 10f), Tolerance, "4. alım etki eklemez.");
        }

        [Test]
        public void FlagPerk_SetsFlag()
        {
            PerkData vampire = _perks.Create("vampire", PerkRarity.Rare, 1, new FlagEffect(PerkFlag.Vampire));

            Assert.IsFalse(_modifiers.HasFlag(PerkFlag.Vampire));
            _inventory.TryAcquire(vampire);

            Assert.IsTrue(_modifiers.HasFlag(PerkFlag.Vampire));
            Assert.IsFalse(_inventory.TryAcquire(vampire), "Maks. yığın 1.");
        }

        [Test]
        public void ThickSkin_RaisesMaxBeforeHealing()
        {
            float maxAtHeal = 0f;
            var inventory = new PerkInventory(_modifiers, amount =>
            {
                maxAtHeal = _modifiers.Get(StatType.PlayerMaxHealth, 100f);
                _heals.Add(amount);
            });
            PerkData thickSkin = _perks.Create("thick_skin", PerkRarity.Common, 3,
                new StatModifierEffect(StatType.PlayerMaxHealth, StatModifierType.Flat, 25f), new HealEffect(25f));

            inventory.TryAcquire(thickSkin);

            CollectionAssert.AreEqual(new[] { 25f }, _heals);
            Assert.AreEqual(125f, maxAtHeal, Tolerance, "İyileşme anında maks. can zaten artmış.");
        }

        [Test]
        public void HealEffect_WithoutHealer_IsSkipped()
        {
            var inventory = new PerkInventory(_modifiers);
            PerkData heal = _perks.Create("heal", PerkRarity.Common, 1, new HealEffect(25f));

            Assert.IsTrue(inventory.TryAcquire(heal));
            Assert.AreEqual(1, inventory.GetStacks("heal"));
        }

        [Test]
        public void WithoutModifiers_StacksCountButEffectsAreSkipped()
        {
            var inventory = new PerkInventory(null);

            Assert.IsTrue(inventory.TryAcquire(QuickTrigger()));
            Assert.AreEqual(1, inventory.TotalStacks);
        }

        [Test]
        public void InvalidPerks_CannotBeAcquired()
        {
            PerkData noId = _perks.Create(string.Empty);

            Assert.IsFalse(_inventory.CanAcquire(null));
            Assert.IsFalse(_inventory.TryAcquire(null));
            Assert.IsFalse(_inventory.TryAcquire(noId));
            Assert.AreEqual(0, _inventory.TotalStacks);
            Assert.AreEqual(0, _inventory.GetStacks(null));
            Assert.AreEqual(0, _inventory.GetStacks("unknown"));
        }

        [Test]
        public void AcquiredIds_KeepFirstAcquireOrder()
        {
            PerkData quickTrigger = QuickTrigger();
            PerkData vampire = _perks.Create("vampire", PerkRarity.Rare, 1, new FlagEffect(PerkFlag.Vampire));

            _inventory.TryAcquire(vampire);
            _inventory.TryAcquire(quickTrigger);
            _inventory.TryAcquire(quickTrigger);

            CollectionAssert.AreEqual(new[] { "vampire", "quick_trigger" }, _inventory.AcquiredIds);
            Assert.AreEqual(3, _inventory.TotalStacks);
        }

        [Test]
        public void Ids_AreOrdinal()
        {
            _inventory.TryAcquire(QuickTrigger());

            Assert.AreEqual(0, _inventory.GetStacks("QUICK_TRIGGER"));
        }

        private PerkData QuickTrigger()
        {
            return _perks.Create("quick_trigger", PerkRarity.Common, 3,
                new StatModifierEffect(StatType.PlayerFireRate, StatModifierType.PercentAdd, 0.15f));
        }
    }
}
