using System;
using LastHaven.Core;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>PauseSources (13.3): birden fazla kaynak; oyun ilk kaynakla durur, son kaynakla devam eder; aynı kaynak bir kez sayılır.</summary>
    public class PauseSourcesTests
    {
        [Test]
        public void SingleSource_PausesAndResumes()
        {
            var sources = new PauseSources();
            var cards = new object();

            Assert.IsTrue(sources.Add(cards), "İlk kaynak oyunu durdurur.");
            Assert.IsTrue(sources.IsPaused);
            Assert.IsTrue(sources.Remove(cards), "Son kaynak çıkınca oyun devam eder.");
            Assert.IsFalse(sources.IsPaused);
        }

        [Test]
        public void TwoSources_StaysPausedUntilBothResume()
        {
            var sources = new PauseSources();
            var cards = new object();
            var menu = new object();

            Assert.IsTrue(sources.Add(cards));
            Assert.IsFalse(sources.Add(menu), "Zaten duraklı: durum değişmedi.");
            Assert.IsFalse(sources.Remove(cards), "Menü hâlâ duraklatıyor.");
            Assert.IsTrue(sources.IsPaused);
            Assert.IsTrue(sources.Contains(menu));
            Assert.IsFalse(sources.Contains(cards));
            Assert.IsTrue(sources.Remove(menu));
            Assert.IsFalse(sources.IsPaused);
        }

        [Test]
        public void SameSourceTwice_CountsOnce()
        {
            var sources = new PauseSources();
            var cards = new object();

            sources.Add(cards);
            Assert.IsFalse(sources.Add(cards));
            Assert.AreEqual(1, sources.Count);
            Assert.IsTrue(sources.Remove(cards), "Tek Resume yeter (sayaç değil küme).");
        }

        [Test]
        public void Remove_UnknownSource_DoesNothing()
        {
            var sources = new PauseSources();
            var cards = new object();
            sources.Add(cards);

            Assert.IsFalse(sources.Remove(new object()));
            Assert.IsTrue(sources.IsPaused);
        }

        [Test]
        public void Sources_ComparedByReference()
        {
            var sources = new PauseSources();
            string first = new string('a', 3);
            string second = new string('a', 3);
            sources.Add(first);

            Assert.IsFalse(sources.Contains(second), "Eşit ama farklı nesne başka kaynaktır.");
        }

        [Test]
        public void RemoveWhere_LastGoneSource_Resumes()
        {
            var sources = new PauseSources();
            var gone = new object();
            var alive = new object();
            sources.Add(gone);
            sources.Add(alive);

            Assert.IsFalse(sources.RemoveWhere(source => source == gone), "Biri kaldı.");
            Assert.IsTrue(sources.RemoveWhere(source => source == alive));
            Assert.IsFalse(sources.IsPaused);
            Assert.IsFalse(sources.RemoveWhere(_ => true), "Boşken durum değişmez.");
        }

        [Test]
        public void Add_Null_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new PauseSources().Add(null));
        }
    }
}
