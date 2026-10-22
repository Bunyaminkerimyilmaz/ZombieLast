using UnityEngine;

namespace LastHaven.UI
{
    /// <summary>
    /// HUD'un oyun hissi ayarları (GDD §16): dünyadan ekrana yansıtılan hasar sayıları ve "+12" jeton yazıları
    /// (<see cref="HudFloatingTextController"/>), evre geçişlerinde ekran ortasındaki büyük başlık (<see cref="HudBannerController"/>).
    /// Asset'i üretici oluşturur: <c>Data/UI/HudFeelConfig.asset</c>. Metinler UXML'de, renkler <c>Theme.uss</c>'te.
    /// </summary>
    public sealed class HudFeelConfig : ScriptableObject
    {
        [Header("Uçan yazılar (ortak)")]
        [SerializeField, Min(1), Tooltip("Havuzdaki etiket sayısı (oyun başında oluşturulur). Hepsi kullanımdayken en eskisi yeniden kullanılır.")]
        private int _floatingTextPoolSize = 40;

        [SerializeField, Min(0f), Tooltip("Yazı ömrü boyunca bu kadar yükselir (m, dünya).")]
        private float _riseHeight = 1.2f;

        [SerializeField, Range(0f, 1f), Tooltip("Ömrün bu kadarında yazı büyükten normal boya iner (vuruş hissi).")]
        private float _popFraction = 0.15f;

        [SerializeField, Min(1f), Tooltip("Yazının ilk boyu (normal boyun katı).")]
        private float _popScale = 1.6f;

        [SerializeField, Range(0f, 1f), Tooltip("Ömrün bu noktasından sonra yazı söner.")]
        private float _fadeStartFraction = 0.6f;

        [Header("Hasar sayıları (ShotHitEvent)")]
        [SerializeField, Min(0.05f), Tooltip("Ömür (sn).")]
        private float _damageLifetime = 0.8f;

        [SerializeField, Tooltip("Sadece oyuncunun mermileri (taret isabetleri sayı göstermez).")]
        private bool _damagePlayerShotsOnly = true;

        [SerializeField, Min(0f), Tooltip("İsabet noktasının bu kadar üstünden başlar (m).")]
        private float _damageHeight = 0.4f;

        [SerializeField, Min(0f), Tooltip("Yatayda en fazla bu kadar rastgele kayar (m); yan yana sayılar üst üste binmesin.")]
        private float _damageJitter = 0.35f;

        [SerializeField, Min(0f), Tooltip("Bu hasar ve altı en küçük boyda.")]
        private float _smallDamage = 10f;

        [SerializeField, Min(0f), Tooltip("Bu hasar ve üstü en büyük boyda ve 'büyük hasar' renginde.")]
        private float _bigDamage = 60f;

        [SerializeField, Min(0.1f), Tooltip("En küçük boy (katsayı).")]
        private float _smallDamageScale = 0.75f;

        [SerializeField, Min(0.1f), Tooltip("En büyük boy (katsayı).")]
        private float _bigDamageScale = 1.7f;

        [Header("Jeton yazıları (CoinCollectedEvent)")]
        [SerializeField, Min(0.05f), Tooltip("Ömür (sn).")]
        private float _coinLifetime = 1f;

        [SerializeField, Min(0f), Tooltip("Toplama noktasının bu kadar üstünden başlar (m).")]
        private float _coinHeight = 2.2f;

        [SerializeField, Min(0f), Tooltip("Son yazıdan bu süre içinde (sn) toplanan jeton aynı yazıya eklenir (+3 +4 → +7).")]
        private float _coinMergeWindow = 0.35f;

        [Header("Evre başlıkları")]
        [SerializeField, Min(0f), Tooltip("Belirme süresi (sn).")]
        private float _bannerFadeIn = 0.25f;

        [SerializeField, Min(0f), Tooltip("Tam görünür kalma süresi (sn).")]
        private float _bannerHold = 1.3f;

        [SerializeField, Min(0f), Tooltip("Sönme süresi (sn).")]
        private float _bannerFadeOut = 0.55f;

        [SerializeField, Min(1f), Tooltip("Belirirken bu boydan normal boya iner (katsayı).")]
        private float _bannerStartScale = 1.3f;

        [SerializeField, Min(1), Tooltip("Başlık gösterilirken en fazla bu kadar başlık sırada bekler; fazlası atılır.")]
        private int _bannerQueueLimit = 3;

        public int FloatingTextPoolSize => _floatingTextPoolSize;

        public float RiseHeight => _riseHeight;

        public float DamageLifetime => _damageLifetime;

        public bool DamagePlayerShotsOnly => _damagePlayerShotsOnly;

        public float DamageHeight => _damageHeight;

        public float DamageJitter => _damageJitter;

        public float BigDamage => _bigDamage;

        public float CoinLifetime => _coinLifetime;

        public float CoinHeight => _coinHeight;

        public float CoinMergeWindow => _coinMergeWindow;

        public int BannerQueueLimit => _bannerQueueLimit;

        public FloatingTextMotion CreateMotion()
        {
            return new FloatingTextMotion(_popFraction, _popScale, _fadeStartFraction);
        }

        /// <summary>Hasara göre yazı boyu (küçük..büyük hasar arasında doğrusal).</summary>
        public float GetDamageScale(float damage)
        {
            return FloatingTextMotion.DamageScale(damage, _smallDamage, _bigDamage, _smallDamageScale, _bigDamageScale);
        }

        public BannerTiming CreateBannerTiming()
        {
            return new BannerTiming(_bannerFadeIn, _bannerHold, _bannerFadeOut, _bannerStartScale);
        }
    }
}
