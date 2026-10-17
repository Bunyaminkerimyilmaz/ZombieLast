using System;
using UnityEngine;

namespace LastHaven.Building
{
    /// <summary>Yapı seviyesindeki sayısal değerler (GDD §11). Can ve fiyat seviyede ayrı alandır; bunlar tipe özel değerlerdir.</summary>
    /// <remarks>Değerler açıkça numaralı (asset'lerde int saklanır): sırayı değiştirme, yenisini sona ekle.</remarks>
    public enum BuildableStat
    {
        /// <summary>Atış başına hasar (taret) veya saniyedeki hasar (tuzak).</summary>
        Damage = 0,

        /// <summary>Atış/sn (taret).</summary>
        FireRate = 1,

        /// <summary>Menzil (m, taret).</summary>
        Range = 2,

        /// <summary>Yavaşlatma oranı (0.4 = %40, tuzak).</summary>
        SlowFraction = 3,

        /// <summary>
        /// Kullanılmıyor (9.3): tuzağın dayanıklılığı seviyenin canıdır (<see cref="BuildableLevel.MaxHealth"/>); her tik canından
        /// düşer (<see cref="SpikeTrap"/>). Değer asset'lerde int saklandığı için silinmedi.
        /// </summary>
        Durability = 4,

        /// <summary>Hasar tiki aralığı (sn, tuzak).</summary>
        TickInterval = 5,
    }

    /// <summary>Bir seviyedeki tek stat değeri.</summary>
    [Serializable]
    public struct BuildableStatValue
    {
        [SerializeField]
        private BuildableStat _stat;

        [SerializeField]
        private float _value;

        public BuildableStatValue(BuildableStat stat, float value)
        {
            _stat = stat;
            _value = value;
        }

        public BuildableStat Stat => _stat;

        public float Value => _value;
    }
}
