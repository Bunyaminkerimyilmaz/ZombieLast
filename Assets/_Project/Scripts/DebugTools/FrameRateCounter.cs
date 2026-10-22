using System;

namespace LastHaven.DebugTools
{
    /// <summary>
    /// FPS ölçer (saf C#): gerçek zamanlı kare süreleri bir pencere boyunca toplanır, pencere dolunca ortalama yazılır. Tek karelik
    /// sıçramalar göstergeyi titretmez; zaman ölçeğinden (duraklatma, debug hızı) etkilenmez.
    /// </summary>
    public sealed class FrameRateCounter
    {
        private readonly float _window;
        private int _frames;
        private float _elapsed;

        /// <param name="window">Ölçüm penceresi (sn, gerçek zaman); çok küçükse 0.01 sayılır.</param>
        public FrameRateCounter(float window)
        {
            _window = Math.Max(0.01f, window);
        }

        /// <summary>Son pencerenin ortalama kare hızı; ilk pencere dolana kadar 0.</summary>
        public float Fps { get; private set; }

        /// <summary>En az bir pencere dolduysa true.</summary>
        public bool HasSample { get; private set; }

        /// <summary>Bir kare ekler. Pencere dolup yeni ortalama yazıldıysa true. Süre ≤ 0 olan kare sayılmaz.</summary>
        public bool Tick(float unscaledDeltaTime)
        {
            if (!(unscaledDeltaTime > 0f))
            {
                return false;
            }

            _frames++;
            _elapsed += unscaledDeltaTime;
            if (_elapsed < _window)
            {
                return false;
            }

            Fps = _frames / _elapsed;
            HasSample = true;
            _frames = 0;
            _elapsed = 0f;
            return true;
        }

        /// <summary>Ölçümü baştan başlatır (ör. panel yeniden açılınca).</summary>
        public void Reset()
        {
            _frames = 0;
            _elapsed = 0f;
            Fps = 0f;
            HasSample = false;
        }
    }
}
