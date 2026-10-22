using UnityEngine;

namespace LastHaven.Audio
{
    /// <summary>
    /// İki müzik kaynağı (deste 0 ve 1) arasında eşit güçlü geçiş (saf C#): yeni parça sinüsle açılır, eski kosinüsle kısılır.
    /// <see cref="Begin"/> öbür desteyi gelen yapar. Geçiş yarıda kesilirse giden deste o anki ağırlığından kısılmaya başlar
    /// (ses sıçramaz). Süre 0 ise geçiş anında olur.
    /// </summary>
    public sealed class MusicCrossfade
    {
        private float _outgoingStartWeight;

        public MusicCrossfade(float duration)
        {
            Duration = Mathf.Max(0f, duration);
        }

        public float Duration { get; }

        /// <summary>Gelen (veya çalan) deste: 0 ya da 1.</summary>
        public int ActiveDeck { get; private set; }

        /// <summary>Geçişin ilerlemesi (0..1); 1 = oturdu.</summary>
        public float Progress { get; private set; } = 1f;

        public bool IsFading => Progress < 1f;

        /// <summary>Yeni geçiş başlatır; yeni parçanın yükleneceği desteyi döndürür.</summary>
        public int Begin()
        {
            _outgoingStartWeight = GetWeight(ActiveDeck);
            ActiveDeck = 1 - ActiveDeck;
            Progress = Duration > 0f ? 0f : 1f;
            return ActiveDeck;
        }

        public void Tick(float deltaTime)
        {
            if (Progress < 1f)
            {
                Progress = Duration > 0f ? Mathf.Min(1f, Progress + Mathf.Max(0f, deltaTime) / Duration) : 1f;
            }
        }

        /// <summary>Destenin ses çarpanı (0..1).</summary>
        public float GetWeight(int deck)
        {
            float angle = Progress * Mathf.PI * 0.5f;
            return Mathf.Clamp01(deck == ActiveDeck ? Mathf.Sin(angle) : _outgoingStartWeight * Mathf.Cos(angle));
        }
    }
}
