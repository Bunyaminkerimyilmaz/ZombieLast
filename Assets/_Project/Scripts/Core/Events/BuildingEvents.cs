using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>İnşa modu açılıp kapandığında gönderilir (GDD §11; B tuşu, gece başlarken otomatik kapanır). İnşa çubuğu (UI) dinler.</summary>
    public readonly struct BuildModeChangedEvent : IEvent
    {
        public BuildModeChangedEvent(bool isActive)
        {
            IsActive = isActive;
        }

        /// <summary>İnşa modu açık mı.</summary>
        public bool IsActive { get; }
    }

    /// <summary>
    /// İnşa modunda seçili yapı değiştiğinde gönderilir (1–5, D-pad, iptal). Seçim kalkınca (iptal, mod kapandı)
    /// <see cref="SlotIndex"/> -1 ve <see cref="BuildableId"/> boştur. İnşa çubuğu (UI) dinler.
    /// </summary>
    public readonly struct BuildSelectionChangedEvent : IEvent
    {
        public BuildSelectionChangedEvent(int slotIndex, string buildableId)
        {
            SlotIndex = slotIndex;
            BuildableId = buildableId ?? string.Empty;
        }

        /// <summary>0 tabanlı yuva (1 tuşu → 0); seçim yoksa -1.</summary>
        public int SlotIndex { get; }

        /// <summary>Seçili yapının id'si (<c>BuildableData.Id</c>); seçim yoksa boş.</summary>
        public string BuildableId { get; }

        public bool HasSelection => SlotIndex >= 0;
    }

    /// <summary>
    /// Yapı yerleştirildiğinde gönderilir, fiyat cüzdandan düşüldükten (<see cref="MoneyChangedEvent"/>) ve yapı
    /// etkinleştikten sonra. Ses, istatistik ve görev/ipucu sistemleri dinler.
    /// </summary>
    public readonly struct StructurePlacedEvent : IEvent
    {
        public StructurePlacedEvent(GameObject structure, string buildableId, Vector3 position, int rotationSteps, int cost)
        {
            Structure = structure;
            BuildableId = buildableId ?? string.Empty;
            Position = position;
            RotationSteps = rotationSteps;
            Cost = cost;
        }

        /// <summary>Yapının kök objesi.</summary>
        public GameObject Structure { get; }

        public string BuildableId { get; }

        /// <summary>Ayak izinin merkezi, yer seviyesinde (dünya).</summary>
        public Vector3 Position { get; }

        /// <summary>Y ekseninde 90°'lik adım sayısı (0–3).</summary>
        public int RotationSteps { get; }

        /// <summary>Ödenen fiyat (para).</summary>
        public int Cost { get; }
    }

    /// <summary>
    /// Yapı ızgaradan kalktığında (satıldı veya yıkıldı) gönderilir; hücreleri boşalmıştır. Obje bu olaydan sonra havuza döner.
    /// Satışta iade bu olaydan önce cüzdana eklenmiştir (<see cref="MoneyChangedEvent"/>).
    /// </summary>
    public readonly struct StructureRemovedEvent : IEvent
    {
        public StructureRemovedEvent(GameObject structure, string buildableId, Vector3 position, StructureRemovalReason reason, int refund)
        {
            Structure = structure;
            BuildableId = buildableId ?? string.Empty;
            Position = position;
            Reason = reason;
            Refund = refund;
        }

        /// <summary>Yapının kök objesi (olaydan sonra havuza döner).</summary>
        public GameObject Structure { get; }

        public string BuildableId { get; }

        /// <summary>Ayak izinin merkezi, yer seviyesinde (dünya).</summary>
        public Vector3 Position { get; }

        public StructureRemovalReason Reason { get; }

        /// <summary>Satışta iade edilen para; yıkılmada 0.</summary>
        public int Refund { get; }
    }

    /// <summary>
    /// İnşa modunda imlecin altındaki yapı değiştiğinde gönderilir (GDD §16 tooltip, Gün 10). Yapıdan çıkınca, mod kapanınca
    /// ve yapı satılınca <see cref="Structure"/> null olur. Yükseltmeden sonra aynı yapı için yeni değerlerle tekrar gönderilir.
    /// </summary>
    public readonly struct StructureHoveredEvent : IEvent
    {
        public StructureHoveredEvent(GameObject structure, string buildableId, int level, int maxLevel, int upgradeCost, int sellRefund)
        {
            Structure = structure;
            BuildableId = buildableId ?? string.Empty;
            Level = level;
            MaxLevel = maxLevel;
            UpgradeCost = upgradeCost;
            SellRefund = sellRefund;
        }

        /// <summary>Üzerine gelinen yapının kök objesi; yoksa null.</summary>
        public GameObject Structure { get; }

        public string BuildableId { get; }

        /// <summary>1 tabanlı seviye (1 = inşa edilen); yapı yoksa 0.</summary>
        public int Level { get; }

        public int MaxLevel { get; }

        /// <summary>Sonraki seviyenin fiyatı; en üst seviyede 0.</summary>
        public int UpgradeCost { get; }

        /// <summary>Şimdi satılırsa iade edilecek para.</summary>
        public int SellRefund { get; }

        public bool HasStructure => Structure != null;

        public bool CanUpgrade => HasStructure && Level < MaxLevel;
    }

    /// <summary>Yapı yükseltildiğinde gönderilir: fiyat cüzdandan düşüldükten, can ve görsel yeni seviyeye geçtikten sonra.</summary>
    public readonly struct StructureUpgradedEvent : IEvent
    {
        public StructureUpgradedEvent(GameObject structure, string buildableId, int level, int cost)
        {
            Structure = structure;
            BuildableId = buildableId ?? string.Empty;
            Level = level;
            Cost = cost;
        }

        public GameObject Structure { get; }

        public string BuildableId { get; }

        /// <summary>Yeni seviye (1 tabanlı; ilk yükseltmeden sonra 2).</summary>
        public int Level { get; }

        /// <summary>Ödenen yükseltme fiyatı (para).</summary>
        public int Cost { get; }
    }

    /// <summary>
    /// Yapının canı bitince gönderilir (düşmanlar yıktı), ızgaradan kalkmadan önce. Ardından aynı yapı için
    /// <see cref="StructureRemovedEvent"/> (<see cref="StructureRemovalReason.Destroyed"/>) gelir. Yıkılma efekti (Feedback) dinler.
    /// </summary>
    public readonly struct StructureDestroyedEvent : IEvent
    {
        public StructureDestroyedEvent(GameObject structure, string buildableId, Vector3 position, Bounds bounds, DamageInfo killingBlow)
        {
            Structure = structure;
            BuildableId = buildableId ?? string.Empty;
            Position = position;
            Bounds = bounds;
            KillingBlow = killingBlow;
        }

        /// <summary>Yapının kök objesi (olaydan sonra havuza döner).</summary>
        public GameObject Structure { get; }

        public string BuildableId { get; }

        /// <summary>Yapının pivotu: ayak izinin merkezi, yer seviyesinde (dünya).</summary>
        public Vector3 Position { get; }

        /// <summary>Yapının dünya uzayındaki kutusu (collider'ı; yoksa pivotta boş kutu). Efektin boyutu buna göre.</summary>
        public Bounds Bounds { get; }

        public DamageInfo KillingBlow { get; }
    }
}
