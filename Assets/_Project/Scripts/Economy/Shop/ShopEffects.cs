using System;
using LastHaven.Core;

namespace LastHaven.Economy
{
    /// <summary>
    /// Market etkilerinin uygulanabilirliği ve uygulanması (saf; GDD §6, §10). Silah açma, yükseltme, mermi ve can paketi olayla
    /// gönderilir (Player uygular); yakıt cüzdana, mıknatıs <see cref="IRunModifiers"/>'a doğrudan yazılır.
    /// </summary>
    public static class ShopEffects
    {
        /// <summary>
        /// Etki şu an anlamlı mı: yakıtta depoda tam miktar kadar yer (para boşa gitmesin; acil yakıtla aynı kural), can paketinde
        /// canlı ve canı eksik oyuncu, mıknatısta run modifier servisi, silah etkilerinde hedef id gerekir.
        /// </summary>
        public static bool CanApply(IShopItem item, IWallet wallet, IRunModifiers modifiers, IPlayer player)
        {
            switch (item.Effect)
            {
                case ShopEffectType.UnlockWeapon:
                case ShopEffectType.WeaponUpgrade:
                case ShopEffectType.BuyAmmo:
                    return !string.IsNullOrEmpty(item.TargetId);
                case ShopEffectType.HealthKit:
                    return player != null && player.IsAlive && player.CurrentHealth < player.MaxHealth;
                case ShopEffectType.BuyFuel:
                    int fuel = ToAmount(item.Value);
                    return fuel > 0 && (long)wallet.Fuel + fuel <= wallet.GetCapacity(WalletResource.Fuel);
                case ShopEffectType.PickupRadius:
                    return modifiers != null;
                default:
                    return false;
            }
        }

        /// <summary>Etkiyi uygular. <paramref name="level"/> öğenin yeni seviyesi. Önce <see cref="CanApply"/> kontrol edilmeli.</summary>
        public static void Apply(IShopItem item, int level, IWallet wallet, IRunModifiers modifiers)
        {
            switch (item.Effect)
            {
                case ShopEffectType.UnlockWeapon:
                    EventBus<WeaponUnlockRequestedEvent>.Raise(new WeaponUnlockRequestedEvent(item.TargetId));
                    break;
                case ShopEffectType.WeaponUpgrade:
                    EventBus<WeaponUpgradePurchasedEvent>.Raise(new WeaponUpgradePurchasedEvent(item.TargetId, item.UpgradeType, level));
                    break;
                case ShopEffectType.BuyAmmo:
                    EventBus<AmmoPurchasedEvent>.Raise(new AmmoPurchasedEvent(item.TargetId, ToAmount(item.Value)));
                    break;
                case ShopEffectType.HealthKit:
                    EventBus<HealRequestedEvent>.Raise(new HealRequestedEvent(item.Value));
                    break;
                case ShopEffectType.BuyFuel:
                    wallet.Add(WalletResource.Fuel, ToAmount(item.Value));
                    break;
                case ShopEffectType.PickupRadius:
                    // Her alım ayrı bir düz ekleme; kaynak id öğenin id'si (3 seviye = +3 m).
                    modifiers.AddModifier(StatModifier.Flat(StatType.PickupRadius, item.Value, item.Id));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(item), item.Effect, "Bilinmeyen market etkisi.");
            }
        }

        /// <summary>Tam sayı miktar (mermi, yakıt): en yakın tam sayı, en az 0.</summary>
        public static int ToAmount(float value)
        {
            return Math.Max(0, (int)Math.Round(value, MidpointRounding.AwayFromZero));
        }
    }
}
