using LastHaven.Audio;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>Ayarlardaki ses düzeyi → mixer desibeli (14.3).</summary>
    public class VolumeDecibelsTests
    {
        private const float Min = -80f;

        [Test]
        public void Full_IsZeroDecibels()
        {
            Assert.AreEqual(0f, VolumeDecibels.FromLinear(1f, Min), 1e-4f);
        }

        [Test]
        public void Half_IsAboutMinusSix()
        {
            Assert.AreEqual(-6.02f, VolumeDecibels.FromLinear(0.5f, Min), 0.01f);
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(0.00001f)]
        public void ZeroOrTiny_IsMinimum(float linear)
        {
            Assert.AreEqual(Min, VolumeDecibels.FromLinear(linear, Min));
        }

        [Test]
        public void AboveOne_IsClampedToZeroDecibels()
        {
            Assert.AreEqual(0f, VolumeDecibels.FromLinear(2f, Min), 1e-4f);
        }
    }
}
