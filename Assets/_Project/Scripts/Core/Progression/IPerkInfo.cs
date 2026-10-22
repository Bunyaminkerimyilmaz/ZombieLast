using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>
    /// Bir perk'in arayüzde görünen bilgileri (GDD §13, §16: seviye kartları, HUD perk listesi). Progression'daki <c>PerkData</c> uygular;
    /// <see cref="IPerkService.GetPerk"/> verir.
    /// </summary>
    public interface IPerkInfo
    {
        /// <summary>Benzersiz id (ordinal karşılaştırılır).</summary>
        string Id { get; }

        /// <summary>Kartta görünen ad (Türkçe).</summary>
        string DisplayName { get; }

        /// <summary>Kartta görünen etki açıklaması (Türkçe).</summary>
        string Description { get; }

        /// <summary>Opsiyonel ikon; null ise arayüz adın baş harflerini gösterir.</summary>
        Sprite Icon { get; }

        /// <summary>Nadirlik; kart çerçevesinin rengi.</summary>
        PerkRarity Rarity { get; }

        /// <summary>En fazla yığın (en az 1).</summary>
        int MaxStacks { get; }
    }
}
