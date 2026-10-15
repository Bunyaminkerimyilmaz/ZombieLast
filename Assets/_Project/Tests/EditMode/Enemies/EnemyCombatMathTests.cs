using System;
using LastHaven.Core;
using LastHaven.Enemies;
using NUnit.Framework;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>Yakın dövüş hasarı (yapı çarpanı), yapı türleri ve saldırı menzili mesafe hesapları.</summary>
    public class EnemyCombatMathTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void IsStructure_EverythingButPlayer()
        {
            foreach (TargetType type in Enum.GetValues(typeof(TargetType)))
            {
                Assert.AreEqual(type != TargetType.Player, type.IsStructure(), type.ToString());
            }
        }

        [Test]
        public void MeleeDamage_AppliesMultiplierOnlyToStructures()
        {
            // Tank (GDD §7): 35 hasar, yapılara ×2.
            Assert.AreEqual(35f, MeleeDamage.Calculate(35f, TargetType.Player, 2f));
            Assert.AreEqual(70f, MeleeDamage.Calculate(35f, TargetType.Generator, 2f));
            Assert.AreEqual(70f, MeleeDamage.Calculate(35f, TargetType.Barricade, 2f));
            Assert.AreEqual(10f, MeleeDamage.Calculate(10f, TargetType.Turret, 1f));
        }

        [Test]
        public void Planar_IgnoresHeight()
        {
            Assert.AreEqual(5f, TargetDistance.Planar(new Vector3(0f, 10f, 0f), new Vector3(3f, -2f, 4f)), Tolerance);
        }

        [Test]
        public void PlanarToBounds_MeasuresToFootprintEdge()
        {
            // Jeneratör: 2×2×2 küp, merkez (0, 1, 0).
            var bounds = new Bounds(new Vector3(0f, 1f, 0f), new Vector3(2f, 2f, 2f));

            Assert.AreEqual(1.5f, TargetDistance.PlanarToBounds(new Vector3(2.5f, 0f, 0f), bounds), Tolerance, "Yan yüz.");
            Assert.AreEqual(Mathf.Sqrt(2f), TargetDistance.PlanarToBounds(new Vector3(-2f, 0f, 2f), bounds), Tolerance, "Köşe.");
            Assert.AreEqual(0f, TargetDistance.PlanarToBounds(new Vector3(0.5f, 0f, -0.5f), bounds), "İçeride.");
            Assert.AreEqual(1f, TargetDistance.PlanarToBounds(new Vector3(0f, 50f, -2f), bounds), Tolerance, "Yükseklik sayılmaz.");
        }
    }
}
