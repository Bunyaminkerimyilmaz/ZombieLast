using System.Collections.Generic;
using UnityEngine;

namespace LastHaven.Enemies
{
    /// <summary>
    /// Bir düşmanın süren yavaşlatmaları (saf C#). Geçerli oran en güçlü olanınkidir (<see cref="Current"/>); oranlar toplanmaz.
    /// Zayıf ama uzun süren yavaşlatma, güçlüsü bitince devreye girer (ör. tuzak %40 bitti, Şok Taret %25 sürüyor).
    /// Aynı oran yeniden gelirse kalan süre uzar (kısalmaz). Hem daha zayıf hem daha kısa kalan kayıt tutulmaz; liste küçük kalır.
    /// </summary>
    public sealed class SlowEffects
    {
        private readonly List<Entry> _entries = new List<Entry>(4);

        /// <summary>Geçerli yavaşlatma oranı (0..1); yoksa 0.</summary>
        public float Current { get; private set; }

        public bool IsSlowed => Current > 0f;

        /// <summary>Süren yavaşlatma kaydı sayısı (testler ve hata ayıklama).</summary>
        public int Count => _entries.Count;

        /// <summary>
        /// Yavaşlatma ekler. Oran 0..1'e kırpılır; oran veya süre 0 ise bir şey yapmaz. Geçerli oran hemen güncellenir.
        /// </summary>
        public void Apply(float fraction, float duration)
        {
            fraction = Mathf.Clamp01(fraction);
            if (fraction <= 0f || duration <= 0f)
            {
                return;
            }

            for (int i = _entries.Count - 1; i >= 0; i--)
            {
                Entry entry = _entries[i];
                if (entry.Fraction >= fraction && entry.Remaining >= duration)
                {
                    return; // Var olan hem güçlü hem uzun: yenisi hiçbir an geçerli olmaz.
                }

                if (entry.Fraction <= fraction && entry.Remaining <= duration)
                {
                    _entries.RemoveAt(i); // Yenisi onu her an kapsar.
                }
            }

            _entries.Add(new Entry(fraction, duration));
            Recalculate();
        }

        /// <summary>Süreleri ilerletir, bitenleri siler ve geçerli oranı günceller.</summary>
        public void Tick(float deltaTime)
        {
            if (_entries.Count == 0)
            {
                return;
            }

            for (int i = _entries.Count - 1; i >= 0; i--)
            {
                float remaining = _entries[i].Remaining - deltaTime;
                if (remaining <= 0f)
                {
                    _entries.RemoveAt(i);
                }
                else
                {
                    _entries[i] = new Entry(_entries[i].Fraction, remaining);
                }
            }

            Recalculate();
        }

        /// <summary>Tüm yavaşlatmaları kaldırır (havuz, ölüm).</summary>
        public void Clear()
        {
            _entries.Clear();
            Current = 0f;
        }

        private void Recalculate()
        {
            float strongest = 0f;
            for (int i = 0; i < _entries.Count; i++)
            {
                strongest = Mathf.Max(strongest, _entries[i].Fraction);
            }

            Current = strongest;
        }

        private readonly struct Entry
        {
            public Entry(float fraction, float remaining)
            {
                Fraction = fraction;
                Remaining = remaining;
            }

            public float Fraction { get; }

            public float Remaining { get; }
        }
    }
}
