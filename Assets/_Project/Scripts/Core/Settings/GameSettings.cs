using System;
using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>
    /// Oyuncu ayarları (GDD §16): ses düzeyleri (genel / müzik / efekt, 0..1 doğrusal), kamera preset'i, ekran sarsıntısı ve tam ekran.
    /// Değişmez değer; ses düzeyleri constructor'da 0..1'e kırpılır. Değiştirmek için <c>WithXxx</c> ile yeni değer üret ve
    /// <see cref="ISettingsService.Apply"/>'a ver.
    /// </summary>
    public readonly struct GameSettings : IEquatable<GameSettings>
    {
        public GameSettings(float masterVolume, float musicVolume, float sfxVolume, CameraPreset cameraPreset, bool screenShake,
            bool fullscreen)
        {
            MasterVolume = ClampVolume(masterVolume);
            MusicVolume = ClampVolume(musicVolume);
            SfxVolume = ClampVolume(sfxVolume);
            CameraPreset = cameraPreset;
            ScreenShake = screenShake;
            Fullscreen = fullscreen;
        }

        /// <summary>Genel ses (0..1); müzik ve efektlerin üstündeki Master grubu.</summary>
        public float MasterVolume { get; }

        public float MusicVolume { get; }

        /// <summary>Efekt ve arayüz sesleri (0..1).</summary>
        public float SfxVolume { get; }

        public CameraPreset CameraPreset { get; }

        public bool ScreenShake { get; }

        public bool Fullscreen { get; }

        public GameSettings WithMasterVolume(float volume) =>
            new GameSettings(volume, MusicVolume, SfxVolume, CameraPreset, ScreenShake, Fullscreen);

        public GameSettings WithMusicVolume(float volume) =>
            new GameSettings(MasterVolume, volume, SfxVolume, CameraPreset, ScreenShake, Fullscreen);

        public GameSettings WithSfxVolume(float volume) =>
            new GameSettings(MasterVolume, MusicVolume, volume, CameraPreset, ScreenShake, Fullscreen);

        public GameSettings WithCameraPreset(CameraPreset preset) =>
            new GameSettings(MasterVolume, MusicVolume, SfxVolume, preset, ScreenShake, Fullscreen);

        public GameSettings WithScreenShake(bool enabled) =>
            new GameSettings(MasterVolume, MusicVolume, SfxVolume, CameraPreset, enabled, Fullscreen);

        public GameSettings WithFullscreen(bool fullscreen) =>
            new GameSettings(MasterVolume, MusicVolume, SfxVolume, CameraPreset, ScreenShake, fullscreen);

        /// <summary>0..1'e kırpar; NaN 0 sayılır.</summary>
        public static float ClampVolume(float volume)
        {
            return float.IsNaN(volume) ? 0f : Mathf.Clamp01(volume);
        }

        public bool Equals(GameSettings other)
        {
            return MasterVolume.Equals(other.MasterVolume) && MusicVolume.Equals(other.MusicVolume) && SfxVolume.Equals(other.SfxVolume)
                   && CameraPreset == other.CameraPreset && ScreenShake == other.ScreenShake && Fullscreen == other.Fullscreen;
        }

        public override bool Equals(object obj)
        {
            return obj is GameSettings other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(MasterVolume, MusicVolume, SfxVolume, (int)CameraPreset, ScreenShake, Fullscreen);
        }

        public static bool operator ==(GameSettings left, GameSettings right) => left.Equals(right);

        public static bool operator !=(GameSettings left, GameSettings right) => !left.Equals(right);

        public override string ToString()
        {
            return $"genel {MasterVolume:0.00}, müzik {MusicVolume:0.00}, efekt {SfxVolume:0.00}, kamera {CameraPreset}, " +
                   $"sarsıntı {(ScreenShake ? "açık" : "kapalı")}, tam ekran {(Fullscreen ? "açık" : "kapalı")}";
        }
    }
}
