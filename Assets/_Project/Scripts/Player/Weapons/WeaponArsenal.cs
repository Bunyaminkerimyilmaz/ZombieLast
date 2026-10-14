using System.Collections.Generic;
using LastHaven.Combat;
using LastHaven.Core;

namespace LastHaven.Player
{
    /// <summary>
    /// Oyuncunun silahları (saf C#): loadout'tan yuva başına bir <see cref="WeaponRuntime"/> kurar, seçimi <see cref="WeaponSlots"/>'ta
    /// tutar ve durum olaylarını (<see cref="WeaponChangedEvent"/>, <see cref="AmmoChangedEvent"/>, doldurma olayları) gönderir.
    /// Girdi ve atış <see cref="WeaponController"/>'da.
    /// </summary>
    public sealed class WeaponArsenal
    {
        private readonly WeaponRuntime[] _weapons;
        private readonly WeaponSlots _slots;

        /// <param name="modifiers">Null olabilir (run modifier'ları uygulanmaz).</param>
        public WeaponArsenal(IReadOnlyList<WeaponSlotDefinition> definitions, WeaponUpgradeConfig upgrades, IRunModifiers modifiers)
        {
            _weapons = new WeaponRuntime[definitions.Count];
            _slots = new WeaponSlots(definitions.Count);

            for (int i = 0; i < definitions.Count; i++)
            {
                WeaponData data = definitions[i].Weapon;
                if (data == null)
                {
                    continue;
                }

                var weapon = new WeaponRuntime(data, upgrades, modifiers);
                weapon.AmmoChanged += () => RaiseAmmoChanged(weapon);
                weapon.ReloadStarted += duration => EventBus<ReloadStartedEvent>.Raise(new ReloadStartedEvent(weapon.Id, duration));
                weapon.ReloadFinished += completed => EventBus<ReloadFinishedEvent>.Raise(new ReloadFinishedEvent(weapon.Id, completed));
                _weapons[i] = weapon;

                if (definitions[i].UnlockedAtStart)
                {
                    _slots.Unlock(i);
                }
            }

            // İlk açık yuvayı ele al (olay göndermeden; başlangıç durumu RaiseCurrentState ile gönderilir).
            _slots.TryCycle(1);
        }

        public int SlotCount => _slots.Count;

        /// <summary>Seçili yuva (0 tabanlı); açık silah yoksa <see cref="WeaponSlots.None"/>.</summary>
        public int CurrentSlot => _slots.Current;

        /// <summary>Eldeki silah; yoksa null.</summary>
        public WeaponRuntime Current => GetWeapon(_slots.Current);

        /// <summary>Yuvadaki silah; yuva geçersiz veya boşsa null.</summary>
        public WeaponRuntime GetWeapon(int slot)
        {
            return _slots.IsValid(slot) ? _weapons[slot] : null;
        }

        public bool IsUnlocked(int slot)
        {
            return _slots.IsUnlocked(slot);
        }

        /// <summary>Yuvayı açar. Elde silah yoksa onu ele alır. Yuva boşsa veya zaten açıksa false.</summary>
        public bool Unlock(int slot)
        {
            if (GetWeapon(slot) == null || !_slots.Unlock(slot))
            {
                return false;
            }

            if (_slots.Current == WeaponSlots.None)
            {
                Select(slot);
            }

            return true;
        }

        /// <summary>Açık ve farklı bir yuvaysa ele alır; eski silahın doldurması iptal olur. Değiştiyse true.</summary>
        public bool Select(int slot)
        {
            WeaponRuntime previous = Current;
            if (!_slots.TrySelect(slot))
            {
                return false;
            }

            // Geri dönülünce boş şarjör kendiliğinden dolar.
            previous?.CancelReload();
            RaiseCurrentState();
            return true;
        }

        /// <summary>Sıradaki (+1) veya önceki (-1) açık silahı ele alır; kilitliler atlanır.</summary>
        public bool Cycle(int direction)
        {
            return Select(_slots.FindNext(direction));
        }

        /// <summary>Eldeki silahı ve mermisini olay olarak gönderir (başlangıçta ve silah değişince).</summary>
        public void RaiseCurrentState()
        {
            WeaponRuntime weapon = Current;
            if (weapon == null)
            {
                return;
            }

            EventBus<WeaponChangedEvent>.Raise(new WeaponChangedEvent(CurrentSlot, weapon.Id, weapon.Data.DisplayName));
            RaiseAmmoChanged(weapon);
        }

        /// <summary>Şarjör kapasitesi dışarıdan değişti (ör. PlayerMagSize perk'i); HUD güncellensin.</summary>
        public void NotifyMagSizeChanged()
        {
            RaiseAmmoChanged(Current);
        }

        private void RaiseAmmoChanged(WeaponRuntime weapon)
        {
            // HUD sadece eldeki silahı gösterir; cepteki silahın değişimi (ör. yükseltme) ele alınınca gönderilir.
            if (weapon == null || weapon != Current)
            {
                return;
            }

            EventBus<AmmoChangedEvent>.Raise(
                new AmmoChangedEvent(weapon.Id, weapon.Ammo, weapon.MagSize, weapon.Reserve, weapon.HasInfiniteReserve));
        }
    }
}
