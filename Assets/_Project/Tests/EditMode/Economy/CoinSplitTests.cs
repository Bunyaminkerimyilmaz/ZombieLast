using System.Collections.Generic;
using System.Linq;
using LastHaven.Economy;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>CoinSplit (7.2): ödülün 1–5 jetona bölünmesi (GDD §10), toplamın korunması.</summary>
    public class CoinSplitTests
    {
        private readonly CoinSplit _split = new CoinSplit(1, 5, 5);
        private List<int> _values;

        [SetUp]
        public void SetUp()
        {
            _values = new List<int>();
        }

        [TestCase(10, new[] { 5, 5 })]       // Yürüyen
        [TestCase(12, new[] { 4, 4, 4 })]    // Koşucu
        [TestCase(20, new[] { 5, 5, 5, 5 })] // Patlayan
        [TestCase(60, new[] { 12, 12, 12, 12, 12 })] // Tank
        [TestCase(500, new[] { 100, 100, 100, 100, 100 })] // Brute
        [TestCase(3, new[] { 3 })]
        [TestCase(13, new[] { 5, 4, 4 })]
        public void Split_GddRewards(int total, int[] expected)
        {
            int count = _split.Split(total, _values);

            Assert.AreEqual(expected.Length, count);
            CollectionAssert.AreEqual(expected, _values);
        }

        [TestCase(0)]
        [TestCase(-5)]
        public void Split_NoValue_NoCoins(int total)
        {
            _values.Add(99);

            Assert.AreEqual(0, _split.Split(total, _values));
            Assert.IsEmpty(_values, "Liste temizlenmeli.");
        }

        [Test]
        public void Split_AnyTotal_KeepsSumAndBounds()
        {
            for (int total = 1; total <= 600; total++)
            {
                int count = _split.Split(total, _values);

                Assert.That(count, Is.InRange(1, 5), $"toplam {total}");
                Assert.AreEqual(count, _values.Count);
                Assert.AreEqual(total, _values.Sum(), $"toplam {total}");
                Assert.That(_values.Min(), Is.GreaterThanOrEqualTo(1));
                Assert.That(_values.Max() - _values.Min(), Is.LessThanOrEqualTo(1), "Değerler eşit dağılmalı.");
            }
        }

        [Test]
        public void GetCount_NeverMoreCoinsThanValue()
        {
            var split = new CoinSplit(3, 5, 5);

            Assert.AreEqual(2, split.GetCount(2), "Her jeton en az 1 değerinde olmalı.");
            Assert.AreEqual(3, split.GetCount(4), "En az jeton sayısı uygulanmalı.");
        }

        [Test]
        public void Constructor_ClampsInvalidValues()
        {
            var split = new CoinSplit(0, -2, 0);

            Assert.AreEqual(1, split.MinCoins);
            Assert.AreEqual(1, split.MaxCoins);
            Assert.AreEqual(1, split.TargetCoinValue);
            Assert.AreEqual(1, split.GetCount(50));
        }

        [Test]
        public void Default_DoesNotThrow()
        {
            CoinSplit split = default;

            Assert.AreEqual(1, split.Split(7, _values));
            CollectionAssert.AreEqual(new[] { 7 }, _values);
        }
    }
}
