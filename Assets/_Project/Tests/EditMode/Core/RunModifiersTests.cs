using System.Collections.Generic;
using LastHaven.Core;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>RunModifiers: stat modifier'ları StatCollection'a, bayrakları kendi dizisine yazar.</summary>
    public class RunModifiersTests
    {
        private const float Tolerance = 0.0001f;

        private RunModifiers _modifiers;

        [SetUp]
        public void SetUp()
        {
            _modifiers = new RunModifiers();
        }

        [Test]
        public void Get_AppliesModifiers()
        {
            _modifiers.AddModifier(StatModifier.Percent(StatType.CoinValue, 0.25f, "greedy"));
            _modifiers.AddModifier(StatModifier.Flat(StatType.CoinValue, 2f, "debug"));

            Assert.AreEqual(15f, _modifiers.Get(StatType.CoinValue, 10f), Tolerance);
            Assert.AreEqual(2, _modifiers.ModifierCount);
        }

        [Test]
        public void RemoveBySource_RestoresBase()
        {
            _modifiers.AddModifier(StatModifier.Percent(StatType.CoinValue, 1f, "night_blood_moon"));

            Assert.AreEqual(1, _modifiers.RemoveBySource("night_blood_moon"));
            Assert.AreEqual(10f, _modifiers.Get(StatType.CoinValue, 10f));
        }

        [Test]
        public void StatChanged_ForwardsCollectionChanges()
        {
            var changed = new List<StatType>();
            _modifiers.StatChanged += changed.Add;

            _modifiers.AddModifier(StatModifier.Flat(StatType.PlayerMaxHealth, 25f, "thick_skin"));
            _modifiers.RemoveBySource("thick_skin");

            CollectionAssert.AreEqual(new[] { StatType.PlayerMaxHealth, StatType.PlayerMaxHealth }, changed);
        }

        [Test]
        public void Flags_StartEmpty()
        {
            Assert.IsFalse(_modifiers.HasFlag(PerkFlag.Ricochet));
            Assert.IsFalse(_modifiers.HasFlag(PerkFlag.Hunter));
        }

        [Test]
        public void AddFlag_SetsOnlyThatFlag()
        {
            _modifiers.AddFlag(PerkFlag.Vampire);

            Assert.IsTrue(_modifiers.HasFlag(PerkFlag.Vampire));
            Assert.IsFalse(_modifiers.HasFlag(PerkFlag.ThornWalls));
            Assert.IsFalse(_modifiers.HasFlag(PerkFlag.ChainExplosion));
        }

        [Test]
        public void AddFlag_Twice_RaisesFlagAddedOnce()
        {
            var added = new List<PerkFlag>();
            _modifiers.FlagAdded += added.Add;

            _modifiers.AddFlag(PerkFlag.ShockTurret);
            _modifiers.AddFlag(PerkFlag.ShockTurret);

            CollectionAssert.AreEqual(new[] { PerkFlag.ShockTurret }, added);
        }

        [Test]
        public void FlagsAndModifiers_AreIndependent()
        {
            _modifiers.AddFlag(PerkFlag.Hunter);
            _modifiers.AddModifier(StatModifier.Flat(StatType.PlayerDamage, 5f, "hunter"));

            _modifiers.RemoveBySource("hunter");

            Assert.IsTrue(_modifiers.HasFlag(PerkFlag.Hunter));
            Assert.AreEqual(20f, _modifiers.Get(StatType.PlayerDamage, 20f));
        }
    }
}
