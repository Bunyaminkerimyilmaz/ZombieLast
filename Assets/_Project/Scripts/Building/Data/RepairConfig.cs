using System;
using System.Globalization;
using UnityEngine;

namespace LastHaven.Building
{
    /// <summary>
    /// Yapı onarımı (GDD §11): mesafe, hız, maliyet, ipucu metni ve yapının üstündeki ilerleme barı. Onarım gündüz ve gece yapılır.
    /// Asset'i üretici oluşturur (<c>Data/Building/RepairConfig.asset</c>); yapı prefab'larındaki <see cref="StructureRepair"/> bağlıdır.
    /// </summary>
    public sealed class RepairConfig : ScriptableObject
    {
        [Header("Onarım (GDD §11)")]
        [SerializeField, Min(0f), Tooltip("Oyuncu yapının ayak izine bu yatay mesafeden (m) yakınken onarabilir.")]
        private float _range = 2f;

        [SerializeField, Min(0.01f), Tooltip("Onarım hızı (can/sn).")]
        private float _healthPerSecond = 50f;

        [SerializeField, Min(0.01f), Tooltip("1 paranın onardığı can. Her tur bu kadar onarır ve 1 para alır; son turda eksik can azsa da 1 para.")]
        private float _healthPerMoney = 5f;

        [Header("İpucu")]
        [SerializeField, Tooltip("Etkileşim ipucu; {0} = tam onarımın parası. Tuş adını (E) UI ekler.")]
        private string _promptFormat = "Onar ({0})";

        [Header("İlerleme barı")]
        [SerializeField, Min(0f), Tooltip("Onarım durduktan sonra yapının üstündeki barın görünür kaldığı süre (sn).")]
        private float _barLingerDuration = 1f;

        public RepairSettings Settings => new RepairSettings(_healthPerSecond, _healthPerMoney);

        public float Range => _range;

        public float BarLingerDuration => _barLingerDuration;

        /// <summary>İpucu metni, ör. "Onar (12)". Biçim bozuksa biçimin kendisi + maliyet.</summary>
        public string FormatPrompt(int cost)
        {
            try
            {
                return string.Format(CultureInfo.InvariantCulture, _promptFormat ?? string.Empty, cost);
            }
            catch (FormatException)
            {
                return _promptFormat + " " + cost.ToString(CultureInfo.InvariantCulture);
            }
        }
    }
}
