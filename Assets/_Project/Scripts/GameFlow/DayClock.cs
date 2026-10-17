using System;

namespace LastHaven.GameFlow
{
    /// <summary>
    /// Gündüz sayacının bildirim kuralları (saf C#; <see cref="GameStateMachine"/> gündüz evresinde kullanır): kalan sürenin tam
    /// saniyesi (yukarı yuvarlı) değişince bir kez, son uyarı süresine inince gündüz başına bir kez.
    /// </summary>
    internal sealed class DayClock
    {
        private readonly float _finalWarningDuration;
        private int _seconds;
        private bool _warned;

        /// <param name="finalWarningDuration">Son uyarı süresi (sn); 0 veya altı: uyarı yok.</param>
        public DayClock(float finalWarningDuration)
        {
            _finalWarningDuration = finalWarningDuration;
        }

        /// <summary>Yeni gündüz: uyarıyı sıfırlar ve başlangıçtaki tam saniyeyi döndürür (her zaman bildirilir).</summary>
        public int Restart(float remaining)
        {
            _warned = false;
            _seconds = WholeSeconds(remaining);
            return _seconds;
        }

        /// <summary>Tam saniye son bildirilenden farklıysa yeni değeri verir.</summary>
        public bool TryAdvanceSecond(float remaining, out int seconds)
        {
            seconds = WholeSeconds(remaining);
            if (seconds == _seconds)
            {
                return false;
            }

            _seconds = seconds;
            return true;
        }

        /// <summary>Kalan süre son uyarı süresine indiyse ve bu gündüz henüz uyarılmadıysa true (bir kez).</summary>
        public bool TryWarn(float remaining)
        {
            if (_warned || _finalWarningDuration <= 0f || remaining > _finalWarningDuration)
            {
                return false;
            }

            _warned = true;
            return true;
        }

        private static int WholeSeconds(float seconds)
        {
            return (int)Math.Ceiling(seconds);
        }
    }
}
