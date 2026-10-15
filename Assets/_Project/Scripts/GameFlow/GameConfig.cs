using LastHaven.Core;
using UnityEngine;

namespace LastHaven.GameFlow
{
    /// <summary>
    /// Oyun akışı ayarları (GDD §3, §5, §9, §15). Asset'i üretici oluşturur: <c>Data/GameFlow/GameConfig.asset</c>.
    /// Yeniden doğma kuralları <see cref="IGameRules"/> olarak diğer modüllere açılır. Toplam gece sayısı burada değil,
    /// <see cref="IWaveService.TotalNights"/>'tan gelir.
    /// </summary>
    public sealed class GameConfig : ScriptableObject, IGameRules
    {
        [Header("Gündüz (GDD §9)")]
        [SerializeField, Min(0f), Tooltip("Gündüz süresi (sn). \"Hazırım\" (Enter) ile erken bitirilebilir.")]
        private float _dayDuration = 120f;

        [SerializeField, Min(0f), Tooltip("Son uyarı süresi (sn): gündüzün bu kadarı kalınca büyük geri sayım ve kırmızı nabız. 0 = uyarı yok.")]
        private float _finalWarningDuration = 15f;

        [Header("Alacakaranlık (GDD §3)")]
        [SerializeField, Min(0f), Tooltip("Gündüzden geceye geçiş süresi (sn). Gece dalgası bu süre bitince başlar. Işık da bu sürede geceye geçer.")]
        private float _duskDuration = 4f;

        [Header("Şafak")]
        [SerializeField, Min(0f), Tooltip("Şafak başında geceden gündüze ışık geçişi (sn). GDD'de yok. 0 = anında.")]
        private float _dawnLightTransitionDuration = 3f;

        [Header("Erken hazır bonusu (GDD §9)")]
        [SerializeField, Min(0), Tooltip("Gündüzden kalan her adım için verilen para.")]
        private int _earlyReadyBonusPerStep = 5;

        [SerializeField, Min(1f), Tooltip("Bonus adımı (sn): kalan her bu kadar saniye için bir kez bonus verilir.")]
        private float _earlyReadyStepSeconds = 10f;

        [Header("Oyuncunun yeniden doğması (GDD §5)")]
        [SerializeField, Min(0f), Tooltip("Ölümden sonra yeniden doğma süresi (sn).")]
        private float _playerRespawnDelay = 10f;

        [SerializeField, Tooltip("Doğma noktası: üs merkezine (jeneratör) göre dünya uzayı konum (m). Varsayılan jeneratörün 2.5 m güneyi.")]
        private Vector3 _playerRespawnOffset = new Vector3(0f, 0f, -2.5f);

        public float DayDuration => _dayDuration;

        public float FinalWarningDuration => _finalWarningDuration;

        public float DuskDuration => _duskDuration;

        /// <summary>Şafakta geceden gündüze ışık geçişi (sn).</summary>
        public float DawnLightTransitionDuration => _dawnLightTransitionDuration;

        public int EarlyReadyBonusPerStep => _earlyReadyBonusPerStep;

        public float EarlyReadyStepSeconds => _earlyReadyStepSeconds;

        /// <inheritdoc />
        public float PlayerRespawnDelay => _playerRespawnDelay;

        /// <inheritdoc />
        public Vector3 PlayerRespawnOffset => _playerRespawnOffset;

        /// <summary>Durum makinesinin ayarları (o anki değerlerle).</summary>
        public GameFlowSettings CreateFlowSettings()
        {
            return new GameFlowSettings(
                _dayDuration, _duskDuration, _finalWarningDuration, _earlyReadyBonusPerStep, _earlyReadyStepSeconds);
        }
    }
}
