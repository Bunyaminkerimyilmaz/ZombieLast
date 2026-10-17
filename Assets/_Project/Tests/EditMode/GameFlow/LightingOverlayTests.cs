using LastHaven.Core;
using LastHaven.GameFlow;
using NUnit.Framework;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// LightingOverlay (GDD §14): olayın sis ve ay rengi ağırlıkla açılır/kapanır, olay ışığı değiştirmiyorsa katman yok, şimşek ay
    /// ışığını kısa süre parlatıp söner.
    /// </summary>
    public class LightingOverlayTests
    {
        private const float Tolerance = 0.001f;

        // Gece profili (NightLighting.asset varsayılanları).
        private static readonly LightingValues Night = new LightingValues(
            Quaternion.Euler(60f, 30f, 0f), new Color(0.45f, 0.55f, 0.9f), 0.5f, new Color(0.16f, 0.2f, 0.36f), true,
            new Color(0.03f, 0.05f, 0.11f), 0.02f);

        [Test]
        public void New_PassesBaseThrough()
        {
            var overlay = new LightingOverlay();

            Assert.IsFalse(overlay.IsActive);
            AssertSame(Night, overlay.Apply(Night));
        }

        [Test]
        public void Fog_FadesInToModifier_ThenOut()
        {
            var overlay = new LightingOverlay();
            overlay.Begin(Modifier(fog: true), 2f);

            Assert.IsTrue(overlay.Tick(1f));
            Assert.AreEqual(0.5f, overlay.Weight, Tolerance);
            Assert.AreEqual(Mathf.Lerp(0.02f, 0.035f, 0.5f), overlay.Apply(Night).FogDensity, Tolerance);

            overlay.Tick(1f);
            LightingValues full = overlay.Apply(Night);
            Assert.AreEqual(0.035f, full.FogDensity, Tolerance);
            Assert.AreEqual(0.14f, full.FogColor.r, Tolerance);
            Assert.AreEqual(Night.SunColor, full.SunColor, "Sis ışık rengine dokunmaz.");
            Assert.IsFalse(overlay.Tick(1f), "Tam ağırlıkta değişiklik yok.");

            overlay.End(2f);
            overlay.Tick(2f);
            Assert.AreEqual(0f, overlay.Weight, Tolerance);
            AssertSame(Night, overlay.Apply(Night));
            Assert.IsFalse(overlay.IsActive);
        }

        [Test]
        public void Light_TintsMoonAndAmbient_ZeroFadeIsInstant()
        {
            var overlay = new LightingOverlay();
            overlay.Begin(Modifier(light: true), 0f);
            overlay.Tick(0.01f);

            LightingValues red = overlay.Apply(Night);

            Assert.AreEqual(0.95f, red.SunColor.r, Tolerance);
            Assert.AreEqual(0.2f, red.SunColor.g, Tolerance);
            Assert.AreEqual(0.32f, red.AmbientColor.r, Tolerance);
            Assert.AreEqual(Night.FogDensity, red.FogDensity, Tolerance, "Kan Ayı sise dokunmaz.");
        }

        [Test]
        public void ModifierWithoutLighting_HasNoLayer()
        {
            var overlay = new LightingOverlay();
            overlay.Begin(Modifier(), 1f);

            Assert.IsFalse(overlay.IsActive, "Fırtına sis/renk değiştirmez (sadece şimşek).");
        }

        [Test]
        public void Flash_BrightensThenFades()
        {
            var overlay = new LightingOverlay();
            overlay.Flash(0.3f, 3f, Color.white, new Color(0.5f, 0.5f, 0.5f));

            Assert.IsTrue(overlay.IsActive);
            Assert.AreEqual(3.5f, overlay.Apply(Night).SunIntensity, Tolerance, "Tepede ay ışığı + 3.");
            Assert.AreEqual(Color.white, overlay.Apply(Night).SunColor);

            overlay.Tick(0.15f);
            float mid = overlay.Apply(Night).SunIntensity;
            Assert.AreEqual(0.5f + 3f * 0.25f, mid, Tolerance, "(1 − t)²: yarıda %25.");

            Assert.IsTrue(overlay.Tick(0.2f));
            Assert.IsFalse(overlay.IsFlashing);
            Assert.AreEqual(0.5f, overlay.Apply(Night).SunIntensity, Tolerance);
        }

        private static NightModifierSettings Modifier(bool fog = false, bool light = false)
        {
            return new NightModifierSettings(
                "test", "Test", string.Empty, 1f, 1f, 1f,
                fog, new Color(0.14f, 0.16f, 0.2f), 0.035f,
                light, new Color(0.95f, 0.2f, 0.14f), new Color(0.32f, 0.08f, 0.08f),
                false, 25f, 5f);
        }

        private static void AssertSame(in LightingValues expected, in LightingValues actual)
        {
            Assert.AreEqual(expected.SunColor, actual.SunColor);
            Assert.AreEqual(expected.SunIntensity, actual.SunIntensity, Tolerance);
            Assert.AreEqual(expected.AmbientColor, actual.AmbientColor);
            Assert.AreEqual(expected.FogEnabled, actual.FogEnabled);
            Assert.AreEqual(expected.FogDensity, actual.FogDensity, Tolerance);
            Assert.AreEqual(expected.FogColor, actual.FogColor);
        }
    }
}
