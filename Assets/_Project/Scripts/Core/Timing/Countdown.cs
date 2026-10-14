using System;

namespace LastHaven.Core
{
    /// <summary>
    /// Saf C# geri sayım (doldurma, yeniden doğma gibi süreli işler). Sadece <see cref="Tick"/> ile ilerler.
    /// </summary>
    public sealed class Countdown
    {
        /// <summary>Son <see cref="Start"/>'ta verilen süre (sn).</summary>
        public float Duration { get; private set; }

        /// <summary>Kalan süre (sn); çalışmıyorsa 0.</summary>
        public float Remaining { get; private set; }

        public bool IsRunning { get; private set; }

        /// <summary>Çalışırken 0 → 1 ilerleme. Süre 0 ise 1.</summary>
        public float Progress => Duration > 0f ? 1f - Remaining / Duration : 1f;

        /// <summary>Sayımı baştan başlatır. Negatif süre 0 sayılır (ilk Tick'te biter).</summary>
        public void Start(float duration)
        {
            Duration = Math.Max(0f, duration);
            Remaining = Duration;
            IsRunning = true;
        }

        /// <summary>Sayımı bitirmeden durdurur; <see cref="Tick"/> true dönmez.</summary>
        public void Stop()
        {
            IsRunning = false;
            Remaining = 0f;
        }

        /// <summary>Süreyi ilerletir. Sayım bu çağrıda bittiyse true döner (bir kez).</summary>
        public bool Tick(float deltaTime)
        {
            if (!IsRunning)
            {
                return false;
            }

            Remaining -= deltaTime;
            if (Remaining > 0f)
            {
                return false;
            }

            Remaining = 0f;
            IsRunning = false;
            return true;
        }
    }
}
