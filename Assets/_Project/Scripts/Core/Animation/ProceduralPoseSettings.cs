using System;
using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>
    /// <see cref="ProceduralPose"/> ayarları (Animator yokken oynatılan yedek animasyon). Değerler GDD'de yok; his için seçildi.
    /// <see cref="AnimationDriver"/> üzerinde serileşir.
    /// </summary>
    [Serializable]
    public sealed class ProceduralPoseSettings
    {
        [Header("Saldırı: öne atılma")]
        [SerializeField, Min(0f), Tooltip("Görselin öne atılma mesafesi (m).")]
        private float _lungeDistance = 0.35f;

        [SerializeField, Min(0.01f), Tooltip("Öne atılıp geri dönme süresi (sn).")]
        private float _lungeDuration = 0.2f;

        [Header("Vuruş: sarsılma")]
        [SerializeField, Min(0f), Tooltip("Yana sarsılma genliği (m). Süre boyunca sönümlenir.")]
        private float _shakeAmplitude = 0.08f;

        [SerializeField, Min(0.01f), Tooltip("Sarsılma süresi (sn).")]
        private float _shakeDuration = 0.15f;

        [SerializeField, Min(0f), Tooltip("Sarsılma frekansı (Hz).")]
        private float _shakeFrequency = 25f;

        [Header("Ölüm: devrilme")]
        [SerializeField, Range(0f, 180f), Tooltip("Devrilme açısı (derece). Yön her ölümde rastgele.")]
        private float _toppleAngle = 90f;

        [SerializeField, Min(0.01f), Tooltip("Devrilme süresi (sn). Düşüş gibi hızlanır.")]
        private float _toppleDuration = 0.4f;

        [SerializeField, Min(0f), Tooltip("Devrilirken görselin yükselme miktarı (m). Gövde yarıçapı kadar olursa yatan kapsül yere gömülmez.")]
        private float _toppleLift;

        [Header("Yürüme: nabız")]
        [SerializeField, Min(0f), Tooltip("Yürürken görselin nabız gibi büyüme oranı (0.15 = %15 büyüyüp geri döner). 0 = kapalı. Patlayan (GDD §18).")]
        private float _walkPulseScale;

        [SerializeField, Min(0.01f), Tooltip("Nabız frekansı (Hz).")]
        private float _walkPulseFrequency = 2.5f;

        [SerializeField, Min(0f), Tooltip("Bu hızın (m/sn) üstünde yürüyor sayılır.")]
        private float _walkPulseMinSpeed = 0.2f;

        [SerializeField, Min(0.01f), Tooltip("Durunca sönme / yürüyünce açılma süresi (sn).")]
        private float _walkPulseBlendTime = 0.2f;

        public ProceduralPoseSettings()
        {
        }

        public ProceduralPoseSettings(
            float lungeDistance,
            float lungeDuration,
            float shakeAmplitude,
            float shakeDuration,
            float shakeFrequency,
            float toppleAngle,
            float toppleDuration,
            float toppleLift,
            float walkPulseScale = 0f,
            float walkPulseFrequency = 2.5f,
            float walkPulseMinSpeed = 0.2f,
            float walkPulseBlendTime = 0.2f)
        {
            _lungeDistance = lungeDistance;
            _lungeDuration = lungeDuration;
            _shakeAmplitude = shakeAmplitude;
            _shakeDuration = shakeDuration;
            _shakeFrequency = shakeFrequency;
            _toppleAngle = toppleAngle;
            _toppleDuration = toppleDuration;
            _toppleLift = toppleLift;
            _walkPulseScale = walkPulseScale;
            _walkPulseFrequency = walkPulseFrequency;
            _walkPulseMinSpeed = walkPulseMinSpeed;
            _walkPulseBlendTime = walkPulseBlendTime;
        }

        public float LungeDistance => _lungeDistance;

        public float LungeDuration => _lungeDuration;

        public float ShakeAmplitude => _shakeAmplitude;

        public float ShakeDuration => _shakeDuration;

        public float ShakeFrequency => _shakeFrequency;

        public float ToppleAngle => _toppleAngle;

        public float ToppleDuration => _toppleDuration;

        public float ToppleLift => _toppleLift;

        public float WalkPulseScale => _walkPulseScale;

        public float WalkPulseFrequency => _walkPulseFrequency;

        public float WalkPulseMinSpeed => _walkPulseMinSpeed;

        public float WalkPulseBlendTime => _walkPulseBlendTime;
    }
}
