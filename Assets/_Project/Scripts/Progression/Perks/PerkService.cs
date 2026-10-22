using System;
using System.Collections.Generic;
using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Progression
{
    /// <summary>
    /// [Systems]/Progression: run'ın perk'leri (GDD §13). <see cref="IPerkService"/> olarak Awake'te kaydolur; saf
    /// <see cref="PerkInventory"/>'yi ilk kullanımda (en geç Start) <see cref="IRunModifiers"/> ile kurar. <see cref="ApplyPerk(PerkData)"/>
    /// etkileri uygular ve <see cref="PerkAcquiredEvent"/> gönderir; iyileşme etkileri <see cref="HealRequestedEvent"/> ile oyuncuya
    /// gider. Kart çekilişi <see cref="PerkOfferGenerator"/> (tohum ve kart sayısı <see cref="ProgressionConfig"/>'te). Katalogdaki boş,
    /// id'siz veya tekrar eden perk'ler uyarıyla atlanır (<see cref="PerkLookup"/>). Yeni sahne = yeni run.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PerkService : MonoBehaviour, IPerkService
    {
        private const string LogPrefix = "[Progression] ";

        [SerializeField, Tooltip("Kart sayısı, çekiliş tohumu ve nadirlik ağırlıkları. Üretici bağlar.")]
        private ProgressionConfig _config;

        [SerializeField, Tooltip("Çekilişteki perk'ler. Üretici bağlar.")]
        private PerkCatalog _catalog;

        private readonly PerkLookup _lookup = new PerkLookup();
        private readonly List<PerkData> _offer = new List<PerkData>();
        private PerkOfferGenerator _generator;
        private Func<PerkRarity, int> _rarityWeight;
        private PerkInventory _inventory;
        private bool _registered;

        /// <summary>Çekilişe giren geçerli perk'ler (katalog sırası).</summary>
        public IReadOnlyList<PerkData> Perks => _lookup.Perks;

        /// <summary>Debug ve testler için; ilk erişimde kurulur.</summary>
        public PerkInventory Inventory => _inventory ??= CreateInventory();

        public int OfferSize => _config != null ? _config.OfferSize : 0;

        public int TotalStacks => Inventory.TotalStacks;

        public IReadOnlyList<string> AcquiredPerkIds => Inventory.AcquiredIds;

        public IPerkInfo GetPerk(string perkId)
        {
            return _lookup.Find(perkId);
        }

        public int GetStacks(string perkId)
        {
            return Inventory.GetStacks(perkId);
        }

        public int GetMaxStacks(string perkId)
        {
            PerkData perk = _lookup.Find(perkId);
            return perk != null ? perk.MaxStacks : 0;
        }

        public bool CanAcquire(string perkId)
        {
            return Inventory.CanAcquire(_lookup.Find(perkId));
        }

        public bool ApplyPerk(string perkId)
        {
            PerkData perk = _lookup.Find(perkId);
            if (perk == null)
            {
                Debug.LogWarning($"{LogPrefix}Perk alınamadı: '{perkId}' katalogda yok.", this);
                return false;
            }

            return ApplyPerk(perk);
        }

        /// <summary>Perk'i alır (katalogda olması gerekmez): etkiler, log, <see cref="PerkAcquiredEvent"/>. Maks. yığındaysa false.</summary>
        public bool ApplyPerk(PerkData perk)
        {
            if (!Inventory.TryAcquire(perk))
            {
                string name = perk != null ? perk.DisplayName : "null";
                Debug.LogWarning($"{LogPrefix}Perk alınamadı: {name} (maks. yığında veya id'si boş).", this);
                return false;
            }

            int stacks = Inventory.GetStacks(perk.Id);
            Debug.Log($"{LogPrefix}PerkAcquiredEvent: {perk.DisplayName} ({stacks}/{perk.MaxStacks}).", this);
            EventBus<PerkAcquiredEvent>.Raise(new PerkAcquiredEvent(perk.Id, perk.DisplayName, stacks, perk.MaxStacks));
            return true;
        }

        public int GenerateOffer(List<string> perkIds, int count)
        {
            if (perkIds == null)
            {
                throw new ArgumentNullException(nameof(perkIds));
            }

            perkIds.Clear();
            GenerateOffer(_offer, count);
            for (int i = 0; i < _offer.Count; i++)
            {
                perkIds.Add(_offer[i].Id);
            }

            return perkIds.Count;
        }

        /// <summary><see cref="GenerateOffer(List{string}, int)"/>'in perk verisi döndüren hali (liste önce temizlenir).</summary>
        public int GenerateOffer(List<PerkData> results, int count)
        {
            if (_generator == null)
            {
                results.Clear();
                return 0;
            }

            return _generator.Generate(_lookup.Perks, Inventory, _rarityWeight, count, results);
        }

        private void Awake()
        {
            if (_config == null || _catalog == null)
            {
                Debug.LogError("PerkService: ProgressionConfig veya PerkCatalog atanmamış; Tools/Last Haven/Build Game Scene çalıştır.", this);
                enabled = false;
                return;
            }

            CollectPerks();
            int seed = _config.OfferSeed != 0 ? _config.OfferSeed : Environment.TickCount;
            _generator = new PerkOfferGenerator(seed);
            _rarityWeight = _config.GetRarityWeight;
            ServiceLocator.Register<IPerkService>(this);
            _registered = true;
        }

        private void Start()
        {
            _ = Inventory;
        }

        private void OnDestroy()
        {
            if (_registered)
            {
                ServiceLocator.Unregister<IPerkService>(this);
            }
        }

        private PerkInventory CreateInventory()
        {
            if (!ServiceLocator.TryGet(out IRunModifiers modifiers))
            {
                Debug.LogWarning("PerkService: IRunModifiers bulunamadı; perk'lerin stat ve bayrak etkileri uygulanmayacak.", this);
            }

            return new PerkInventory(modifiers, RequestHeal);
        }

        private void RequestHeal(float amount)
        {
            EventBus<HealRequestedEvent>.Raise(new HealRequestedEvent(amount));
        }

        private void CollectPerks()
        {
            var skipped = new List<PerkData>();
            _lookup.Collect(_catalog.Perks, skipped);
            for (int i = 0; i < skipped.Count; i++)
            {
                PerkData perk = skipped[i];
                Debug.LogWarning($"{LogPrefix}Katalogda id'si boş veya tekrar eden perk atlandı: {perk.name} ('{perk.Id}').", perk);
            }
        }
    }
}
