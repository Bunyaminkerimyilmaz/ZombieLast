using System;
using System.Collections.Generic;
using LastHaven.Core;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>StatCollection: (taban + Σflat) × (1 + Σyüzde), kaynak bazlı kaldırma ve değişim olayları.</summary>
    public class StatCollectionTests
    {
        private const float Tolerance = 0.0001f;

        private StatCollection _stats;
        private List<StatType> _changed;

        [SetUp]
        public void SetUp()
        {
            _stats = new StatCollection();
            _changed = new List<StatType>();
            _stats.Changed += _changed.Add;
        }

        [Test]
        public void Get_NoModifiers_ReturnsBase()
        {
            Assert.AreEqual(20f, _stats.Get(StatType.PlayerDamage, 20f));
            Assert.AreEqual(0, _stats.Count);
        }

        [Test]
        public void Flat_AddsToBase()
        {
            _stats.Add(StatModifier.Flat(StatType.PlayerMaxHealth, 25f, "thick_skin"));

            Assert.AreEqual(125f, _stats.Get(StatType.PlayerMaxHealth, 100f), Tolerance);
        }

        [Test]
        public void Percent_MultipliesBase()
        {
            _stats.Add(StatModifier.Percent(StatType.PlayerDamage, 0.15f, "heavy_rounds"));

            Assert.AreEqual(23f, _stats.Get(StatType.PlayerDamage, 20f), Tolerance);
        }

        [Test]
        public void FlatAndPercent_FlatAppliedBeforePercent()
        {
            // Market Mıknatıs +1 m, Mıknatıs perk'i +%50: (3 + 1) × 1.5 = 6.
            _stats.Add(StatModifier.Percent(StatType.PickupRadius, 0.5f, "perk_magnet"));
            _stats.Add(StatModifier.Flat(StatType.PickupRadius, 1f, "shop_magnet"));

            Assert.AreEqual(6f, _stats.Get(StatType.PickupRadius, 3f), Tolerance);
        }

        [Test]
        public void Percents_AreAddedNotCompounded()
        {
            // Hızlı Tetik ×2 yığın: +%15 + %15 = +%30 (×1.3225 değil).
            _stats.Add(StatModifier.Percent(StatType.PlayerFireRate, 0.15f, "quick_trigger"));
            _stats.Add(StatModifier.Percent(StatType.PlayerFireRate, 0.15f, "quick_trigger"));

            Assert.AreEqual(13f, _stats.Get(StatType.PlayerFireRate, 10f), Tolerance);
            Assert.AreEqual(0.3f, _stats.GetPercent(StatType.PlayerFireRate), Tolerance);
        }

        [Test]
        public void NegativePercent_Reduces()
        {
            // Adrenalin: dash bekleme süresi −%40.
            _stats.Add(StatModifier.Percent(StatType.DashCooldown, -0.4f, "adrenaline"));

            Assert.AreEqual(1.2f, _stats.Get(StatType.DashCooldown, 2f), Tolerance);
        }

        [Test]
        public void PercentBelowMinusHundred_ClampsToZero()
        {
            _stats.Add(StatModifier.Percent(StatType.DashCooldown, -0.7f, "a"));
            _stats.Add(StatModifier.Percent(StatType.DashCooldown, -0.7f, "b"));

            Assert.AreEqual(0f, _stats.Get(StatType.DashCooldown, 2f));
        }

        [Test]
        public void Modifiers_AffectOnlyTheirStat()
        {
            _stats.Add(StatModifier.Percent(StatType.TurretFireRate, 0.25f, "turret_master"));

            Assert.AreEqual(5f, _stats.Get(StatType.PlayerFireRate, 5f));
            Assert.AreEqual(6.25f, _stats.Get(StatType.TurretFireRate, 5f), Tolerance);
        }

        [Test]
        public void RemoveBySource_RemovesOnlyThatSourceAcrossStats()
        {
            _stats.Add(StatModifier.Percent(StatType.CoinValue, 1f, "night_blood_moon"));
            _stats.Add(StatModifier.Flat(StatType.PickupRadius, 2f, "night_blood_moon"));
            _stats.Add(StatModifier.Percent(StatType.CoinValue, 0.25f, "greedy"));

            int removed = _stats.RemoveBySource("night_blood_moon");

            Assert.AreEqual(2, removed);
            Assert.AreEqual(1, _stats.Count);
            Assert.AreEqual(12.5f, _stats.Get(StatType.CoinValue, 10f), Tolerance);
            Assert.AreEqual(3f, _stats.Get(StatType.PickupRadius, 3f), Tolerance);
        }

        [Test]
        public void RemoveBySource_UnknownOrCaseDifferent_RemovesNothing()
        {
            _stats.Add(StatModifier.Flat(StatType.PlayerDamage, 5f, "Perk"));
            _changed.Clear();

            Assert.AreEqual(0, _stats.RemoveBySource("perk"));
            Assert.AreEqual(0, _stats.RemoveBySource("missing"));
            Assert.AreEqual(1, _stats.Count);
            Assert.IsEmpty(_changed);
        }

        [Test]
        public void AddAndRemoveRepeatedly_NoFloatDrift()
        {
            _stats.Add(StatModifier.Percent(StatType.PlayerDamage, 0.1f, "base"));
            for (int i = 0; i < 1000; i++)
            {
                _stats.Add(StatModifier.Percent(StatType.PlayerDamage, 0.3f, "temp"));
                _stats.RemoveBySource("temp");
            }

            Assert.AreEqual(0.1f, _stats.GetPercent(StatType.PlayerDamage));
        }

        [Test]
        public void Changed_RaisedOncePerAffectedStat()
        {
            _stats.Add(StatModifier.Flat(StatType.PlayerDamage, 1f, "a"));
            _stats.Add(StatModifier.Flat(StatType.PlayerDamage, 1f, "a"));
            _stats.Add(StatModifier.Flat(StatType.CoinValue, 1f, "a"));
            CollectionAssert.AreEqual(
                new[] { StatType.PlayerDamage, StatType.PlayerDamage, StatType.CoinValue }, _changed);
            _changed.Clear();

            _stats.RemoveBySource("a");

            CollectionAssert.AreEquivalent(new[] { StatType.PlayerDamage, StatType.CoinValue }, _changed);
        }

        [Test]
        public void Changed_ListenerSeesUpdatedValuesOfAllStats()
        {
            _stats.Add(StatModifier.Flat(StatType.PlayerDamage, 1f, "a"));
            _stats.Add(StatModifier.Flat(StatType.CoinValue, 1f, "a"));
            var seen = new List<float>();
            _stats.Changed += _ => seen.Add(_stats.GetFlat(StatType.PlayerDamage) + _stats.GetFlat(StatType.CoinValue));

            _stats.RemoveBySource("a");

            CollectionAssert.AreEqual(new[] { 0f, 0f }, seen);
        }

        [Test]
        public void Clear_RemovesAllAndNotifies()
        {
            _stats.Add(StatModifier.Flat(StatType.TurretRange, 3f, "a"));
            _stats.Add(StatModifier.Flat(StatType.BarricadeMaxHealth, 50f, "b"));
            _changed.Clear();

            _stats.Clear();

            Assert.AreEqual(0, _stats.Count);
            Assert.AreEqual(12f, _stats.Get(StatType.TurretRange, 12f));
            CollectionAssert.AreEquivalent(new[] { StatType.TurretRange, StatType.BarricadeMaxHealth }, _changed);
        }

        [Test]
        public void Modifier_EmptySource_Throws()
        {
            Assert.Throws<ArgumentException>(() => StatModifier.Flat(StatType.PlayerDamage, 1f, ""));
            Assert.Throws<ArgumentException>(() => StatModifier.Percent(StatType.PlayerDamage, 1f, null));
        }
    }
}
