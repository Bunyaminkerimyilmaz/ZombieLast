using UnityEngine;

namespace LastHaven.Enemies
{
    /// <summary>Düşman hız çarpanının kaynağı. Her kaynak kendi çarpanını yazar; etkili çarpan hepsinin çarpımıdır.</summary>
    public enum SpeedSource
    {
        /// <summary>Durum efektleri (yavaşlatma: 1 − oran; <see cref="EnemyStatusEffects"/>).</summary>
        Status = 0,

        /// <summary>Boss öfkesi (GDD §7 Brute: ×1.4; <see cref="BossBehaviour"/>).</summary>
        Boss = 1,

        /// <summary>Gece olayı (GDD §14 Sis: ×1.1; dalga tarafı doğuşta yazar).</summary>
        NightEvent = 2,
    }

    /// <summary>Kaynak başına hız çarpanları (saf). Bir kaynağın yazdığı değer diğerini ezmez; <see cref="Product"/> çarpımdır.</summary>
    public sealed class SpeedMultipliers
    {
        private const int SourceCount = 3;

        private readonly float[] _values = { 1f, 1f, 1f };

        /// <summary>Tüm kaynakların çarpımı (negatif çarpanlar 0'a kırpılır).</summary>
        public float Product { get; private set; } = 1f;

        public float Get(SpeedSource source) => _values[(int)source];

        public void Set(SpeedSource source, float multiplier)
        {
            _values[(int)source] = Mathf.Max(0f, multiplier);
            float product = 1f;
            for (int i = 0; i < SourceCount; i++)
            {
                product *= _values[i];
            }

            Product = product;
        }

        /// <summary>Hepsini 1'e döndürür (havuzdan çıkış).</summary>
        public void Reset()
        {
            for (int i = 0; i < SourceCount; i++)
            {
                _values[i] = 1f;
            }

            Product = 1f;
        }
    }
}
