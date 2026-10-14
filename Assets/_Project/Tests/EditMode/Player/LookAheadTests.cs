using LastHaven.Player;
using NUnit.Framework;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>LookAhead: kamera hedefi nişan yönüne yatayda en fazla look-ahead kadar kayar (GDD §4).</summary>
    public class LookAheadTests
    {
        private const float Tolerance = 0.0001f;
        private const float MaxDistance = 3f;

        [Test]
        public void FarAimPoint_ClampsToMaxDistance()
        {
            Vector3 offset = LookAhead.Compute(new Vector3(1f, 0f, 1f), new Vector3(11f, 1.2f, 1f), MaxDistance);

            Assert.AreEqual(3f, offset.x, Tolerance);
            Assert.AreEqual(0f, offset.y, Tolerance);
            Assert.AreEqual(0f, offset.z, Tolerance);
        }

        [Test]
        public void NearAimPoint_UsesFullOffset()
        {
            Vector3 offset = LookAhead.Compute(Vector3.zero, new Vector3(1f, 1.2f, -2f), MaxDistance);

            Assert.AreEqual(1f, offset.x, Tolerance);
            Assert.AreEqual(0f, offset.y, Tolerance);
            Assert.AreEqual(-2f, offset.z, Tolerance);
        }

        [Test]
        public void AimPointOnPlayer_NoOffset()
        {
            Assert.AreEqual(Vector3.zero, LookAhead.Compute(Vector3.one, new Vector3(1f, 5f, 1f), MaxDistance));
        }

        [Test]
        public void ZeroOrNegativeMaxDistance_NoOffset()
        {
            Assert.AreEqual(Vector3.zero, LookAhead.Compute(Vector3.zero, Vector3.forward * 10f, 0f));
            Assert.AreEqual(Vector3.zero, LookAhead.Compute(Vector3.zero, Vector3.forward * 10f, -1f));
        }
    }
}
