using LastHaven.Combat;
using NUnit.Framework;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>SpreadPattern: saçmalar koninin içinde, her biri kendi diliminde; dönüş yatay düzlemde.</summary>
    public class SpreadPatternTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void SinglePellet_UsesWholeCone()
        {
            Assert.AreEqual(-1f, SpreadPattern.GetPelletAngle(0, 1, 2f, 0f), Tolerance);
            Assert.AreEqual(0f, SpreadPattern.GetPelletAngle(0, 1, 2f, 0.5f), Tolerance);
            Assert.AreEqual(1f, SpreadPattern.GetPelletAngle(0, 1, 2f, 1f), Tolerance);
        }

        [Test]
        public void Pellets_StayInsideTheirSlice()
        {
            const int pellets = 8;
            const float spread = 18f;
            const float slice = spread / pellets;

            for (int i = 0; i < pellets; i++)
            {
                float min = SpreadPattern.GetPelletAngle(i, pellets, spread, 0f);
                float max = SpreadPattern.GetPelletAngle(i, pellets, spread, 1f);
                Assert.AreEqual(-spread * 0.5f + i * slice, min, Tolerance);
                Assert.AreEqual(min + slice, max, Tolerance);
                Assert.GreaterOrEqual(min, -spread * 0.5f - Tolerance);
                Assert.LessOrEqual(max, spread * 0.5f + Tolerance);
            }
        }

        [Test]
        public void ZeroSpread_OrNoPellets_GivesNoDeviation()
        {
            Assert.AreEqual(0f, SpreadPattern.GetPelletAngle(3, 8, 0f, 0.7f));
            Assert.AreEqual(0f, SpreadPattern.GetPelletAngle(0, 0, 18f, 0.7f));
        }

        [Test]
        public void RandomOutsideRange_IsClamped()
        {
            Assert.AreEqual(1f, SpreadPattern.GetPelletAngle(0, 1, 2f, 5f), Tolerance);
            Assert.AreEqual(-1f, SpreadPattern.GetPelletAngle(0, 1, 2f, -5f), Tolerance);
        }

        [Test]
        public void RotateAroundUp_StaysHorizontalAndKeepsLength()
        {
            Vector3 rotated = SpreadPattern.RotateAroundUp(Vector3.forward, 90f);

            Assert.AreEqual(1f, rotated.x, Tolerance, "Yukarıdan bakınca pozitif açı saat yönü: ileri → sağ.");
            Assert.AreEqual(0f, rotated.y, Tolerance);
            Assert.AreEqual(0f, rotated.z, Tolerance);
            Assert.AreEqual(1f, rotated.magnitude, Tolerance);
        }
    }
}
