using System.Collections.Generic;
using UnityEngine;

namespace LastHaven.Waves
{
    /// <summary>
    /// Dalga ayarları (GDD §8): gecelerin sıralı listesi ve canlı düşman tavanı.
    /// Asset'i üretici oluşturur: <c>Data/Waves/WaveConfig.asset</c>.
    /// </summary>
    public sealed class WaveConfig : ScriptableObject
    {
        [Header("Geceler")]
        [SerializeField, Tooltip("Gece verileri, oynanış sırasıyla: 0. eleman 1. gece.")]
        private List<NightData> _nights = new List<NightData>();

        [Header("Sınırlar")]
        [SerializeField, Min(1), Tooltip("Aynı anda en fazla canlı dalga düşmanı (GDD §8). Fazlası kuyrukta bekler, yer açılınca doğar.")]
        private int _maxAliveEnemies = 120;

        /// <summary>Elemanlar null olabilir (inspector'da boş bırakılmış satır).</summary>
        public IReadOnlyList<NightData> Nights => _nights;

        public int MaxAliveEnemies => _maxAliveEnemies;
    }
}
