using System;
using System.Collections.Generic;
using LastHaven.Core;
using LastHaven.Progression;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// Perk kartı çekilişi (13.2, GDD §13): farklı kartlar, tohumla tekrarlanabilirlik, maks. yığındakilerin dışlanması, nadirlik
    /// ağırlıkları (önce nadirlik, sonra eşit olasılıkla perk).
    /// </summary>
    public class PerkOfferGeneratorTests
    {
        private const int Seed = 12345;

        private readonly List<int> _results = new List<int>();
        private TestPerks _perks;

        [SetUp]
        public void SetUp()
        {
            _perks = new TestPerks();
        }

        [TearDown]
        public void TearDown()
        {
            _perks.DestroyAll();
        }

        [Test]
        public void Generate_PicksDistinctAvailableCandidates()
        {
            var generator = new PerkOfferGenerator(Seed);
            List<PerkOfferCandidate> candidates = GddPool();

            for (int draw = 0; draw < 200; draw++)
            {
                Assert.AreEqual(3, generator.Generate(candidates, GddWeight, 3, _results));
                Assert.AreEqual(3, _results.Count);
                Assert.AreEqual(3, new HashSet<int>(_results).Count, "Aynı çekilişte bir perk iki kez çıkmaz.");
            }
        }

        [Test]
        public void SameSeed_GivesSameOffers()
        {
            var first = new PerkOfferGenerator(Seed);
            var second = new PerkOfferGenerator(Seed);
            var other = new List<int>();
            List<PerkOfferCandidate> candidates = GddPool();

            for (int draw = 0; draw < 20; draw++)
            {
                first.Generate(candidates, GddWeight, 3, _results);
                second.Generate(candidates, GddWeight, 3, other);
                CollectionAssert.AreEqual(_results, other);
            }
        }

        [Test]
        public void DifferentSeeds_GiveDifferentSequences()
        {
            var first = new PerkOfferGenerator(1);
            var second = new PerkOfferGenerator(2);
            var other = new List<int>();
            List<PerkOfferCandidate> candidates = GddPool();
            bool differs = false;

            for (int draw = 0; draw < 20 && !differs; draw++)
            {
                first.Generate(candidates, GddWeight, 3, _results);
                second.Generate(candidates, GddWeight, 3, other);
                differs = !SequenceEqual(_results, other);
            }

            Assert.IsTrue(differs);
        }

        [Test]
        public void UnavailableCandidates_AreNeverOffered()
        {
            var generator = new PerkOfferGenerator(Seed);
            List<PerkOfferCandidate> candidates = GddPool();
            for (int i = 0; i < candidates.Count; i += 2)
            {
                candidates[i] = new PerkOfferCandidate(candidates[i].Rarity, false);
            }

            for (int draw = 0; draw < 300; draw++)
            {
                generator.Generate(candidates, GddWeight, 3, _results);
                foreach (int index in _results)
                {
                    Assert.IsTrue(candidates[index].IsAvailable, $"Maks. yığındaki aday {index} çıktı.");
                }
            }
        }

        [Test]
        public void FewerCandidatesThanCount_ReturnsAllAvailable()
        {
            var generator = new PerkOfferGenerator(Seed);
            var candidates = new List<PerkOfferCandidate>
            {
                new PerkOfferCandidate(PerkRarity.Common, false),
                new PerkOfferCandidate(PerkRarity.Rare, true),
                new PerkOfferCandidate(PerkRarity.Epic, true),
            };

            Assert.AreEqual(2, generator.Generate(candidates, GddWeight, 3, _results));
            CollectionAssert.AreEquivalent(new[] { 1, 2 }, _results);
        }

        [Test]
        public void NoAvailableCandidate_ReturnsZeroAndClearsResults()
        {
            var generator = new PerkOfferGenerator(Seed);
            _results.Add(99);

            Assert.AreEqual(0, generator.Generate(new[] { new PerkOfferCandidate(PerkRarity.Common, false) }, GddWeight, 3, _results));
            Assert.AreEqual(0, _results.Count);
        }

        [Test]
        public void ZeroWeightRarity_IsNeverOffered()
        {
            var generator = new PerkOfferGenerator(Seed);
            List<PerkOfferCandidate> candidates = GddPool();
            int NoEpic(PerkRarity rarity) => rarity == PerkRarity.Epic ? 0 : GddWeight(rarity);

            for (int draw = 0; draw < 300; draw++)
            {
                generator.Generate(candidates, NoEpic, 3, _results);
                foreach (int index in _results)
                {
                    Assert.AreNotEqual(PerkRarity.Epic, candidates[index].Rarity);
                }
            }

            var onlyEpics = new[] { new PerkOfferCandidate(PerkRarity.Epic, true) };
            Assert.AreEqual(0, generator.Generate(onlyEpics, NoEpic, 3, _results), "Ağırlığı 0 olan tek nadirlik: kart yok.");
        }

        [Test]
        public void SingleCard_RarityShare_FollowsGddWeights()
        {
            const int draws = 20000;
            var generator = new PerkOfferGenerator(Seed);
            List<PerkOfferCandidate> candidates = GddPool();
            int[] counts = new int[3];

            for (int draw = 0; draw < draws; draw++)
            {
                generator.Generate(candidates, GddWeight, 1, _results);
                counts[(int)candidates[_results[0]].Rarity]++;
            }

            Assert.AreEqual(0.6, counts[0] / (double)draws, 0.02, "Sıradan %60 (perk sayısından bağımsız).");
            Assert.AreEqual(0.3, counts[1] / (double)draws, 0.02, "Nadir %30.");
            Assert.AreEqual(0.1, counts[2] / (double)draws, 0.015, "Epik %10.");
        }

        [Test]
        public void SingleCard_WithinRarity_PerksAreEquallyLikely()
        {
            const int draws = 20000;
            var generator = new PerkOfferGenerator(Seed);
            var candidates = new List<PerkOfferCandidate>();
            for (int i = 0; i < 4; i++)
            {
                candidates.Add(new PerkOfferCandidate(PerkRarity.Common, true));
            }

            int[] counts = new int[4];
            for (int draw = 0; draw < draws; draw++)
            {
                generator.Generate(candidates, GddWeight, 1, _results);
                counts[_results[0]]++;
            }

            foreach (int count in counts)
            {
                Assert.AreEqual(0.25, count / (double)draws, 0.02);
            }
        }

        [Test]
        public void ExhaustedRarity_OthersShareItsWeight()
        {
            const int draws = 20000;
            var generator = new PerkOfferGenerator(Seed);
            var candidates = new List<PerkOfferCandidate>
            {
                new PerkOfferCandidate(PerkRarity.Common, true),
                new PerkOfferCandidate(PerkRarity.Rare, false),
                new PerkOfferCandidate(PerkRarity.Epic, true),
            };
            int commons = 0;

            for (int draw = 0; draw < draws; draw++)
            {
                generator.Generate(candidates, GddWeight, 1, _results);
                commons += _results[0] == 0 ? 1 : 0;
            }

            Assert.AreEqual(60.0 / 70.0, commons / (double)draws, 0.02, "Nadir kalmayınca Sıradan 60 / (60 + 10).");
        }

        [Test]
        public void PerkList_ExcludesMaxedPerks_AndReturnsPerkData()
        {
            var modifiers = new RunModifiers();
            var inventory = new PerkInventory(modifiers);
            PerkData maxed = _perks.Create("maxed", PerkRarity.Common, 1);
            PerkData open = _perks.Create("open", PerkRarity.Common, 2);
            PerkData rare = _perks.Create("rare", PerkRarity.Rare, 1);
            var perks = new List<PerkData> { maxed, null, open, rare };
            Assert.IsTrue(inventory.TryAcquire(maxed));
            Assert.IsTrue(inventory.TryAcquire(open));
            var generator = new PerkOfferGenerator(Seed);
            var offer = new List<PerkData>();

            for (int draw = 0; draw < 100; draw++)
            {
                Assert.AreEqual(2, generator.Generate(perks, inventory, GddWeight, 3, offer));
                CollectionAssert.AreEquivalent(new[] { open, rare }, offer, "Maks. yığındaki ve boş eleman çıkmaz.");
            }
        }

        [Test]
        public void NullArguments_Throw()
        {
            var generator = new PerkOfferGenerator(Seed);
            List<PerkOfferCandidate> candidates = GddPool();

            Assert.Throws<ArgumentNullException>(() => generator.Generate((IReadOnlyList<PerkOfferCandidate>)null, GddWeight, 3, _results));
            Assert.Throws<ArgumentNullException>(() => generator.Generate(candidates, null, 3, _results));
            Assert.Throws<ArgumentNullException>(() => generator.Generate(candidates, GddWeight, 3, null));
        }

        // GDD §13: 5 Sıradan, 5 Nadir, 4 Epik.
        private static List<PerkOfferCandidate> GddPool()
        {
            var candidates = new List<PerkOfferCandidate>();
            Add(candidates, PerkRarity.Common, 5);
            Add(candidates, PerkRarity.Rare, 5);
            Add(candidates, PerkRarity.Epic, 4);
            return candidates;
        }

        private static void Add(List<PerkOfferCandidate> candidates, PerkRarity rarity, int count)
        {
            for (int i = 0; i < count; i++)
            {
                candidates.Add(new PerkOfferCandidate(rarity, true));
            }
        }

        private static int GddWeight(PerkRarity rarity)
        {
            return rarity switch
            {
                PerkRarity.Common => 60,
                PerkRarity.Rare => 30,
                PerkRarity.Epic => 10,
                _ => 0,
            };
        }

        private static bool SequenceEqual(List<int> a, List<int> b)
        {
            if (a.Count != b.Count)
            {
                return false;
            }

            for (int i = 0; i < a.Count; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
