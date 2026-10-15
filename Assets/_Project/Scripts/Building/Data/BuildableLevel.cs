using System;
using System.Collections.Generic;
using UnityEngine;

namespace LastHaven.Building
{
    /// <summary>
    /// Yapının bir seviyesi (GDD §11 tablosunun bir hücresi): ad, fiyat, maksimum can ve tipe özel statlar.
    /// Statlar o seviyedeki <b>mutlak</b> değerdir (ör. taret 2. seviye hasarı 8 × 1.6 = 12.8), öncekine göre çarpan değil.
    /// </summary>
    [Serializable]
    public sealed class BuildableLevel
    {
        [SerializeField, Tooltip("Seviyenin görünen adı (ör. Ahşap, Metal, Beton). Boş olabilir.")]
        private string _displayName;

        [SerializeField, Min(0), Tooltip("Para. 1. seviyede inşa fiyatı, sonrakilerde bu seviyeye yükseltme fiyatı.")]
        private int _cost;

        [SerializeField, Min(1f), Tooltip("Bu seviyedeki maksimum can.")]
        private float _maxHealth = 100f;

        [SerializeField, Tooltip("Seviyenin görünümü (GDD §18: barikat kahve → gri → beyaz). Visual'da herhangi bir seviyenin " +
                                 "materyalini kullanan yuvalar bununla değişir; diğer materyallere dokunulmaz. Boş olabilir.")]
        private Material _material;

        [SerializeField, Tooltip("Tipe özel değerler (hasar, atış hızı, menzil, yavaşlatma...). Aynı stat bir kez yazılmalı.")]
        private List<BuildableStatValue> _stats = new List<BuildableStatValue>();

        public BuildableLevel()
        {
        }

        public BuildableLevel(string displayName, int cost, float maxHealth, params BuildableStatValue[] stats)
            : this(displayName, cost, maxHealth, null, stats)
        {
        }

        public BuildableLevel(string displayName, int cost, float maxHealth, Material material, params BuildableStatValue[] stats)
        {
            _displayName = displayName;
            _cost = cost;
            _maxHealth = maxHealth;
            _material = material;
            _stats = new List<BuildableStatValue>(stats ?? Array.Empty<BuildableStatValue>());
        }

        public string DisplayName => _displayName;

        public int Cost => _cost;

        public float MaxHealth => _maxHealth;

        /// <summary>Seviyenin görsel materyali; null olabilir.</summary>
        public Material Material => _material;

        public IReadOnlyList<BuildableStatValue> Stats => _stats;

        /// <summary>Stat bu seviyede yazılıysa değerini verir. Aynı stat iki kez yazılmışsa ilki geçerlidir.</summary>
        public bool TryGetStat(BuildableStat stat, out float value)
        {
            for (int i = 0; i < _stats.Count; i++)
            {
                if (_stats[i].Stat == stat)
                {
                    value = _stats[i].Value;
                    return true;
                }
            }

            value = 0f;
            return false;
        }

        /// <summary>Stat yazılıysa değeri, değilse <paramref name="fallback"/>.</summary>
        public float GetStat(BuildableStat stat, float fallback)
        {
            return TryGetStat(stat, out float value) ? value : fallback;
        }
    }
}
