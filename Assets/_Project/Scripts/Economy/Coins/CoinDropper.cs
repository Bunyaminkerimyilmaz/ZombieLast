using System.Collections.Generic;
using LastHaven.Core;
using UnityEngine;
using Random = System.Random;

namespace LastHaven.Economy
{
    /// <summary>
    /// [Systems]/Economy: <see cref="EnemyKilledEvent"/>'te ödülü jetonlara bölüp ölüm noktasının çevresine saçar (GDD §10).
    /// Toplam değer = ödül × kombo çarpanı × <see cref="IRunModifiers"/> CoinValue; öldürme önce <see cref="ComboService"/>'e
    /// işlenir, çarpan oradan gelir (öldürme anındaki değer). Jetonlar <see cref="IPoolService"/>'ten
    /// alınır ve oyun başında ön ısıtılır. Her kare oyuncunun mıknatısını (<see cref="IPlayer.PickupRadius"/>) jetonlara verir;
    /// değen jetonun değerini <see cref="IWallet"/>'a ekler ve <see cref="CoinCollectedEvent"/> gönderir.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CoinDropper : MonoBehaviour
    {
        private const float BaseCoinValueMultiplier = 1f;

        [SerializeField, Tooltip("Jeton prefab'ı, ön ısıtma, bölme ve hareket ayarları. Üretici bağlar.")]
        private EconomyConfig _config;

        [SerializeField, Tooltip("Öldürmeleri sayan ve jeton çarpanını veren kombo servisi. Sahne adımı bağlar.")]
        private ComboService _combo;

        private readonly List<int> _values = new List<int>();
        private readonly List<Vector3> _landings = new List<Vector3>();
        private CoinSplit _split;
        private CoinScatter _scatter;
        private IPoolService _pool;
        private IWallet _wallet;
        private IPlayer _player;
        private IRunModifiers _modifiers;
        private MagnetTarget _magnet;

        /// <summary>Bu karenin mıknatısı; oyuncu yoksa <see cref="MagnetTarget.None"/>.</summary>
        public MagnetTarget Magnet => _magnet;

        /// <summary>
        /// <paramref name="totalValue"/>'yu jetonlara bölüp <paramref name="position"/> (ayak) çevresine saçar.
        /// Değer 0 ise ya da havuz yoksa bir şey yapmaz.
        /// </summary>
        public void Drop(Vector3 position, int totalValue)
        {
            if (_pool == null || _split.Split(totalValue, _values) == 0)
            {
                return;
            }

            CoinMotionSettings settings = _config.CoinMotion;
            _scatter.GetLandings(position, _values.Count, settings, _landings);
            Vector3 start = position + Vector3.up * settings.SpawnHeight;
            for (int i = 0; i < _values.Count; i++)
            {
                Coin coin = _pool.Get(_config.CoinPrefab, start, Quaternion.identity);
                coin.Launch(
                    this, _values[i], settings, start, _landings[i],
                    _config.CoinLifetime, _config.CoinBlinkDuration, _scatter.NextAngle());
            }
        }

        /// <summary>Jeton oyuncuya değdi: değeri cüzdana ekler, olayı gönderir, jetonu havuza iade eder (jetonlar havuzdan gelir).</summary>
        public void Collect(Coin coin)
        {
            if (coin.Value > 0)
            {
                _wallet?.Add(WalletResource.Money, coin.Value);
                EventBus<CoinCollectedEvent>.Raise(new CoinCollectedEvent(coin.Value, coin.transform.position));
            }

            _pool.Release(coin.gameObject);
        }

        /// <summary>Jetonun ömrü doldu: havuza iade eder.</summary>
        public void Expire(Coin coin)
        {
            _pool.Release(coin.gameObject);
        }

        private void Awake()
        {
            if (_config == null || _config.CoinPrefab == null)
            {
                Debug.LogError("CoinDropper: EconomyConfig veya jeton prefab'ı atanmamış; Tools/Last Haven/Rebuild All çalıştır.", this);
                enabled = false;
                return;
            }

            _split = _config.CreateCoinSplit();
            _scatter = new CoinScatter(new Random());
            if (_combo == null)
            {
                Debug.LogWarning("CoinDropper: ComboService atanmamış; kombo sayılmayacak ve jetonlar çarpansız düşecek.", this);
            }
        }

        private void Start()
        {
            if (!ServiceLocator.TryGet(out _pool))
            {
                Debug.LogWarning("CoinDropper: IPoolService bulunamadı; düşmanlar jeton düşürmeyecek.", this);
            }
            else
            {
                _pool.Prewarm(_config.CoinPrefab.gameObject, _config.CoinPrewarm);
            }

            if (!ServiceLocator.TryGet(out _wallet))
            {
                Debug.LogWarning("CoinDropper: IWallet bulunamadı; toplanan jetonlar paraya eklenmeyecek.", this);
            }

            if (!ServiceLocator.TryGet(out _player))
            {
                Debug.LogWarning("CoinDropper: IPlayer bulunamadı; jetonlar toplanamayacak.", this);
            }

            // Yoksa jeton değeri çarpansız (WeaponController zaten uyarır).
            ServiceLocator.TryGet(out _modifiers);
        }

        private void OnEnable()
        {
            EventBus<EnemyKilledEvent>.Subscribe(OnEnemyKilled);
        }

        private void OnDisable()
        {
            EventBus<EnemyKilledEvent>.Unsubscribe(OnEnemyKilled);
        }

        private void Update()
        {
            _magnet = _player != null
                ? new MagnetTarget(
                    _player.Position + Vector3.up * _config.CoinMotion.CollectHeight, _player.PickupRadius, _player.IsAlive)
                : MagnetTarget.None;
        }

        private void OnEnemyKilled(EnemyKilledEvent killed)
        {
            float comboMultiplier = _combo != null
                ? _combo.RegisterKill(killed.KilledByPlayer)
                : ComboTracker.BaseMultiplier;
            float valueMultiplier = _modifiers != null
                ? _modifiers.Get(StatType.CoinValue, BaseCoinValueMultiplier)
                : BaseCoinValueMultiplier;
            Drop(killed.Position, CoinValue.Calculate(killed.MoneyReward, comboMultiplier, valueMultiplier));
        }
    }
}
