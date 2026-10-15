using LastHaven.Feedback;
using NUnit.Framework;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>Yıkılma efekti: parça sayısı yapının taban alanına göre, sınırlar içinde.</summary>
    public class DebrisEffectTests
    {
        [Test]
        public void GetParticleCount_ScalesWithFootprintArea()
        {
            Assert.AreEqual(24, DebrisEffect.GetParticleCount(new Vector3(3f, 1.5f, 1f), 8f, 10, 40), "3×1 barikat.");
            Assert.AreEqual(32, DebrisEffect.GetParticleCount(new Vector3(2f, 2f, 2f), 8f, 10, 40), "2×2; yükseklik sayılmaz.");
        }

        [Test]
        public void GetParticleCount_IsClampedToMinAndMax()
        {
            Assert.AreEqual(10, DebrisEffect.GetParticleCount(Vector3.zero, 8f, 10, 40), "Kutusuz yapı da efekt alır.");
            Assert.AreEqual(40, DebrisEffect.GetParticleCount(new Vector3(10f, 1f, 10f), 8f, 10, 40));
            Assert.AreEqual(10, DebrisEffect.GetParticleCount(new Vector3(3f, 1f, 1f), -5f, 10, 40), "Negatif yoğunluk 0 sayılır.");
            Assert.AreEqual(10, DebrisEffect.GetParticleCount(new Vector3(10f, 1f, 10f), 8f, 10, 5), "Max < min ise min.");
        }
    }
}
