using System.Collections.Generic;
using LastHaven.Core;
using LastHaven.Economy;
using NUnit.Framework;
using UnityEngine;
using Random = System.Random;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// LootTable (GDD §9, 11.1): uzaklık eşiği (yakın &lt; 32 ≤ uzak), ağırlıklı seçim, dahil miktar aralığı; LootTableData'nın
    /// varsayılanları GDD tablosu.
    /// </summary>
    public class LootTableTests
    {
        private static readonly List<LootRewardOption> NearOnlyFuel = new List<LootRewardOption>
        {
            new LootRewardOption(LootRewardType.Fuel, 15, 15, 1f),
        };

        private static readonly List<LootRewardOption> FarOnlyMoney = new List<LootRewardOption>
        {
            new LootRewardOption(LootRewardType.Money, 80, 150, 1f),
        };

        [TestCase(0f, false)]
        [TestCase(31.99f, false)]
        [TestCase(32f, true)]
        [TestCase(38f, true)]
        public void Roll_UsesFarTableFromThresholdInclusive(float distance, bool expectFar)
        {
            var table = new LootTable(32f, NearOnlyFuel, FarOnlyMoney);

            LootRoll roll = table.Roll(distance, new Random(1));

            Assert.AreEqual(expectFar, roll.IsFar);
            Assert.AreEqual(expectFar ? LootRewardType.Money : LootRewardType.Fuel, roll.Type);
            Assert.IsFalse(roll.IsEmpty);
        }

        [TestCase(0.0, "a")]
        [TestCase(0.24, "a")]
        [TestCase(0.26, "b")]
        [TestCase(0.9999999, "b")]
        public void Pick_FollowsCumulativeWeights(double roll, string expected)
        {
            var a = new LootRewardOption(LootRewardType.Money, 1, 1, 1f);
            var b = new LootRewardOption(LootRewardType.Fuel, 1, 1, 3f);

            LootRewardOption picked = LootTable.Pick(new List<LootRewardOption> { a, b }, new FixedRandom(roll));

            Assert.AreSame(expected == "a" ? a : b, picked);
        }

        [Test]
        public void Pick_SkipsZeroWeightAndNull()
        {
            var zero = new LootRewardOption(LootRewardType.Money, 1, 1, 0f);
            var only = new LootRewardOption(LootRewardType.Fuel, 1, 1, 1f);
            var options = new List<LootRewardOption> { zero, null, only };

            Assert.AreSame(only, LootTable.Pick(options, new FixedRandom(0.0)));
            Assert.AreSame(only, LootTable.Pick(options, new FixedRandom(0.9999999)));
        }

        [Test]
        public void Roll_NoSelectableOption_IsEmpty()
        {
            var options = new List<LootRewardOption> { new LootRewardOption(LootRewardType.Money, 10, 20, 0f) };
            var table = new LootTable(32f, options, new List<LootRewardOption>());

            Assert.IsNull(LootTable.Pick(options, new Random(3)));
            Assert.IsTrue(table.Roll(10f, new Random(3)).IsEmpty);
            Assert.IsTrue(table.Roll(35f, new Random(3)).IsEmpty);
        }

        [Test]
        public void Pick_EqualWeights_RoughlyEqualShares()
        {
            var options = new List<LootRewardOption>
            {
                new LootRewardOption(LootRewardType.Money, 1, 1, 1f),
                new LootRewardOption(LootRewardType.Fuel, 1, 1, 1f),
                new LootRewardOption(LootRewardType.AmmoPack, 1, 1, 1f),
            };
            var counts = new Dictionary<LootRewardType, int>();
            var random = new Random(42);
            const int rolls = 3000;

            for (int i = 0; i < rolls; i++)
            {
                LootRewardType type = LootTable.Pick(options, random).Type;
                counts[type] = counts.TryGetValue(type, out int count) ? count + 1 : 1;
            }

            foreach (LootRewardOption option in options)
            {
                Assert.That(counts[option.Type] / (float)rolls, Is.InRange(0.28f, 0.39f), option.Type.ToString());
            }
        }

        [Test]
        public void RollAmount_InclusiveRange_HitsBothEnds()
        {
            var option = new LootRewardOption(LootRewardType.Money, 30, 60, 1f);
            var random = new Random(5);
            bool sawMin = false;
            bool sawMax = false;

            for (int i = 0; i < 2000; i++)
            {
                int amount = option.RollAmount(random);
                Assert.That(amount, Is.InRange(30, 60));
                sawMin |= amount == 30;
                sawMax |= amount == 60;
            }

            Assert.IsTrue(sawMin, "En az (30) çıkmalı.");
            Assert.IsTrue(sawMax, "En çok (60) dahil.");
        }

        [Test]
        public void Option_MaxBelowMin_UsesMin_NegativeWeightIsZero()
        {
            var option = new LootRewardOption(LootRewardType.Fuel, 15, 5, -2f);

            Assert.AreEqual(15, option.MaxAmount);
            Assert.AreEqual(15, option.RollAmount(new Random(1)));
            Assert.AreEqual(0f, option.Weight);
        }

        [Test]
        public void Data_DefaultsMatchGdd()
        {
            var data = ScriptableObject.CreateInstance<LootTableData>();
            try
            {
                Assert.AreEqual(32f, data.FarDistance);
                AssertOptions(data.NearRewards, (LootRewardType.Money, 30, 60), (LootRewardType.Fuel, 15, 15));
                AssertOptions(data.FarRewards,
                    (LootRewardType.Money, 80, 150), (LootRewardType.Fuel, 40, 40), (LootRewardType.AmmoPack, 1, 1));
                Assert.AreEqual(2, data.AmmoPack.Count);
                Assert.AreEqual("shotgun", data.AmmoPack[0].WeaponId);
                Assert.AreEqual(12, data.AmmoPack[0].Amount, "Marketteki pompalı paketi.");
                Assert.AreEqual("rifle", data.AmmoPack[1].WeaponId);
                Assert.AreEqual(60, data.AmmoPack[1].Amount, "Marketteki tüfek paketi.");

                LootTable table = data.CreateTable();
                Assert.IsFalse(table.IsFar(31.9f));
                Assert.IsTrue(table.IsFar(32f));
            }
            finally
            {
                Object.DestroyImmediate(data);
            }
        }

        [Test]
        public void Data_FarMoneyIsAlwaysBetterThanNearMoney()
        {
            var data = ScriptableObject.CreateInstance<LootTableData>();
            try
            {
                Assert.Greater(data.FarRewards[0].MinAmount, data.NearRewards[0].MaxAmount, "Uzak para > yakın para.");
                Assert.Greater(data.FarRewards[1].MinAmount, data.NearRewards[1].MaxAmount, "Uzak yakıt > yakın yakıt.");
            }
            finally
            {
                Object.DestroyImmediate(data);
            }
        }

        private static void AssertOptions(IReadOnlyList<LootRewardOption> options, params (LootRewardType Type, int Min, int Max)[] expected)
        {
            Assert.AreEqual(expected.Length, options.Count);
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i].Type, options[i].Type, $"seçenek {i}");
                Assert.AreEqual(expected[i].Min, options[i].MinAmount, $"seçenek {i}");
                Assert.AreEqual(expected[i].Max, options[i].MaxAmount, $"seçenek {i}");
                Assert.Greater(options[i].Weight, 0f, $"seçenek {i}: GDD 'biri seçilir'");
            }
        }

        /// <summary>NextDouble hep aynı değeri döndürür; Next(min, max) en azı.</summary>
        private sealed class FixedRandom : Random
        {
            private readonly double _value;

            public FixedRandom(double value)
            {
                _value = value;
            }

            public override double NextDouble()
            {
                return _value;
            }

            public override int Next(int minValue, int maxValue)
            {
                return minValue;
            }

            protected override double Sample()
            {
                return _value;
            }
        }
    }
}
