using System.Collections.Generic;
using LastHaven.Core;
using LastHaven.Progression;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>PerkLookup (13.3'te PerkService'ten ayrıldı): katalog sırası, geçersizler atlanır, ordinal arama, IPerkInfo.</summary>
    public class PerkLookupTests
    {
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
        public void Collect_KeepsOrder_SkipsNullEmptyAndDuplicateIds()
        {
            PerkData first = _perks.Create("quick_trigger", PerkRarity.Common, 3);
            PerkData empty = _perks.Create(string.Empty);
            PerkData duplicate = _perks.Create("quick_trigger");
            PerkData second = _perks.Create("vampire", PerkRarity.Rare);
            var lookup = new PerkLookup();
            var skipped = new List<PerkData>();

            lookup.Collect(new[] { first, null, empty, duplicate, second }, skipped);

            CollectionAssert.AreEqual(new[] { first, second }, lookup.Perks);
            CollectionAssert.AreEqual(new[] { empty, duplicate }, skipped);
        }

        [Test]
        public void Find_IsOrdinal()
        {
            PerkData perk = _perks.Create("vampire", PerkRarity.Rare);
            var lookup = new PerkLookup();
            lookup.Collect(new[] { perk });

            Assert.AreSame(perk, lookup.Find("vampire"));
            Assert.IsNull(lookup.Find("Vampire"));
            Assert.IsNull(lookup.Find("VAMPİRE"));
            Assert.IsNull(lookup.Find(null));
            Assert.IsNull(lookup.Find(string.Empty));
        }

        [Test]
        public void PerkData_ExposesInfoThroughCoreInterface()
        {
            IPerkInfo info = _perks.Create("ricochet", PerkRarity.Epic, 1);

            Assert.AreEqual("ricochet", info.Id);
            Assert.AreEqual(PerkRarity.Epic, info.Rarity);
            Assert.AreEqual(1, info.MaxStacks);
            Assert.IsNull(info.Icon, "İkon opsiyonel.");
        }
    }
}
