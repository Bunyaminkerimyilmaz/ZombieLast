using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Economy
{
    /// <summary>
    /// Ganimet sandıkları (GDD §9): günlük sayı, halka, açma süresi, yerleşim kuralları, prefab'lar ve toplanabilir ödüllerin
    /// hareketi. Asset'i üretici oluşturur: <c>Data/Loot/LootConfig.asset</c>. Ödül seçenekleri <see cref="LootTableData"/>'da.
    /// <see cref="LootCrateSpawner"/> ve <see cref="LootDispenser"/> kullanır.
    /// </summary>
    public sealed class LootConfig : ScriptableObject
    {
        [Header("Sandıklar (GDD §9)")]
        [SerializeField, Min(0), Tooltip("Her gündüz başında çıkan sandık sayısı. Yer bulunamazsa daha azı çıkar (uyarıyla).")]
        private int _cratesPerDay = 6;

        [SerializeField, Min(0f), Tooltip("Sandığın üs merkezine en kısa yatay uzaklığı (m).")]
        private float _minDistance = 22f;

        [SerializeField, Min(0f), Tooltip("Sandığın üs merkezine en uzun yatay uzaklığı (m).")]
        private float _maxDistance = 38f;

        [SerializeField, Min(0f), Tooltip("Açmak için etkileşim tuşunu basılı tutma süresi (sn). Bırakılırsa ilerleme sıfırlanır.")]
        private float _openHoldDuration = 2f;

        [SerializeField, Min(0f), Tooltip("Oyuncu sandığın kenarına bu yatay mesafeden (m) yakınsa açabilir. GDD'de yok; onarımla aynı.")]
        private float _interactionRange = 2f;

        [SerializeField, Tooltip("Etkileşim ipucu (tuş adını UI ekler: \"E — Aç\").")]
        private string _openPrompt = "Aç";

        [Header("Ödüller")]
        [SerializeField, Tooltip("Yakın/uzak ödül tablosu. Üretici bağlar.")]
        private LootTableData _lootTable;

        [Header("Yerleşim (GDD'de yok)")]
        [SerializeField, Min(1), Tooltip("Sandık başına rastgele nokta deneme sayısı.")]
        private int _placementAttempts = 30;

        [SerializeField, Min(0.1f), Tooltip("Rastgele noktaya en yakın NavMesh noktası en fazla bu kadar uzakta aranır (m).")]
        private float _navMeshSampleDistance = 2f;

        [SerializeField, Min(0f), Tooltip("Sandık merkezinin engellere/yapılara en az yatay uzaklığı (m).")]
        private float _obstacleClearance = 1.2f;

        [SerializeField, Min(0f), Tooltip("İki sandık arası en az yatay uzaklık (m).")]
        private float _minCrateSpacing = 6f;

        [SerializeField, Tooltip("Sandığın çakışmaması gereken layer'lar.")]
        private LayerMask _blockingMask = GameLayers.ObstacleMask | GameLayers.StructureMask;

        [Header("Prefab'lar (üretici bağlar)")]
        [SerializeField, Tooltip("Havuzlanan sandık.")]
        private LootCrate _cratePrefab;

        [SerializeField, Tooltip("Yakıt ödülünün düştüğü toplanabilir bidon.")]
        private LootPickup _fuelPickupPrefab;

        [SerializeField, Tooltip("Mermi paketinin düştüğü toplanabilir kutu.")]
        private LootPickup _ammoPickupPrefab;

        [SerializeField, Min(0), Tooltip("Oyun başında havuzda hazırlanan sandık sayısı.")]
        private int _cratePrewarm = 6;

        [SerializeField, Min(0), Tooltip("Oyun başında havuzda hazırlanan bidon/kutu sayısı (her biri).")]
        private int _pickupPrewarm = 3;

        [Header("Toplanabilir ödüller (GDD'de yok; para jetonları EconomyConfig'te)")]
        [SerializeField, Min(0f), Tooltip("Yere düşen bidon/kutunun ömrü (sn); sonra kaybolur.")]
        private float _pickupLifetime = 30f;

        [SerializeField, Min(0f), Tooltip("Ömrün son bu kadar saniyesinde yanıp söner.")]
        private float _pickupBlinkDuration = 3f;

        [SerializeField, Tooltip("Saçılma yayı, dönme ve mıknatıs uçuşu (jetonlarla aynı kurallar).")]
        private CoinMotionSettings _pickupMotion = new CoinMotionSettings();

        public int CratesPerDay => _cratesPerDay;

        public float MinDistance => _minDistance;

        public float MaxDistance => _maxDistance;

        public float OpenHoldDuration => _openHoldDuration;

        public float InteractionRange => _interactionRange;

        public string OpenPrompt => _openPrompt ?? string.Empty;

        public LootTableData LootTable => _lootTable;

        public int PlacementAttempts => _placementAttempts;

        public float NavMeshSampleDistance => _navMeshSampleDistance;

        public float ObstacleClearance => _obstacleClearance;

        public float MinCrateSpacing => _minCrateSpacing;

        public LayerMask BlockingMask => _blockingMask;

        public LootCrate CratePrefab => _cratePrefab;

        public LootPickup FuelPickupPrefab => _fuelPickupPrefab;

        public LootPickup AmmoPickupPrefab => _ammoPickupPrefab;

        public int CratePrewarm => _cratePrewarm;

        public int PickupPrewarm => _pickupPrewarm;

        public float PickupLifetime => _pickupLifetime;

        public float PickupBlinkDuration => _pickupBlinkDuration;

        public CoinMotionSettings PickupMotion => _pickupMotion ??= new CoinMotionSettings();

        /// <summary>Sandık halkası (o anki değerlerle).</summary>
        public LootRing CreateRing()
        {
            return new LootRing(_minDistance, _maxDistance);
        }

        /// <summary>Ödül türünün toplanabilir prefab'ı; para (jeton) ve bilinmeyen türde null.</summary>
        public LootPickup GetPickupPrefab(LootRewardType type)
        {
            return type switch
            {
                LootRewardType.Fuel => _fuelPickupPrefab,
                LootRewardType.AmmoPack => _ammoPickupPrefab,
                _ => null,
            };
        }
    }
}
