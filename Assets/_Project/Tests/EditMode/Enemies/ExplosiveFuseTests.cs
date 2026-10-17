using LastHaven.Enemies;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>ExplosiveFuse: kurulu değilken patlamaz, bir kez patlar, zincir gecikmesi, havuz döngüsü.</summary>
    public class ExplosiveFuseTests
    {
        private ExplosiveFuse _fuse;

        [SetUp]
        public void SetUp()
        {
            _fuse = new ExplosiveFuse();
        }

        [Test]
        public void New_IsNotArmed_CannotDetonateOrPrime()
        {
            Assert.IsFalse(_fuse.IsArmed);
            Assert.IsFalse(_fuse.TryPrime(0.15f));
            Assert.IsFalse(_fuse.TryDetonate());
        }

        [Test]
        public void Armed_DetonatesOnce()
        {
            _fuse.Arm();

            Assert.IsTrue(_fuse.TryDetonate());
            Assert.IsTrue(_fuse.HasDetonated);
            Assert.IsFalse(_fuse.IsArmed);
            Assert.IsFalse(_fuse.TryDetonate(), "İkinci kez patlamaz (ölüm yolu tekrar çağırır).");
            Assert.IsFalse(_fuse.TryPrime(0.15f), "Patlamış olan zincire girmez.");
        }

        [Test]
        public void Primed_FiresAfterDelay_Once()
        {
            _fuse.Arm();

            Assert.IsTrue(_fuse.TryPrime(0.15f));
            Assert.IsTrue(_fuse.IsPrimed);
            Assert.IsFalse(_fuse.TryPrime(0.15f), "Zaten tetiklendi.");

            Assert.IsFalse(_fuse.Tick(0.1f));
            Assert.IsTrue(_fuse.Tick(0.06f)); // 0.15 − 0.1 − 0.05 float'ta tam 0 olmayabilir.
            Assert.IsFalse(_fuse.Tick(0.1f), "Gecikme bir kez dolar.");
        }

        [Test]
        public void Primed_ThenDetonatedEarly_StopsDelay()
        {
            _fuse.Arm();
            _fuse.TryPrime(0.15f);

            Assert.IsTrue(_fuse.TryDetonate(), "Gecikme sürerken vurulup ölürse hemen patlar.");
            Assert.IsFalse(_fuse.IsPrimed);
            Assert.IsFalse(_fuse.Tick(1f));
        }

        [Test]
        public void Disarm_ThenArm_StartsFresh()
        {
            _fuse.Arm();
            _fuse.TryPrime(0.15f);
            _fuse.Disarm();

            Assert.IsFalse(_fuse.Tick(1f), "Havuzdaki patlamaz.");

            _fuse.TryDetonate();
            _fuse.Arm();

            Assert.IsTrue(_fuse.IsArmed);
            Assert.IsFalse(_fuse.HasDetonated);
            Assert.IsFalse(_fuse.IsPrimed);
        }
    }
}
