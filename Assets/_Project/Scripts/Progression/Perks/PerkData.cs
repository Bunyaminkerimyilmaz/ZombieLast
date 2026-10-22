using System.Collections.Generic;
using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Progression
{
    /// <summary>
    /// Bir perk (GDD §13). Asset'leri üretici oluşturur: <c>Data/Progression/Perks/&lt;Ad&gt;.asset</c>; <see cref="PerkCatalog"/>'a
    /// bağlıdır. Her alımda <see cref="Effects"/> sırayla uygulanır; en fazla <see cref="MaxStacks"/> kez alınabilir. Arayüz bilgileri
    /// Core'daki <see cref="IPerkInfo"/> ile okunur (seviye kartları, HUD perk listesi).
    /// </summary>
    public sealed class PerkData : ScriptableObject, IPerkInfo
    {
        /// <summary>Perk modifier'larının kaynak id öneki: "perk:quick_trigger".</summary>
        public const string SourcePrefix = "perk:";

        [Header("Kimlik")]
        [SerializeField, Tooltip("Benzersiz id (ordinal karşılaştırılır), ör. quick_trigger. Modifier kaynağı ve kayıtlar bununla.")]
        private string _id = string.Empty;

        [SerializeField, Tooltip("Kartta görünen ad.")]
        private string _displayName = string.Empty;

        [SerializeField, TextArea(2, 4), Tooltip("Kartta görünen etki açıklaması.")]
        private string _description = string.Empty;

        [SerializeField, Tooltip("Opsiyonel kart ikonu; boşsa arayüz baş harfi gösterir.")]
        private Sprite _icon;

        [Header("Çekiliş (GDD §13)")]
        [SerializeField, Tooltip("Nadirlik; çekiliş ağırlığı ProgressionConfig'te.")]
        private PerkRarity _rarity;

        [SerializeField, Min(1), Tooltip("En fazla kaç kez alınabilir. Ulaşınca tekrar sunulmaz.")]
        private int _maxStacks = 1;

        [Header("Etkiler")]
        [SerializeReference, Tooltip("Her alımda (yığın başına) sırayla uygulanır. İyileşme maks. can etkisinden sonra gelmeli.")]
        private List<PerkEffect> _effects = new List<PerkEffect>();

        public string Id => _id;

        public string DisplayName => _displayName;

        public string Description => _description;

        /// <summary>Null olabilir.</summary>
        public Sprite Icon => _icon;

        public PerkRarity Rarity => _rarity;

        public int MaxStacks => Mathf.Max(1, _maxStacks);

        public IReadOnlyList<PerkEffect> Effects => _effects;

        /// <summary>Eklenen modifier'ların kaynak id'si (<see cref="SourcePrefix"/> + id). Sadece alımda okunur.</summary>
        public string SourceId => SourcePrefix + _id;
    }
}
