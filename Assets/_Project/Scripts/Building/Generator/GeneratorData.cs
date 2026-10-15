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
        [SerializeField, Min(0f), Tooltip("Yakıt kapasitesi (birim).")]
        private float _fuelCapacity = 150f;

        [SerializeField, Min(0f), Tooltip("Gece taban tüketimi (birim/sn). Gündüz yakıt harcanmaz.")]
        private float _baseFuelConsumption = 0.4f;

        [SerializeField, Min(0f), Tooltip("Her taret için gece ek tüketimi (birim/sn).")]
        private float _fuelConsumptionPerTurret = 0.05f;

        [Header("Acil yakıt (gece)")]
        [SerializeField, Min(0f), Tooltip("Acil yakıt fiyatı (para). Normal fiyatın 2 katı.")]
        private int _emergencyFuelPrice = 30;

        [SerializeField, Min(0f), Tooltip("Bir acil alımda gelen yakıt (birim).")]
        private float _emergencyFuelAmount = 10f;

        public float MaxHealth => _maxHealth;

        public float FuelCapacity => _fuelCapacity;

        public float BaseFuelConsumption => _baseFuelConsumption;

        public float FuelConsumptionPerTurret => _fuelConsumptionPerTurret;

        public int EmergencyFuelPrice => _emergencyFuelPrice;

        public float EmergencyFuelAmount => _emergencyFuelAmount;
    }
}
