using System;
using System.Collections.Generic;
using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Tests.EditMode
{
    /// <summary>Seviye kartları testleri için bekleyen seviye kuyruğu (sadece kuyruk kısmı anlamlı).</summary>
    internal sealed class FakeExperienceService : IExperienceService
    {
        private readonly Queue<int> _pending = new Queue<int>();

        public int Level { get; private set; } = 1;
        public int Xp => 0;
        public int XpToNextLevel => 16;
        public int TotalXp => 0;
        public int PendingLevelUps => _pending.Count;

        /// <summary>Bir seviye atlatır (kuyruğa ekler).</summary>
        public void LevelUp()
        {
            Level++;
            _pending.Enqueue(Level);
        }

        public bool TryDequeueLevelUp(out int level)
        {
            if (_pending.Count == 0)
            {
                level = 0;
                return false;
            }

            level = _pending.Dequeue();
            return true;
        }
    }

    /// <summary>Basit perk servisi: kartlar listenin başından, alınabilir perk'lerden çekilir (rastgelelik yok).</summary>
    internal sealed class FakePerkService : IPerkService
    {
        private readonly List<FakePerk> _perks = new List<FakePerk>();
        private readonly Dictionary<string, int> _stacks = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly List<string> _acquired = new List<string>();

        public int OfferSize { get; set; } = 3;
        public int TotalStacks { get; private set; }
        public IReadOnlyList<string> AcquiredPerkIds => _acquired;
        public List<string> Applied { get; } = new List<string>();

        public FakePerk Add(string id, int maxStacks = 1, PerkRarity rarity = PerkRarity.Common)
        {
            var perk = new FakePerk(id, maxStacks, rarity);
            _perks.Add(perk);
            return perk;
        }

        public IPerkInfo GetPerk(string perkId)
        {
            return _perks.Find(perk => perk.Id == perkId);
        }

        public int GetStacks(string perkId)
        {
            return _stacks.TryGetValue(perkId, out int stacks) ? stacks : 0;
        }

        public int GetMaxStacks(string perkId)
        {
            return GetPerk(perkId)?.MaxStacks ?? 0;
        }

        public bool CanAcquire(string perkId)
        {
            IPerkInfo perk = GetPerk(perkId);
            return perk != null && GetStacks(perkId) < perk.MaxStacks;
        }

        public bool ApplyPerk(string perkId)
        {
            if (!CanAcquire(perkId))
            {
                return false;
            }

            int stacks = GetStacks(perkId) + 1;
            _stacks[perkId] = stacks;
            if (stacks == 1)
            {
                _acquired.Add(perkId);
            }

            TotalStacks++;
            Applied.Add(perkId);
            return true;
        }

        public int GenerateOffer(List<string> perkIds, int count)
        {
            perkIds.Clear();
            for (int i = 0; i < _perks.Count && perkIds.Count < count; i++)
            {
                if (CanAcquire(_perks[i].Id))
                {
                    perkIds.Add(_perks[i].Id);
                }
            }

            return perkIds.Count;
        }
    }

    internal sealed class FakePerk : IPerkInfo
    {
        public FakePerk(string id, int maxStacks, PerkRarity rarity)
        {
            Id = id;
            DisplayName = id;
            Description = id + " açıklama";
            MaxStacks = maxStacks;
            Rarity = rarity;
        }

        public string Id { get; }
        public string DisplayName { get; set; }
        public string Description { get; }
        public Sprite Icon => null;
        public PerkRarity Rarity { get; }
        public int MaxStacks { get; }
    }

    /// <summary>Duraklatma servisi: kaynakları saf <see cref="PauseSources"/>'ta tutar, zaman ölçeğine dokunmaz.</summary>
    internal sealed class FakePauseService : IPauseService
    {
        private readonly PauseSources _sources = new PauseSources();

        public bool IsPaused => _sources.IsPaused;

        public void Pause(object source)
        {
            _sources.Add(source);
        }

        public void Resume(object source)
        {
            _sources.Remove(source);
        }

        public bool IsPausedBy(object source)
        {
            return _sources.Contains(source);
        }
    }
}
