using System.Collections.Generic;
using UnityEngine;

namespace LastHaven.Audio
{
    /// <summary>
    /// Ses çalma sınırları (saf C#): ses başına aynı anda en fazla kopya ve iki çalış arası bekleme, tüm sesler için toplam kaynak
    /// sınırı. Sesler referansla anahtarlanır (<c>SoundData</c>). Zaman çağırandan gelir (gerçek zaman). Çalış başlayınca
    /// <see cref="Started"/>, bitince <see cref="Finished"/> çağrılır.
    /// </summary>
    public sealed class SoundGate
    {
        private readonly Dictionary<object, SoundState> _states = new Dictionary<object, SoundState>();
        private readonly int _maxVoices;

        public SoundGate(int maxVoices)
        {
            _maxVoices = Mathf.Max(1, maxVoices);
        }

        /// <summary>Şu an çalan toplam kopya.</summary>
        public int ActiveCount { get; private set; }

        public int MaxVoices => _maxVoices;

        /// <summary>
        /// Ses şimdi çalabilir mi: toplam sınır dolu değil, bu sesin kopyaları <paramref name="maxSimultaneous"/>'tan az (0 = sınırsız)
        /// ve son başlangıçtan beri en az <paramref name="cooldown"/> geçti (0 = yok). Durumu değiştirmez.
        /// </summary>
        public bool CanPlay(object sound, int maxSimultaneous, float cooldown, float now)
        {
            if (sound == null || ActiveCount >= _maxVoices)
            {
                return false;
            }

            if (!_states.TryGetValue(sound, out SoundState state))
            {
                return true;
            }

            if (maxSimultaneous > 0 && state.Active >= maxSimultaneous)
            {
                return false;
            }

            return cooldown <= 0f || now - state.LastStart >= cooldown;
        }

        /// <summary>Bir kopya çalmaya başladı.</summary>
        public void Started(object sound, float now)
        {
            if (sound == null)
            {
                return;
            }

            if (!_states.TryGetValue(sound, out SoundState state))
            {
                state = new SoundState();
                _states.Add(sound, state);
            }

            state.Active++;
            state.LastStart = now;
            ActiveCount++;
        }

        /// <summary>Bir kopya bitti (başlamamış sesin bitişi yok sayılır).</summary>
        public void Finished(object sound)
        {
            if (sound == null || !_states.TryGetValue(sound, out SoundState state) || state.Active <= 0)
            {
                return;
            }

            state.Active--;
            ActiveCount--;
        }

        /// <summary>Bu sesin şu an çalan kopya sayısı.</summary>
        public int GetActive(object sound)
        {
            return sound != null && _states.TryGetValue(sound, out SoundState state) ? state.Active : 0;
        }

        private sealed class SoundState
        {
            public int Active;
            public float LastStart;
        }
    }
}
