using System.Collections.Generic;
using LastHaven.Core;
using LastHaven.GameFlow;
using NUnit.Framework;

namespace LastHaven.Tests.EditMode
{
    /// <summary>
    /// Ayarların saf durumu ve kaydı (14.3): boş kayıtta varsayılanlar, kayda yazma / geri okuma, bozuk değerler, kirli işareti ve
    /// diske yazma. Gerçek PlayerPrefs'e dokunmaz (bellek içi kayıt).
    /// </summary>
    public class SettingsModelTests
    {
        private static readonly GameSettings Defaults = new GameSettings(1f, 0.8f, 0.9f, CameraPreset.Isometric, true, true);

        private sealed class MemoryStore : ISettingsStore
        {
            public readonly Dictionary<string, float> Floats = new Dictionary<string, float>();
            public readonly Dictionary<string, int> Ints = new Dictionary<string, int>();
            public int SaveCount;
            public int WriteCount;

            public bool TryGetFloat(string key, out float value) => Floats.TryGetValue(key, out value);

            public bool TryGetInt(string key, out int value) => Ints.TryGetValue(key, out value);

            public void SetFloat(string key, float value)
            {
                Floats[key] = value;
                WriteCount++;
            }

            public void SetInt(string key, int value)
            {
                Ints[key] = value;
                WriteCount++;
            }

            public void Save() => SaveCount++;
        }

        [Test]
        public void EmptyStore_UsesDefaults_NotDirty()
        {
            var model = new SettingsModel(new MemoryStore(), Defaults);

            Assert.AreEqual(Defaults, model.Current);
            Assert.IsFalse(model.IsDirty);
        }

        [Test]
        public void Write_ThenLoad_RoundTrips()
        {
            var store = new MemoryStore();
            var settings = new GameSettings(0.25f, 0f, 0.5f, CameraPreset.TopDown, false, false);

            SettingsModel.Write(store, settings);

            Assert.AreEqual(settings, SettingsModel.Load(store, Defaults));
        }

        [Test]
        public void Load_MissingKeysFallBackToDefaultsIndividually()
        {
            var store = new MemoryStore();
            store.Floats[SettingsModel.MusicVolumeKey] = 0.3f;
            store.Ints[SettingsModel.ScreenShakeKey] = 0;

            GameSettings loaded = SettingsModel.Load(store, Defaults);

            Assert.AreEqual(0.3f, loaded.MusicVolume);
            Assert.IsFalse(loaded.ScreenShake);
            Assert.AreEqual(Defaults.MasterVolume, loaded.MasterVolume);
            Assert.AreEqual(Defaults.CameraPreset, loaded.CameraPreset);
            Assert.AreEqual(Defaults.Fullscreen, loaded.Fullscreen);
        }

        [Test]
        public void Load_InvalidValues_AreClampedOrDefaulted()
        {
            var store = new MemoryStore();
            store.Floats[SettingsModel.MasterVolumeKey] = 7f;
            store.Floats[SettingsModel.SfxVolumeKey] = -3f;
            store.Ints[SettingsModel.CameraPresetKey] = 99;

            GameSettings loaded = SettingsModel.Load(store, Defaults);

            Assert.AreEqual(1f, loaded.MasterVolume);
            Assert.AreEqual(0f, loaded.SfxVolume);
            Assert.AreEqual(Defaults.CameraPreset, loaded.CameraPreset, "Tanımsız preset varsayılana döner.");
        }

        [Test]
        public void Apply_Same_DoesNothing()
        {
            var store = new MemoryStore();
            var model = new SettingsModel(store, Defaults);

            Assert.IsFalse(model.Apply(Defaults));
            Assert.IsFalse(model.IsDirty);
            Assert.AreEqual(0, store.WriteCount);
        }

        [Test]
        public void Apply_Different_WritesStore_MarksDirty_SaveOnce()
        {
            var store = new MemoryStore();
            var model = new SettingsModel(store, Defaults);
            GameSettings changed = Defaults.WithSfxVolume(0.2f);

            Assert.IsTrue(model.Apply(changed));
            Assert.AreEqual(changed, model.Current);
            Assert.IsTrue(model.IsDirty);
            Assert.AreEqual(0.2f, store.Floats[SettingsModel.SfxVolumeKey], 1e-6f);
            Assert.AreEqual(0, store.SaveCount, "Apply diske yazmaz.");

            Assert.IsTrue(model.Save());
            Assert.IsFalse(model.Save(), "Değişiklik yokken ikinci Save etkisiz.");
            Assert.AreEqual(1, store.SaveCount);
            Assert.IsFalse(model.IsDirty);
        }

        [Test]
        public void ReopenedModel_ReadsSavedSettings()
        {
            var store = new MemoryStore();
            var first = new SettingsModel(store, Defaults);
            first.Apply(Defaults.WithCameraPreset(CameraPreset.TopDown).WithMasterVolume(0.5f));
            first.Save();

            var second = new SettingsModel(store, Defaults);

            Assert.AreEqual(CameraPreset.TopDown, second.Current.CameraPreset);
            Assert.AreEqual(0.5f, second.Current.MasterVolume);
        }

        [Test]
        public void Keys_SharePrefix()
        {
            foreach (string key in new[]
                     {
                         SettingsModel.MasterVolumeKey, SettingsModel.MusicVolumeKey, SettingsModel.SfxVolumeKey,
                         SettingsModel.CameraPresetKey, SettingsModel.ScreenShakeKey, SettingsModel.FullscreenKey,
                     })
            {
                StringAssert.StartsWith(SettingsModel.KeyPrefix, key);
            }
        }
    }
}
