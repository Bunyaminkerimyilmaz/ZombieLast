namespace LastHaven.DebugTools
{
    /// <summary>
    /// Kare başı GC ayırma özeti (saf C#): kare sayısı, toplam ve en büyük ayırma, ayırma yapan kare sayısı. Debug paneli her
    /// yenilemede sıfırlar (son yarım saniye), stres testi baştan sona tutar.
    /// </summary>
    public sealed class AllocationWindow
    {
        public int Frames { get; private set; }

        /// <summary>En az bir bayt ayıran kareler.</summary>
        public int AllocatingFrames { get; private set; }

        public long TotalBytes { get; private set; }

        public long MaxBytes { get; private set; }

        /// <summary>Kare başı ortalama bayt; kare yoksa 0.</summary>
        public double AverageBytes => Frames > 0 ? (double)TotalBytes / Frames : 0d;

        /// <summary>Ayırma yapan karelerin oranı (0..1); kare yoksa 0.</summary>
        public double AllocatingRatio => Frames > 0 ? (double)AllocatingFrames / Frames : 0d;

        /// <summary>Bir karenin ayırmasını ekler (negatif 0 sayılır).</summary>
        public void Add(long bytes)
        {
            if (bytes < 0)
            {
                bytes = 0;
            }

            Frames++;
            TotalBytes += bytes;
            if (bytes > 0)
            {
                AllocatingFrames++;
            }

            if (bytes > MaxBytes)
            {
                MaxBytes = bytes;
            }
        }

        public void Reset()
        {
            Frames = 0;
            AllocatingFrames = 0;
            TotalBytes = 0;
            MaxBytes = 0;
        }
    }
}
