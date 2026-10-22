using LastHaven.Core;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>Oyuncu ayarları değeri (14.3): ses düzeyleri 0..1'e kırpılır, With* tek alanı değiştirir, eşitlik değer bazlı.</summary>
    public class GameSettingsTests
    {
        private static GameSettings Sample() => new GameSettings(0.8f, 0.6f, 0.4f, CameraPreset.Isometric, true, false);

        [Test]
        public void Constructor_ClampsVolumes_NaNIsZero()
        {
            var settings = new GameSettings(1.5f, -0.2f, float.NaN, CameraPreset.TopDown, false, true);

            Assert.AreEqual(1f, settings.MasterVolume);
            Assert.AreEqual(0f, settings.MusicVolume);
            Assert.AreEqual(0f, settings.SfxVolume);
        }

        [Test]
        public void With_ChangesOnlyThatField()
        {
            GameSettings settings = Sample();

            GameSettings changed = settings.WithMusicVolume(0.1f).WithCameraPreset(CameraPreset.TopDown).WithFullscreen(true);

            Assert.AreEqual(0.8f, changed.MasterVolume);
            Assert.AreEqual(0.1f, changed.MusicVolume, 1e-6f);
            Assert.AreEqual(0.4f, changed.SfxVolume);
            Assert.AreEqual(CameraPreset.TopDown, changed.CameraPreset);
            Assert.IsTrue(changed.ScreenShake);
            Assert.IsTrue(changed.Fullscreen);
            Assert.AreEqual(0.6f, settings.MusicVolume, "Değişmez değer: asıl değişmez.");
        }

        [Test]
        public void With_ClampsVolume()
        {
            Assert.AreEqual(1f, Sample().WithSfxVolume(3f).SfxVolume);
            Assert.AreEqual(0f, Sample().WithMasterVolume(-1f).MasterVolume);
        }

        [Test]
        public void Equality_IsByValue()
        {
            Assert.IsTrue(Sample() == Sample());
            Assert.AreEqual(Sample().GetHashCode(), Sample().GetHashCode());
            Assert.IsTrue(Sample() != Sample().WithScreenShake(false));
            Assert.IsFalse(Sample().Equals(Sample().WithSfxVolume(0.41f)));
        }
    }
}
