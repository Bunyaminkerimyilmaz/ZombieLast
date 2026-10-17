using LastHaven.Combat;
using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Player
{
    /// <summary>
    /// Market satın almalarının oyuncuya etkisi (GDD §6, §10): silah açma, silah yükseltmesi, yedek mermi
    /// (<see cref="WeaponController"/>'ın silahlarına) ve can paketi (<see cref="Health"/>). 11.1: ganimetten toplanan mermi kutusu
    /// da (<see cref="AmmoPickedUpEvent"/>) yedeğe eklenir. Olayları Economy (market, ganimet) gönderir; bu bileşen sadece uygular.
    /// Yakıt (cüzdan) ve mıknatıs (<see cref="IRunModifiers"/>) Economy'nin kendisinde.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(WeaponController), typeof(Health))]
    public sealed class PlayerPurchaseEffects : MonoBehaviour
    {
        private const string LogPrefix = "[Player] ";

        private WeaponController _weapons;
        private Health _health;

        private void Awake()
        {
            _weapons = GetComponent<WeaponController>();
            _health = GetComponent<Health>();
        }

        private void OnEnable()
        {
            EventBus<WeaponUnlockRequestedEvent>.Subscribe(OnWeaponUnlockRequested);
            EventBus<WeaponUpgradePurchasedEvent>.Subscribe(OnWeaponUpgradePurchased);
            EventBus<AmmoPurchasedEvent>.Subscribe(OnAmmoPurchased);
            EventBus<AmmoPickedUpEvent>.Subscribe(OnAmmoPickedUp);
            EventBus<HealRequestedEvent>.Subscribe(OnHealRequested);
        }

        private void OnDisable()
        {
            EventBus<WeaponUnlockRequestedEvent>.Unsubscribe(OnWeaponUnlockRequested);
            EventBus<WeaponUpgradePurchasedEvent>.Unsubscribe(OnWeaponUpgradePurchased);
            EventBus<AmmoPurchasedEvent>.Unsubscribe(OnAmmoPurchased);
            EventBus<AmmoPickedUpEvent>.Unsubscribe(OnAmmoPickedUp);
            EventBus<HealRequestedEvent>.Unsubscribe(OnHealRequested);
        }

        private void OnWeaponUnlockRequested(WeaponUnlockRequestedEvent request)
        {
            WeaponArsenal arsenal = GetArsenal();
            int slot = arsenal != null ? arsenal.FindSlot(request.WeaponId) : WeaponSlots.None;
            if (arsenal != null && arsenal.Unlock(slot))
            {
                Debug.Log($"{LogPrefix}Silah açıldı: {arsenal.GetWeapon(slot).Data.DisplayName} ({slot + 1}. yuva).", this);
                return;
            }

            Debug.LogWarning($"{LogPrefix}Silah açılamadı: '{request.WeaponId}' loadout'ta yok veya zaten açık.", this);
        }

        private void OnWeaponUpgradePurchased(WeaponUpgradePurchasedEvent purchase)
        {
            WeaponRuntime weapon = GetArsenal()?.FindWeapon(purchase.WeaponId);
            if (weapon != null && weapon.TryUpgrade(purchase.UpgradeType))
            {
                Debug.Log($"{LogPrefix}Yükseltme: {weapon.Data.DisplayName} {purchase.UpgradeType} " +
                          $"{weapon.GetUpgradeLevel(purchase.UpgradeType)}/{weapon.GetMaxUpgradeLevel(purchase.UpgradeType)}.", this);
                return;
            }

            Debug.LogWarning($"{LogPrefix}Yükseltme uygulanamadı: '{purchase.WeaponId}' {purchase.UpgradeType} (silah yok veya " +
                             "WeaponUpgradeConfig'teki en yüksek seviyede; market verisiyle uyuşmuyor).", this);
        }

        private void OnAmmoPurchased(AmmoPurchasedEvent purchase)
        {
            AddReserve(purchase.WeaponId, purchase.Amount);
        }

        private void OnAmmoPickedUp(AmmoPickedUpEvent pickup)
        {
            AddReserve(pickup.WeaponId, pickup.Amount);
        }

        private void AddReserve(string weaponId, int amount)
        {
            WeaponRuntime weapon = GetArsenal()?.FindWeapon(weaponId);
            if (weapon != null && weapon.AddReserve(amount))
            {
                Debug.Log($"{LogPrefix}Mermi: {weapon.Data.DisplayName} +{amount} (yedek {weapon.Reserve}).", this);
                return;
            }

            Debug.LogWarning($"{LogPrefix}Mermi eklenemedi: '{weaponId}' +{amount} (silah yok veya yedeği sonsuz).", this);
        }

        private void OnHealRequested(HealRequestedEvent request)
        {
            float healed = _health.Heal(request.Amount);
            Debug.Log($"{LogPrefix}İyileşme: +{healed:0.#} can ({_health.Current:0.#}/{_health.Max:0.#}).", this);
        }

        private WeaponArsenal GetArsenal()
        {
            WeaponArsenal arsenal = _weapons.Arsenal;
            if (arsenal == null)
            {
                Debug.LogWarning($"{LogPrefix}Silahlar henüz hazır değil (WeaponController.Start çalışmadı).", this);
            }

            return arsenal;
        }
    }
}
