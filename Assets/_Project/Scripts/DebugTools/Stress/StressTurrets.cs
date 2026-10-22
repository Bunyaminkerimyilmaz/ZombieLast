using System;
using System.Collections.Generic;
using LastHaven.Building;
using LastHaven.Core;
using UnityEngine;

namespace LastHaven.DebugTools
{
    /// <summary>
    /// Stres testinin taretleri: merkezin çevresine eşit açılarla normal inşa yoluyla yerleştirir (<see cref="BuildSite.TryPlace"/>;
    /// fiyatı önce cüzdana eklenir, yerleşmezse geri alınır: para değişmez), test boyunca hasar almazlar (<see cref="DamageImmunity"/>),
    /// sonda iadesiz kaldırılır. Yer doluysa (yapı, engel) o açı atlanır.
    /// </summary>
    public sealed class StressTurrets
    {
        private readonly List<Structure> _placed = new List<Structure>();
        private readonly List<DamageImmunity> _immunities = new List<DamageImmunity>();

        /// <summary>Yerleştirilen taret sayısı.</summary>
        public int Count => _placed.Count;

        /// <summary><paramref name="count"/> taret yerleştirmeyi dener; yerleşeni döndürür.</summary>
        public int Place(BuildSite site, BuildableData turret, Vector3 center, int count, float radius, IWallet wallet)
        {
            if (site == null || turret == null || count <= 0)
            {
                return 0;
            }

            float step = 2f * Mathf.PI / count;
            for (int i = 0; i < count; i++)
            {
                Vector3 point = center + new Vector3(Mathf.Cos(step * i), 0f, Mathf.Sin(step * i)) * radius;
                if (TryPlace(site, turret, point, wallet, out Structure structure))
                {
                    var immunity = new DamageImmunity();
                    immunity.Apply(structure.Health);
                    _placed.Add(structure);
                    _immunities.Add(immunity);
                }
            }

            return _placed.Count;
        }

        /// <summary>Korumayı kaldırır ve hâlâ yerleşik taretleri iadesiz söker.</summary>
        public void Remove(BuildSite site)
        {
            for (int i = 0; i < _placed.Count; i++)
            {
                _immunities[i].Remove();
                Structure structure = _placed[i];
                if (site != null && structure != null && site.TryGetPlaced(structure.gameObject, out _))
                {
                    site.Remove(structure.gameObject, StructureRemovalReason.Sold, 0);
                }
            }

            _placed.Clear();
            _immunities.Clear();
        }

        /// <summary>Sadece korumayı kaldırır (sahne kapanırken; yapılar zaten yok oluyor).</summary>
        public void Unprotect()
        {
            for (int i = 0; i < _immunities.Count; i++)
            {
                _immunities[i].Remove();
            }
        }

        /// <summary>Normal inşa yolu. Para önce eklenir; yerleşmezse geri alınır.</summary>
        private static bool TryPlace(BuildSite site, BuildableData turret, Vector3 point, IWallet wallet, out Structure structure)
        {
            structure = null;
            int cost = turret.BuildCost;
            wallet?.Add(WalletResource.Money, cost);
            if (!site.TryPlace(turret, point, 0, out BuildPlacement placement))
            {
                wallet?.TrySpend(WalletResource.Money, cost);
                return false;
            }

            return site.TryGetStructureAt(placement.Position, out structure) && structure != null;
        }

        /// <summary>İnşa yuvalarında id'si (ordinal) eşleşen yapı; yoksa null.</summary>
        public static BuildableData FindBuildable(BuildSite site, string id)
        {
            IReadOnlyList<BuildableData> slots = site != null && site.Config != null ? site.Config.Slots : null;
            if (slots == null)
            {
                return null;
            }

            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] != null && string.Equals(slots[i].Id, id, StringComparison.Ordinal))
                {
                    return slots[i];
                }
            }

            return null;
        }
    }
}
