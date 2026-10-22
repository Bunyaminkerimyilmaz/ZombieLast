using System;
using LastHaven.Core;

namespace LastHaven.GameFlow
{
    /// <summary>
    /// Ayarların saf durumu ve kaydı: oluşturulurken kayıttan yükler (eksik veya geçersiz değer → varsayılan), <see cref="Apply"/>
    /// değişen ayarı kayda yazar ve kirli işaretler, <see cref="Save"/> kirliyse kaydı diske yazdırır. Anahtarlar
    /// <see cref="KeyPrefix"/> ile başlar (ordinal).
    /// </summary>
    public sealed class SettingsModel
    {
        public const string KeyPrefix = "LastHaven.Settings.";
        public const string MasterVolumeKey = KeyPrefix + "MasterVolume";
        public const string MusicVolumeKey = KeyPrefix + "MusicVolume";
        public const string SfxVolumeKey = KeyPrefix + "SfxVolume";
        public const string CameraPresetKey = KeyPrefix + "CameraPreset";
        public const string ScreenShakeKey = KeyPrefix + "ScreenShake";
        public const string FullscreenKey = KeyPrefix + "Fullscreen";

        private readonly ISettingsStore _store;

        public SettingsModel(ISettingsStore store, GameSettings defaults)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            Defaults = defaults;
            Current = Load(store, defaults);
        }

        public GameSettings Defaults { get; }

        public GameSettings Current { get; private set; }

        /// <summary>Kayda yazılmış ama diske henüz yazılmamış değişiklik var mı.</summary>
        public bool IsDirty { get; private set; }

        /// <summary>Farklıysa ayarları değiştirip kayda yazar; değiştiyse true.</summary>
        public bool Apply(GameSettings settings)
        {
            if (settings == Current)
            {
                return false;
            }

            Current = settings;
            Write(_store, settings);
            IsDirty = true;
            return true;
        }

        /// <summary>Kirliyse diske yazar; yazdıysa true.</summary>
        public bool Save()
        {
            if (!IsDirty)
            {
                return false;
            }

            _store.Save();
            IsDirty = false;
            return true;
        }

        /// <summary>Kayıttaki ayarlar; olmayan anahtar ve tanımsız kamera preset'i varsayılandan, ses düzeyleri 0..1'e kırpılır.</summary>
        public static GameSettings Load(ISettingsStore store, GameSettings defaults)
        {
            float master = store.TryGetFloat(MasterVolumeKey, out float storedMaster) ? storedMaster : defaults.MasterVolume;
            float music = store.TryGetFloat(MusicVolumeKey, out float storedMusic) ? storedMusic : defaults.MusicVolume;
            float sfx = store.TryGetFloat(SfxVolumeKey, out float storedSfx) ? storedSfx : defaults.SfxVolume;
            CameraPreset preset = store.TryGetInt(CameraPresetKey, out int storedPreset) && Enum.IsDefined(typeof(CameraPreset), storedPreset)
                ? (CameraPreset)storedPreset
                : defaults.CameraPreset;
            bool shake = store.TryGetInt(ScreenShakeKey, out int storedShake) ? storedShake != 0 : defaults.ScreenShake;
            bool fullscreen = store.TryGetInt(FullscreenKey, out int storedFullscreen) ? storedFullscreen != 0 : defaults.Fullscreen;
            return new GameSettings(master, music, sfx, preset, shake, fullscreen);
        }

        /// <summary>Tüm ayarları kayda yazar (diske yazmaz).</summary>
        public static void Write(ISettingsStore store, GameSettings settings)
        {
            store.SetFloat(MasterVolumeKey, settings.MasterVolume);
            store.SetFloat(MusicVolumeKey, settings.MusicVolume);
            store.SetFloat(SfxVolumeKey, settings.SfxVolume);
            store.SetInt(CameraPresetKey, (int)settings.CameraPreset);
            store.SetInt(ScreenShakeKey, settings.ScreenShake ? 1 : 0);
            store.SetInt(FullscreenKey, settings.Fullscreen ? 1 : 0);
        }
    }
}
