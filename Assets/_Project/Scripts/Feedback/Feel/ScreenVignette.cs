using LastHaven.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace LastHaven.Feedback
{
    /// <summary>
    /// [Systems]/Feedback/ScreenVignette: ekran kenarında kırmızı vignette (GDD §16). Kırmızı Vignette profili olan global bir
    /// <see cref="Volume"/>'ün ağırlığını sürer (gün/gece Volume'lerinin üstünde, öncelik yüksek): hasar flaşı (<see cref="Flash"/>,
    /// <see cref="FeedbackDirector"/> çağırır) ve oyuncunun canı %30'un altındayken nabız. Mantık <see cref="VignettePulse"/>'ta;
    /// gerçek zamanla ilerler (duraklatma ve donmada da atar). Ağırlık sadece değişince yazılır.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ScreenVignette : MonoBehaviour
    {
        [SerializeField, Tooltip("Flaş ve nabız ayarları.")]
        private FeedbackConfig _config;

        [SerializeField, Tooltip("Kırmızı kenar profili olan global Volume (weight bu bileşenle sürülür, başta 0).")]
        private Volume _volume;

        private VignettePulse _pulse;
        private IPlayer _player;

        /// <summary>Hasar flaşını başlatır.</summary>
        public void Flash()
        {
            _pulse?.Flash();
        }

        private void Awake()
        {
            if (_config == null || _volume == null)
            {
                Debug.LogError("ScreenVignette: config veya Volume atanmamış; Tools/Last Haven/Build Game Scene çalıştır.", this);
                enabled = false;
                return;
            }

            _pulse = new VignettePulse(_config.CreateVignetteSettings());
            _volume.weight = 0f;
        }

        private void Start()
        {
            if (!ServiceLocator.TryGet(out _player))
            {
                Debug.LogWarning("ScreenVignette: IPlayer bulunamadı; düşük can nabzı olmayacak.", this);
            }
        }

        private void Update()
        {
            bool alive = _player != null && _player.IsAlive;
            float ratio = alive && _player.MaxHealth > 0f ? _player.CurrentHealth / _player.MaxHealth : 1f;
            float weight = _pulse.Tick(Time.unscaledDeltaTime, ratio, alive);
            if (!Mathf.Approximately(weight, _volume.weight))
            {
                _volume.weight = weight;
            }
        }
    }
}
