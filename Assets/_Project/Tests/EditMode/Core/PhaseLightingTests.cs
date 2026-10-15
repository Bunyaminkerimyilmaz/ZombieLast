using System;
using LastHaven.Core;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>PhaseLighting: alacakaranlık ve gece → gece; gündüz, şafak, zafer → gündüz; açılış ve oyun bitti → değişmez.</summary>
    public class PhaseLightingTests
    {
        [TestCase(GamePhase.Boot, LightingTarget.Unchanged)]
        [TestCase(GamePhase.Day, LightingTarget.Day)]
        [TestCase(GamePhase.Dusk, LightingTarget.Night)]
        [TestCase(GamePhase.Night, LightingTarget.Night)]
        [TestCase(GamePhase.Dawn, LightingTarget.Day)]
        [TestCase(GamePhase.GameOver, LightingTarget.Unchanged)]
        [TestCase(GamePhase.Victory, LightingTarget.Day)]
        public void For_MapsPhase(GamePhase phase, LightingTarget expected)
        {
            Assert.AreEqual(expected, PhaseLighting.For(phase));
        }

        [Test]
        public void For_CoversEveryPhase()
        {
            // Yeni evre eklenirse bu kurala da bakılsın diye: şu an 7 evre var.
            Assert.AreEqual(7, Enum.GetValues(typeof(GamePhase)).Length, "GamePhase değişti; PhaseLighting kuralını güncelle.");
        }
    }
}
