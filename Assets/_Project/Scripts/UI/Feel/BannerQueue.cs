using System;
using System.Collections.Generic;

namespace LastHaven.UI
{
    /// <summary>
    /// Ekran ortasındaki büyük başlıkların sırası (saf C#; <see cref="HudBannerController"/>'ın mantığı). Başlık yokken gelen hemen
    /// gösterilir; biri gösterilirken gelenler sırada bekler (sıra doluysa yenisi atılır). Her başlık belirir → durur → söner
    /// (<see cref="BannerTiming"/>); bitince sıradaki başlar.
    /// </summary>
    public sealed class BannerQueue<T>
    {
        private readonly Queue<T> _waiting = new Queue<T>();
        private readonly BannerTiming _timing;
        private readonly int _limit;

        public BannerQueue(BannerTiming timing, int queueLimit)
        {
            _timing = timing;
            _limit = Math.Max(0, queueLimit);
        }

        public bool IsShowing { get; private set; }

        /// <summary>Gösterilen başlık; <see cref="IsShowing"/> false ise anlamsız.</summary>
        public T Current { get; private set; }

        /// <summary>Gösterilen başlığın yaşı (sn).</summary>
        public float Age { get; private set; }

        public int WaitingCount => _waiting.Count;

        /// <summary>Başlık ekler. Gösterilen başlık değiştiyse (hemen başladı) true; sıraya girdiyse veya atıldıysa false.</summary>
        public bool Enqueue(T banner)
        {
            if (!IsShowing)
            {
                Begin(banner);
                return true;
            }

            if (_waiting.Count < _limit)
            {
                _waiting.Enqueue(banner);
            }

            return false;
        }

        /// <summary>İlerletir. Gösterilen başlık değiştiyse (bitti / sıradaki başladı) true.</summary>
        public bool Tick(float deltaTime)
        {
            if (!IsShowing)
            {
                return false;
            }

            Age += deltaTime;
            if (Age < _timing.Total)
            {
                return false;
            }

            if (_waiting.Count > 0)
            {
                Begin(_waiting.Dequeue());
            }
            else
            {
                IsShowing = false;
                Current = default;
            }

            return true;
        }

        /// <summary>Gösterilen başlığın görünümü.</summary>
        public BannerPose Evaluate()
        {
            return IsShowing ? _timing.Evaluate(Age) : new BannerPose(0f, 1f);
        }

        /// <summary>Gösterileni ve sırayı siler.</summary>
        public void Clear()
        {
            _waiting.Clear();
            IsShowing = false;
            Current = default;
            Age = 0f;
        }

        private void Begin(T banner)
        {
            Current = banner;
            Age = 0f;
            IsShowing = true;
        }
    }

    /// <summary>Bir başlığın zamanlaması: belirme (büyükten normale iner, opaklık 0 → 1), durma, sönme (1 → 0).</summary>
    public readonly struct BannerTiming
    {
        public BannerTiming(float fadeIn, float hold, float fadeOut, float startScale)
        {
            FadeIn = Math.Max(0f, fadeIn);
            Hold = Math.Max(0f, hold);
            FadeOut = Math.Max(0f, fadeOut);
            StartScale = Math.Max(1f, startScale);
        }

        public float FadeIn { get; }

        public float Hold { get; }

        public float FadeOut { get; }

        public float StartScale { get; }

        public float Total => FadeIn + Hold + FadeOut;

        public BannerPose Evaluate(float age)
        {
            if (age < FadeIn)
            {
                float t = age / FadeIn;
                float eased = 1f - (1f - t) * (1f - t);
                return new BannerPose(t, StartScale + (1f - StartScale) * eased);
            }

            float fadeOutStart = FadeIn + Hold;
            if (age < fadeOutStart)
            {
                return new BannerPose(1f, 1f);
            }

            if (FadeOut <= 0f || age >= Total)
            {
                return new BannerPose(0f, 1f);
            }

            return new BannerPose(1f - (age - fadeOutStart) / FadeOut, 1f);
        }
    }

    /// <summary>Başlığın bir anki görünümü.</summary>
    public readonly struct BannerPose
    {
        public BannerPose(float opacity, float scale)
        {
            Opacity = opacity;
            Scale = scale;
        }

        public float Opacity { get; }

        public float Scale { get; }
    }
}
