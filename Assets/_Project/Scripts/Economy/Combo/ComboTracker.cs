using System;
using System.Collections.Generic;

namespace LastHaven.Economy
{
    /// <summary>
    /// Kombo (GDD §10, saf): her öldürme sayacı artırır ve pencereyi (2.5 sn) baştan başlatır; pencere dolarsa veya
    /// <see cref="Reset"/> (oyuncu hasar aldı) çağrılırsa sıfırlanır. Çarpan, sayının ulaştığı en yüksek eşiğin çarpanıdır
    /// (5 → ×1.5, 15 → ×2, 30 → ×3); eşiğe ulaştıran öldürme de o çarpanı alır. Sadece <see cref="Tick"/> ile ilerler.
    /// </summary>
    public sealed class ComboTracker
    {
        /// <summary>Eşiğe ulaşılmadan (ve kombo yokken) çarpan.</summary>
        public const float BaseMultiplier = 1f;

        private readonly ComboTier[] _tiers;

        public ComboTracker(float window, IReadOnlyList<ComboTier> tiers)
        {
            Window = Math.Max(0f, window);
            _tiers = new ComboTier[tiers?.Count ?? 0];
            for (int i = 0; i < _tiers.Length; i++)
            {
                _tiers[i] = tiers[i];
            }
        }

        /// <summary>İki öldürme arasındaki en uzun süre (sn).</summary>
        public float Window { get; }

        /// <summary>Ardışık öldürme sayısı; 0 = kombo yok.</summary>
        public int Count { get; private set; }

        /// <summary>Şu anki jeton çarpanı.</summary>
        public float Multiplier { get; private set; } = BaseMultiplier;

        /// <summary>Pencerenin kalan süresi (sn); kombo yokken 0.</summary>
        public float WindowRemaining { get; private set; }

        /// <summary>Pencerenin kalan oranı (1 → 0); kombo yokken 0.</summary>
        public float WindowRatio => Count > 0 && Window > 0f ? WindowRemaining / Window : 0f;

        public bool IsActive => Count > 0;

        /// <summary>
        /// Sayıya göre çarpan: <paramref name="count"/>'u geçmeyen eşiklerin en yüksek çarpanı, hiçbiri yoksa
        /// <see cref="BaseMultiplier"/>. Eşiklerin sırası önemsizdir.
        /// </summary>
        public static float GetMultiplier(int count, IReadOnlyList<ComboTier> tiers)
        {
            float multiplier = BaseMultiplier;
            if (tiers == null)
            {
                return multiplier;
            }

            for (int i = 0; i < tiers.Count; i++)
            {
                if (count >= tiers[i].Kills && tiers[i].Multiplier > multiplier)
                {
                    multiplier = tiers[i].Multiplier;
                }
            }

            return multiplier;
        }

        /// <summary>Öldürmeyi sayar, pencereyi baştan başlatır ve bu öldürmenin çarpanını (sayıldıktan sonraki) döndürür.</summary>
        public float RegisterKill()
        {
            if (Count < int.MaxValue)
            {
                Count++;
            }

            WindowRemaining = Window;
            Multiplier = GetMultiplier(Count, _tiers);
            return Multiplier;
        }

        /// <summary>Pencereyi ilerletir. Pencere bu çağrıda dolduysa komboyu sıfırlar ve true döndürür (bir kez).</summary>
        public bool Tick(float deltaTime)
        {
            if (Count == 0)
            {
                return false;
            }

            WindowRemaining -= deltaTime;
            if (WindowRemaining > 0f)
            {
                return false;
            }

            Clear();
            return true;
        }

        /// <summary>Komboyu sıfırlar (oyuncu hasar aldı). Kombo varsa true.</summary>
        public bool Reset()
        {
            if (Count == 0)
            {
                return false;
            }

            Clear();
            return true;
        }

        private void Clear()
        {
            Count = 0;
            WindowRemaining = 0f;
            Multiplier = BaseMultiplier;
        }
    }
}
