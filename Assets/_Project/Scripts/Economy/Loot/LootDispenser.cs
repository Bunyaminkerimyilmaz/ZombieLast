using System.Collections.Generic;
using LastHaven.Core;
using UnityEngine;
using Random = System.Random;

namespace LastHaven.Economy
{
    /// <summary>
    /// [Systems]/Loot: açılan sandığın ödülünü seçer (<see cref="LootTable"/>, GDD §9) ve saçar: para <see cref="CoinDropper"/> ile
    /// jeton olarak; yakıt ve mermi paketi toplanabilir <see cref="LootPickup"/> olarak (havuzdan). Ardından
    /// <see cref="LootCollectedEvent"/> gönderir. Toplanan bidon yakıtı cüzdana ekler (kapasitede doyar); mermi kutusu paketteki
    /// her silah için <see cref="AmmoPickedUpEvent"/> gönderir. Mıknatıs jetonlarınkiyle aynı (<see cref="CoinDropper.Magnet"/>).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LootDispenser : MonoBehaviour
    {
        private const string LogPrefix = "[Economy] ";

        [SerializeField, Tooltip("Ödül tablosu, toplanabilir prefab'lar ve hareketleri. Üretici bağlar.")]
        private LootConfig _config;

        [SerializeField, Tooltip("Para ödülünü jeton olarak saçan bileşen ([Systems]/Economy). Sahne adımı bağlar.")]
        private CoinDropper _coins;

        private readonly List<Vector3> _landings = new List<Vector3>();
        private LootTable _table;
        private CoinScatter _scatter;
        private Random _random;
        private IPoolService _pool;
        private IWallet _wallet;

        /// <summary>Bu karenin mıknatısı (oyuncunun toplama yarıçapı); jeton düşürücü yoksa <see cref="MagnetTarget.None"/>.</summary>
        public MagnetTarget Magnet => _coins != null ? _coins.Magnet : MagnetTarget.None;

        /// <summary>
        /// Sandığın uzaklığına göre ödül çeker, <paramref name="position"/> (zemin) çevresine saçar ve olayı gönderir.
        /// Tablo yoksa veya ödül boşsa hiçbir şey saçmaz.
        /// </summary>
        public LootRoll Dispense(Vector3 position, float distance)
        {
            if (_table == null)
            {
                return default;
            }

            LootRoll roll = _table.Roll(distance, _random);
            if (roll.IsEmpty)
            {
                Debug.LogWarning($"{LogPrefix}Ganimet tablosunda seçilebilir ödül yok ({(roll.IsFar ? "uzak" : "yakın")}).", this);
                return roll;
            }

            if (roll.Type == LootRewardType.Money)
            {
                if (_coins != null)
                {
                    _coins.Drop(position, roll.Amount);
                }
            }
            else
            {
                Launch(_config.GetPickupPrefab(roll.Type), position, roll.Amount);
            }

            EventBus<LootCollectedEvent>.Raise(new LootCollectedEvent(position, roll.Type, roll.Amount, roll.IsFar));
            return roll;
        }

        /// <summary>Ödül oyuncuya değdi: etkisini uygular, eşyayı havuza iade eder.</summary>
        public void Collect(LootPickup pickup)
        {
            int amount = pickup.Amount;
            switch (pickup.RewardType)
            {
                case LootRewardType.Fuel:
                    _wallet?.Add(WalletResource.Fuel, amount);
                    Debug.Log($"{LogPrefix}Ganimet toplandı: +{amount} yakıt (yakıt {_wallet?.Fuel}).", this);
                    break;
                case LootRewardType.AmmoPack:
                    GrantAmmo(amount);
                    break;
                case LootRewardType.Money:
                    _wallet?.Add(WalletResource.Money, amount);
                    break;
            }

            _pool.Release(pickup.gameObject);
        }

        /// <summary>Eşyanın ömrü doldu: havuza iade eder.</summary>
        public void Expire(LootPickup pickup)
        {
            _pool.Release(pickup.gameObject);
        }

        private void Awake()
        {
            if (_config == null || _config.LootTable == null)
            {
                Debug.LogError("LootDispenser: LootConfig veya ödül tablosu atanmamış; Tools/Last Haven/Rebuild All çalıştır.", this);
                enabled = false;
                return;
            }

            if (_coins == null)
            {
                Debug.LogWarning("LootDispenser: CoinDropper atanmamış; para ödülleri saçılmayacak ve eşyalar toplanamayacak.", this);
            }

            _random = new Random();
            _scatter = new CoinScatter(_random);
            _table = _config.LootTable.CreateTable();
        }

        private void Start()
        {
            if (!ServiceLocator.TryGet(out _pool))
            {
                Debug.LogWarning("LootDispenser: IPoolService bulunamadı; yakıt ve mermi ödülleri düşmeyecek.", this);
            }
            else
            {
                Prewarm(_config.FuelPickupPrefab);
                Prewarm(_config.AmmoPickupPrefab);
            }

            if (!ServiceLocator.TryGet(out _wallet))
            {
                Debug.LogWarning("LootDispenser: IWallet bulunamadı; toplanan yakıt eklenmeyecek.", this);
            }
        }

        private void Prewarm(LootPickup prefab)
        {
            if (prefab != null)
            {
                _pool.Prewarm(prefab.gameObject, _config.PickupPrewarm);
            }
        }

        private void Launch(LootPickup prefab, Vector3 position, int amount)
        {
            if (prefab == null || _pool == null)
            {
                Debug.LogWarning($"{LogPrefix}Ganimet eşyası düşürülemedi: prefab veya havuz yok.", this);
                return;
            }

            CoinMotionSettings settings = _config.PickupMotion;
            _scatter.GetLandings(position, 1, settings, _landings);
            Vector3 start = position + Vector3.up * settings.SpawnHeight;
            LootPickup pickup = _pool.Get(prefab, start, Quaternion.identity);
            pickup.Launch(
                this, amount, settings, start, _landings[0],
                _config.PickupLifetime, _config.PickupBlinkDuration, _scatter.NextAngle());
        }

        private void GrantAmmo(int packs)
        {
            IReadOnlyList<LootAmmoGrant> grants = _config.LootTable.AmmoPack;
            for (int i = 0; i < grants.Count; i++)
            {
                int amount = grants[i].Amount * packs;
                if (amount > 0)
                {
                    EventBus<AmmoPickedUpEvent>.Raise(new AmmoPickedUpEvent(grants[i].WeaponId, amount));
                }
            }

            Debug.Log($"{LogPrefix}Ganimet toplandı: {packs} mermi paketi.", this);
        }
    }
}
