using System.Collections.Generic;
using UnityEngine;

namespace LastHaven.Economy
{
    /// <summary>
    /// Ekonomi ayarları (GDD §10). Asset'i üretici oluşturur: <c>Data/Economy/EconomyConfig.asset</c>.
    /// Erken hazır bonusu burada değil, <c>GameConfig</c>'te (GameFlow hesaplar, <see cref="Core.EarlyReadyEvent"/> taşır).
    /// Jeton alanlarını <see cref="CoinDropper"/>, kombo alanlarını <see cref="ComboService"/>, satış alanını Gün 8 kullanır.
    /// </summary>
    public sealed class EconomyConfig : ScriptableObject
    {
        [Header("Başlangıç (GDD §10)")]
        [SerializeField, Min(0), Tooltip("Run başındaki para.")]
        private int _startingMoney = 300;

        [SerializeField, Min(0), Tooltip("Run başındaki yakıt (birim).")]
        private int _startingFuel = 100;

        [Header("Faiz (GDD §10)")]
        [SerializeField, Range(0f, 1f), Tooltip("Şafakta mevcut paranın bu oranı eklenir (0.1 = %10). Aşağı yuvarlanır.")]
        private float _interestRate = 0.1f;

        [SerializeField, Min(0), Tooltip("Şafak başına en fazla faiz (para). 0 = faiz yok.")]
        private int _interestCap = 50;

        [Header("Jetonlar (GDD §10)")]
        [SerializeField, Min(0f), Tooltip("Yere düşen jetonun ömrü (sn); sonra kaybolur.")]
        private float _coinLifetime = 12f;

        [SerializeField, Min(0f), Tooltip("Ömrün son bu kadar saniyesinde jeton yanıp söner.")]
        private float _coinBlinkDuration = 3f;

        [SerializeField, Min(1), Tooltip("Bir ödülün bölündüğü en az jeton sayısı.")]
        private int _minCoinsPerDrop = 1;

        [SerializeField, Min(1), Tooltip("Bir ödülün bölündüğü en fazla jeton sayısı.")]
        private int _maxCoinsPerDrop = 5;

        [SerializeField, Min(1), Tooltip("Jeton başına hedef değer: jeton sayısı = ödül / bu (yukarı yuvarlı), sonra en az/en fazla " +
                                         "aralığına kırpılır. 5 → Yürüyen (10) 2, Koşucu (12) 3, Tank (60) 5 jeton. GDD'de yok.")]
        private int _targetCoinValue = 5;

        [Header("Jeton prefab'ı ve hareketi (GDD'de yok)")]
        [SerializeField, Tooltip("Havuzlanan jeton. Üretici bağlar.")]
        private Coin _coinPrefab;

        [SerializeField, Min(0), Tooltip("Oyun başında havuzda hazırlanan jeton sayısı. Aşılınca havuz büyür (uyarıyla). " +
                                         "×3 komboda Yürüyen 5 jeton düşürür; 12 sn'lik ömür boyunca yüzlerce jeton olabilir.")]
        private int _coinPrewarm = 200;

        [SerializeField, Tooltip("Saçılma yayı, dönme, yanıp sönme ve mıknatıs uçuşu.")]
        private CoinMotionSettings _coinMotion = new CoinMotionSettings();

        [Header("Kombo (GDD §10)")]
        [SerializeField, Min(0f), Tooltip("İki öldürme arasındaki en uzun süre (sn); dolunca kombo sıfırlanır.")]
        private float _comboWindow = 2.5f;

        [SerializeField, Tooltip("Kombo eşikleri, öldürme sayısına göre artan sırada.")]
        private List<ComboTier> _comboTiers = new List<ComboTier>
        {
            new ComboTier(5, 1.5f),
            new ComboTier(15, 2f),
            new ComboTier(30, 3f),
        };

        [Header("Satış (GDD §10)")]
        [SerializeField, Range(0f, 1f), Tooltip("Satılan yapıya harcanan toplam paranın iade oranı (0.5 = %50).")]
        private float _sellRefundRate = 0.5f;

        public int StartingMoney => _startingMoney;

        public int StartingFuel => _startingFuel;

        public float InterestRate => _interestRate;

        public int InterestCap => _interestCap;

        public float CoinLifetime => _coinLifetime;

        public float CoinBlinkDuration => _coinBlinkDuration;

        public int MinCoinsPerDrop => _minCoinsPerDrop;

        public int MaxCoinsPerDrop => _maxCoinsPerDrop;

        public int TargetCoinValue => _targetCoinValue;

        public Coin CoinPrefab => _coinPrefab;

        public int CoinPrewarm => _coinPrewarm;

        public CoinMotionSettings CoinMotion => _coinMotion ??= new CoinMotionSettings();

        public float ComboWindow => _comboWindow;

        public IReadOnlyList<ComboTier> ComboTiers => _comboTiers;

        public float SellRefundRate => _sellRefundRate;

        /// <summary>Faiz kuralı (o anki değerlerle).</summary>
        public InterestRule CreateInterestRule()
        {
            return new InterestRule(_interestRate, _interestCap);
        }

        /// <summary>Kombo takipçisi (o anki pencere ve eşiklerle).</summary>
        public ComboTracker CreateComboTracker()
        {
            return new ComboTracker(_comboWindow, _comboTiers);
        }

        /// <summary>Ödülü jetonlara bölme kuralı (o anki değerlerle).</summary>
        public CoinSplit CreateCoinSplit()
        {
            return new CoinSplit(_minCoinsPerDrop, _maxCoinsPerDrop, _targetCoinValue);
        }
    }
}
