using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>
    /// Oyuncu bir ganimet sandığını açtı (GDD §9): ödül seçildi ve saçıldı (para jeton, yakıt/mermi toplanabilir eşya olarak).
    /// Ödül henüz cüzdana girmedi; oyuncu topladıkça eklenir. Ses, efekt ve istatistik dinler.
    /// </summary>
    public readonly struct LootCollectedEvent : IEvent
    {
        public LootCollectedEvent(Vector3 position, LootRewardType rewardType, int amount, bool isFar)
        {
            Position = position;
            RewardType = rewardType;
            Amount = amount;
            IsFar = isFar;
        }

        /// <summary>Sandığın yeri (zemin, dünya).</summary>
        public Vector3 Position { get; }

        public LootRewardType RewardType { get; }

        /// <summary>Para, yakıt birimi veya mermi paketi adedi.</summary>
        public int Amount { get; }

        /// <summary>Sandık uzak halkadaydı (GDD §9: merkeze ≥ 32 m; daha iyi ödül tablosu).</summary>
        public bool IsFar { get; }
    }

    /// <summary>
    /// Oyuncu ganimetten bir mermi kutusu topladı: silahın yedek mermisine eklenmeli. Economy'deki ganimet dağıtıcısı mermi
    /// paketindeki her silah için bir kez gönderir, Player uygular (kilitli silahın yedeği de artar, açılınca hazır olur).
    /// </summary>
    public readonly struct AmmoPickedUpEvent : IEvent
    {
        public AmmoPickedUpEvent(string weaponId, int amount)
        {
            WeaponId = weaponId;
            Amount = amount;
        }

        /// <summary>Silahın id'si (<c>WeaponData.Id</c>).</summary>
        public string WeaponId { get; }

        /// <summary>Eklenecek yedek mermi.</summary>
        public int Amount { get; }
    }
}
