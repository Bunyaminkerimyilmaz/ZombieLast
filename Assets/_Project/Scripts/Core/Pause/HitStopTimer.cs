namespace LastHaven.Core
{
    /// <summary>
    /// Hit-stop sayacı (saf C#; <see cref="IHitStopService"/>'in mantığı). Çakışan istekler kalan süreyi uzatır, kısaltmaz.
    /// Köprü gerçek zamanla (<c>Time.unscaledDeltaTime</c>) ilerletir; <see cref="Request"/> / <see cref="Tick"/> donma durumunu
    /// değiştirdiyse true döner, köprü sadece o zaman <c>Time.timeScale</c>'e yazar.
    /// </summary>
    public sealed class HitStopTimer
    {
        private readonly Countdown _countdown = new Countdown();

        public bool IsActive => _countdown.IsRunning;

        /// <summary>Kalan donma süresi (sn); donma yoksa 0.</summary>
        public float Remaining => _countdown.Remaining;

        /// <summary>Donma ister. Donma bu çağrıyla başladıysa true; süre ≤ 0 ise veya donma zaten sürüyorsa false.</summary>
        public bool Request(float duration)
        {
            if (!(duration > 0f))
            {
                return false;
            }

            bool started = !IsActive;
            if (started || duration > Remaining)
            {
                _countdown.Start(duration);
            }

            return started;
        }

        /// <summary>Gerçek zamanla ilerletir. Donma bu çağrıda bittiyse true (bir kez).</summary>
        public bool Tick(float unscaledDeltaTime)
        {
            return _countdown.Tick(unscaledDeltaTime);
        }

        /// <summary>Donmayı bitirmeden kaldırır (ör. duraklatma devraldı); <see cref="Tick"/> true dönmez.</summary>
        public void Cancel()
        {
            _countdown.Stop();
        }
    }
}
