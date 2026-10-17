using System;
using System.Globalization;
using UnityEngine;

namespace LastHaven.Building
{
    /// <summary>Jeneratör ayarları (GDD §12). Asset'i üretici oluşturur: <c>Data/Base/GeneratorData.asset</c>.</summary>
    public sealed class GeneratorData : ScriptableObject
    {
        [Header("Can")]
        [SerializeField, Min(1f), Tooltip("Maksimum can. Sıfıra inerse oyun biter.")]
        private float _maxHealth = 1500f;

        [Header("Yakıt")]
        [SerializeField, Min(0), Tooltip("Yakıt kapasitesi (birim). Cüzdandaki yakıt bunu aşamaz.")]
        private int _fuelCapacity = 150;

        [SerializeField, Min(0f), Tooltip("Gece taban tüketimi (birim/sn). Gündüz yakıt harcanmaz.")]
        private float _baseFuelConsumption = 0.4f;

        [SerializeField, Min(0f), Tooltip("Her taret için gece ek tüketimi (birim/sn).")]
        private float _fuelConsumptionPerTurret = 0.05f;

        [Header("Acil yakıt (gece)")]
        [SerializeField, Min(0), Tooltip("Acil yakıt fiyatı (para). Normal fiyatın 2 katı.")]
        private int _emergencyFuelPrice = 30;

        [SerializeField, Min(1), Tooltip("Bir acil alımda gelen yakıt (birim). Depoda bu kadar yer yoksa alım sunulmaz.")]
        private int _emergencyFuelAmount = 10;

        [SerializeField, Min(0f), Tooltip("Bir alım için E'yi basılı tutma süresi (sn). Her basışta bir alım.")]
        private float _emergencyFuelHoldDuration = 1f;

        [SerializeField, Min(0f), Tooltip("Oyuncu jeneratörün kenarına bu yatay mesafeden (m) yakınken alabilir.")]
        private float _emergencyFuelRange = 2f;

        [SerializeField, Tooltip("Etkileşim ipucu; {0} = fiyat (para), {1} = yakıt miktarı. Tuş adını (E) UI ekler.")]
        private string _emergencyFuelPromptFormat = "Acil yakıt +{1} ({0} para)";

        public float MaxHealth => _maxHealth;

        public int FuelCapacity => _fuelCapacity;

        public float BaseFuelConsumption => _baseFuelConsumption;

        public float FuelConsumptionPerTurret => _fuelConsumptionPerTurret;

        public int EmergencyFuelPrice => _emergencyFuelPrice;

        public int EmergencyFuelAmount => _emergencyFuelAmount;

        public float EmergencyFuelHoldDuration => _emergencyFuelHoldDuration;

        public float EmergencyFuelRange => _emergencyFuelRange;

        /// <summary>İpucu metni, ör. "Acil yakıt +10 (30 para)". Biçim bozuksa biçimin kendisi + fiyat.</summary>
        public string FormatEmergencyFuelPrompt()
        {
            try
            {
                return string.Format(CultureInfo.InvariantCulture, _emergencyFuelPromptFormat ?? string.Empty,
                    _emergencyFuelPrice, _emergencyFuelAmount);
            }
            catch (FormatException)
            {
                return _emergencyFuelPromptFormat + " " + _emergencyFuelPrice.ToString(CultureInfo.InvariantCulture);
            }
        }
    }
}
