using LastHaven.Enemies;
using NUnit.Framework;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// ExplosionChain: yatay yarıçaptaki üyeler gecikmeyle tetiklenir; dışındakiler, tetiklenmişler ve çıkarılanlar tetiklenmez; zincir
    /// halka halka ilerler.
    /// </summary>
    public class ExplosionChainTests
    {
        private sealed class FakeExplosive : IChainExplosive
        {
            private readonly ExplosiveFuse _fuse = new ExplosiveFuse();

            public FakeExplosive(Vector3 position)
            {
                Position = position;
                _fuse.Arm();
            }

            public Vector3 Position { get; }

            public bool IsPrimed => _fuse.IsPrimed;

            public float Delay => _fuse.RemainingDelay;

            public ExplosiveFuse Fuse => _fuse;

            public bool TryPrime(float delay) => _fuse.TryPrime(delay);
        }

        private ExplosionChain _chain;

        [SetUp]
        public void SetUp()
        {
            _chain = new ExplosionChain();
        }

        [Test]
        public void PrimeAround_PrimesMembersWithinPlanarRadius()
        {
            var near = new FakeExplosive(new Vector3(2f, 5f, 0f)); // Yükseklik sayılmaz.
            var edge = new FakeExplosive(new Vector3(0f, 0f, 3f));
            var far = new FakeExplosive(new Vector3(3.1f, 0f, 0f));
            _chain.Add(near);
            _chain.Add(edge);
            _chain.Add(far);

            int primed = _chain.PrimeAround(Vector3.zero, 3f, 0.15f);

            Assert.AreEqual(2, primed);
            Assert.IsTrue(near.IsPrimed);
            Assert.AreEqual(0.15f, near.Delay, 0.0001f);
            Assert.IsTrue(edge.IsPrimed, "Sınır dahil.");
            Assert.IsFalse(far.IsPrimed);
        }

        [Test]
        public void PrimeAround_SkipsAlreadyPrimedAndRemoved()
        {
            var primed = new FakeExplosive(Vector3.right);
            var removed = new FakeExplosive(Vector3.left);
            _chain.Add(primed);
            _chain.Add(removed);
            primed.TryPrime(0.15f);
            _chain.Remove(removed);

            Assert.AreEqual(0, _chain.PrimeAround(Vector3.zero, 3f, 0.15f));
            Assert.IsFalse(removed.IsPrimed);
            Assert.AreEqual(1, _chain.Count);
        }

        [Test]
        public void Add_IgnoresDuplicatesAndNull()
        {
            var member = new FakeExplosive(Vector3.zero);
            _chain.Add(member);
            _chain.Add(member);
            _chain.Add(null);

            Assert.AreEqual(1, _chain.Count);
        }

        [Test]
        public void Chain_PropagatesHopByHop()
        {
            // 0 — 2.5 — 5 m: ilk patlama sadece komşusunu, o da bir sonrakini tetikler.
            var first = new FakeExplosive(Vector3.zero);
            var second = new FakeExplosive(new Vector3(2.5f, 0f, 0f));
            var third = new FakeExplosive(new Vector3(5f, 0f, 0f));
            _chain.Add(first);
            _chain.Add(second);
            _chain.Add(third);

            first.Fuse.TryDetonate();
            _chain.Remove(first);
            Assert.AreEqual(1, _chain.PrimeAround(first.Position, 3f, 0.15f));
            Assert.IsFalse(third.IsPrimed);

            Assert.IsTrue(second.Fuse.Tick(0.15f));
            second.Fuse.TryDetonate();
            _chain.Remove(second);
            Assert.AreEqual(1, _chain.PrimeAround(second.Position, 3f, 0.15f));
            Assert.IsTrue(third.IsPrimed);
        }
    }
}
