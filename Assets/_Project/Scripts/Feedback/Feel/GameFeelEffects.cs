using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Feedback
{
    /// <summary>
    /// Oyun hissi efektleri (<see cref="FeedbackDirector"/>'ün olay → his eşlemesi; saf köprü): oyuncunun atışı → minik sarsıntı
    /// (pompalı orta), düşman ölümü → küçük sarsıntı, ağır düşman (Tank, boss) ölümü → büyük sarsıntı + vuruş donması, patlama →
    /// sarsıntı, oyuncu hasarı → kırmızı ekran kenarı. Sarsıntılar <see cref="CameraShakeRequestedEvent"/> (konumlu) olarak istenir;
    /// kamera uzaklığa göre zayıflatır ve ayarlarda kapalıysa yok sayar. Değerler <see cref="FeedbackConfig"/>'te.
    /// </summary>
    public sealed class GameFeelEffects
    {
        private readonly FeedbackConfig _config;
        private readonly ScreenVignette _vignette;
        private readonly IHitStopService _hitStop;

        /// <param name="vignette">Null olabilir (ekran kenarı yok).</param>
        /// <param name="hitStop">Null olabilir (donma yok).</param>
        public GameFeelEffects(FeedbackConfig config, ScreenVignette vignette, IHitStopService hitStop)
        {
            _config = config;
            _vignette = vignette;
            _hitStop = hitStop;
        }

        public void OnWeaponFired(in WeaponFiredEvent fired)
        {
            if (fired.Source != null && fired.Source.CompareTag(GameTags.Player))
            {
                RequestShake(_config.GetShotShake(fired.WeaponId), fired.Origin);
            }
        }

        public void OnEnemyKilled(in EnemyKilledEvent killed)
        {
            bool heavy = _config.IsHeavy(killed.EnemyId);
            RequestShake(_config.GetKillShake(heavy, killed.KilledByPlayer), killed.Position);

            float hitStop = _config.GetKillHitStop(heavy, killed.KilledByPlayer);
            if (hitStop > 0f && _hitStop != null)
            {
                _hitStop.HitStop(hitStop);
            }
        }

        public void OnExplosion(in ExplosionEvent explosion)
        {
            RequestShake(_config.ExplosionShake, explosion.Position);
        }

        public void OnPlayerDamaged()
        {
            if (_vignette != null)
            {
                _vignette.Flash();
            }
        }

        private static void RequestShake(float strength, Vector3 position)
        {
            if (strength > 0f)
            {
                EventBus<CameraShakeRequestedEvent>.Raise(new CameraShakeRequestedEvent(strength, position));
            }
        }
    }
}
