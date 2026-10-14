using System;

namespace LastHaven.Core
{
    /// <summary>Kamera açısı preset'leri (GDD §4). Ayarlardan seçilir.</summary>
    public enum CameraPreset
    {
        Isometric,
        TopDown,
    }

    public static class CameraPresets
    {
        private static readonly int Count = Enum.GetValues(typeof(CameraPreset)).Length;

        /// <summary>Sıradaki preset; sonuncudan sonra ilkine döner.</summary>
        public static CameraPreset Next(CameraPreset preset)
        {
            return (CameraPreset)(((int)preset + 1) % Count);
        }
    }
}
