using LastHaven.GameFlow;
using NUnit.Framework;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>LightingValues.Lerp: uçlarda değerler aynen, ortada harmanlanmış; kapalı sis yoğunluğu 0'dan kayar.</summary>
    public class LightingValuesTests
    {
        private const float Tolerance = 0.0001f;

        private static readonly LightingValues Day = new LightingValues(
            Quaternion.Euler(50f, -30f, 0f), Color.white, 1.2f, new Color(0.5f, 0.5f, 0.5f), true, Color.gray, 0.006f);

        private static readonly LightingValues Night = new LightingValues(
            Quaternion.Euler(60f, 30f, 0f), Color.blue, 0.4f, new Color(0.1f, 0.1f, 0.3f), true, Color.black, 0.02f);

        [Test]
        public void Lerp_AtEnds_ReturnsInputs()
        {
            AssertSame(Day, LightingValues.Lerp(Day, Night, 0f));
            AssertSame(Night, LightingValues.Lerp(Day, Night, 1f));
            AssertSame(Day, LightingValues.Lerp(Day, Night, -1f));
            AssertSame(Night, LightingValues.Lerp(Day, Night, 2f));
        }

        [Test]
        public void Lerp_Midway_BlendsEverything()
        {
            LightingValues mid = LightingValues.Lerp(Day, Night, 0.5f);

            Assert.AreEqual(0.8f, mid.SunIntensity, Tolerance);
            Assert.AreEqual(Color.Lerp(Color.white, Color.blue, 0.5f), mid.SunColor);
            Assert.AreEqual(0.3f, mid.AmbientColor.r, Tolerance);
            Assert.AreEqual(0.4f, mid.AmbientColor.b, Tolerance);
            Assert.IsTrue(mid.FogEnabled);
            Assert.AreEqual(0.013f, mid.FogDensity, Tolerance);
            Assert.AreEqual(Color.Lerp(Color.gray, Color.black, 0.5f), mid.FogColor);

            float toDay = Quaternion.Angle(Day.SunRotation, mid.SunRotation);
            float toNight = Quaternion.Angle(Night.SunRotation, mid.SunRotation);
            Assert.AreEqual(toDay, toNight, 0.01f, "Güneş iki yönün tam ortasında.");
        }

        [Test]
        public void Lerp_FogTurningOn_FadesDensityFromZeroWithTargetColor()
        {
            LightingValues clear = WithFog(Day, false, Color.red, 0.5f);

            LightingValues quarter = LightingValues.Lerp(clear, Night, 0.25f);

            Assert.IsTrue(quarter.FogEnabled);
            Assert.AreEqual(0.005f, quarter.FogDensity, Tolerance, "Kapalı sisin yoğunluğu 0 sayılır.");
            Assert.AreEqual(Night.FogColor, quarter.FogColor, "Açık olan tarafın rengi kullanılır.");
        }

        [Test]
        public void Lerp_FogTurningOff_KeepsSourceColor()
        {
            LightingValues clear = WithFog(Night, false, Color.red, 0.5f);

            LightingValues mid = LightingValues.Lerp(Day, clear, 0.5f);

            Assert.AreEqual(0.003f, mid.FogDensity, Tolerance);
            Assert.AreEqual(Day.FogColor, mid.FogColor);
        }

        [Test]
        public void Lerp_BothFogOff_StaysOff()
        {
            LightingValues a = WithFog(Day, false, Color.red, 0.5f);
            LightingValues b = WithFog(Night, false, Color.green, 0.5f);

            LightingValues mid = LightingValues.Lerp(a, b, 0.5f);

            Assert.IsFalse(mid.FogEnabled);
            Assert.AreEqual(0f, mid.FogDensity);
        }

        private static LightingValues WithFog(in LightingValues source, bool enabled, Color color, float density)
        {
            return new LightingValues(
                source.SunRotation, source.SunColor, source.SunIntensity, source.AmbientColor, enabled, color, density);
        }

        private static void AssertSame(in LightingValues expected, in LightingValues actual)
        {
            Assert.AreEqual(0f, Quaternion.Angle(expected.SunRotation, actual.SunRotation), 0.01f);
            Assert.AreEqual(expected.SunColor, actual.SunColor);
            Assert.AreEqual(expected.SunIntensity, actual.SunIntensity, Tolerance);
            Assert.AreEqual(expected.AmbientColor, actual.AmbientColor);
            Assert.AreEqual(expected.FogEnabled, actual.FogEnabled);
            Assert.AreEqual(expected.FogColor, actual.FogColor);
            Assert.AreEqual(expected.FogDensity, actual.FogDensity, Tolerance);
        }
    }
}
