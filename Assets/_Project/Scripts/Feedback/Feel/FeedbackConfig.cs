using System.Collections.Generic;
using UnityEngine;

namespace LastHaven.Feedback
{
    /// <summary>
    /// Oyun hissi eşlemesi (GDD §16; <see cref="FeedbackDirector"/> + <see cref="ScreenVignette"/>): olay → kamera sarsıntısı, vuruş
    /// donması (hit-stop), ekran kenarı flaşı ve düşük can nabzı. Sarsıntı şiddeti 0..1 (1 ≈ güçlü darbe); genlik, süre, uzaklık
    /// zayıflaması ve aç/kapa kamera ayarlarında (<c>CameraConfig</c>). Asset'i üretici oluşturur: <c>Data/Feedback/FeedbackConfig.asset</c>
    /// (silah ve ağır düşman listelerini ilk oluşturmada doldurur).
    /// </summary>
    public sealed class FeedbackConfig : ScriptableObject
    {
        [Header("Atış sarsıntısı (sadece oyuncunun atışları)")]
        [SerializeField, Min(0f), Tooltip("Listede olmayan silahlar (tabanca, tüfek): minik.")]
        private float _shotShake = 0.06f;

        [SerializeField, Tooltip("Silaha özel şiddet, silah id'siyle (pompalı: orta).")]
        private List<WeaponShake> _weaponShakes = new List<WeaponShake>();

        [Header("Ölüm sarsıntısı")]
        [SerializeField, Min(0f), Tooltip("Normal düşman ölümü: küçük.")]
        private float _killShake = 0.12f;

        [SerializeField, Tooltip("Normal ölüm sadece oyuncunun öldürmesinde sarsar (taret/tuzak öldürmeleri sarsmaz).")]
        private bool _killShakePlayerKillsOnly = true;

        [SerializeField, Min(0f), Tooltip("Ağır düşman (Tank, boss) ölümü: büyük. Kim öldürürse öldürsün; uzaklıkla zayıflar.")]
        private float _heavyKillShake = 0.6f;

        [SerializeField, Tooltip("Ağır sayılan düşmanların id'leri (EnemyData.Id).")]
        private List<string> _heavyEnemyIds = new List<string>();

        [Header("Vuruş donması (hit-stop)")]
        [SerializeField, Min(0f), Tooltip("Ağır düşman ölümünde oyun bu kadar donar (sn, gerçek zaman). 0 = kapalı.")]
        private float _heavyKillHitStop = 0.06f;

        [SerializeField, Tooltip("Donma sadece oyuncunun öldürmesinde (uzaktaki taret öldürmesi oyunu dondurmaz).")]
        private bool _hitStopPlayerKillsOnly = true;

        [Header("Patlama")]
        [SerializeField, Min(0f), Tooltip("Patlayan / Zincirleme Son patlaması (uzaklıkla zayıflar).")]
        private float _explosionShake = 0.35f;

        [Header("Ekran kenarı: hasar flaşı")]
        [SerializeField, Range(0f, 1f), Tooltip("Oyuncu hasar alınca kırmızı kenarın tepe ağırlığı (Volume weight).")]
        private float _damageFlashWeight = 1f;

        [SerializeField, Min(0f), Tooltip("Flaşın sönme süresi (sn).")]
        private float _damageFlashDuration = 0.35f;

        [Header("Ekran kenarı: düşük can nabzı")]
        [SerializeField, Range(0f, 1f), Tooltip("Can oranı bunun altındayken kenar nabız gibi atar (GDD: %30).")]
        private float _lowHealthThreshold = 0.3f;

        [SerializeField, Min(0.05f), Tooltip("Bir nabzın süresi (sn).")]
        private float _lowHealthPulsePeriod = 0.9f;

        [SerializeField, Range(0f, 1f), Tooltip("Nabzın en sönük ağırlığı.")]
        private float _lowHealthMinWeight = 0.2f;

        [SerializeField, Range(0f, 1f), Tooltip("Nabzın en parlak ağırlığı.")]
        private float _lowHealthMaxWeight = 0.65f;

        public IReadOnlyList<WeaponShake> WeaponShakes => _weaponShakes;

        public IReadOnlyList<string> HeavyEnemyIds => _heavyEnemyIds;

        public float ExplosionShake => _explosionShake;

        /// <summary>Oyuncunun bu silahla atışının sarsıntısı.</summary>
        public float GetShotShake(string weaponId)
        {
            return FeedbackRules.GetShotShake(weaponId, _weaponShakes, _shotShake);
        }

        public bool IsHeavy(string enemyId)
        {
            return FeedbackRules.IsHeavy(enemyId, _heavyEnemyIds);
        }

        public float GetKillShake(bool heavy, bool killedByPlayer)
        {
            return FeedbackRules.GetKillShake(heavy, killedByPlayer, _killShake, _heavyKillShake, _killShakePlayerKillsOnly);
        }

        public float GetKillHitStop(bool heavy, bool killedByPlayer)
        {
            return FeedbackRules.GetKillHitStop(heavy, killedByPlayer, _heavyKillHitStop, _hitStopPlayerKillsOnly);
        }

        public VignetteSettings CreateVignetteSettings()
        {
            return new VignetteSettings(_damageFlashWeight, _damageFlashDuration, _lowHealthThreshold, _lowHealthPulsePeriod,
                _lowHealthMinWeight, _lowHealthMaxWeight);
        }
    }
}
