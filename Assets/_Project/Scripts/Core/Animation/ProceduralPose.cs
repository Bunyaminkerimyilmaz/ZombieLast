using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>
    /// Animator olmayan görseller için prosedürel yedek animasyon (saf C#): saldırıda öne atılma, vuruşta yana sarsılma,
    /// ölümde verilen yöne devrilme, yürürken (açıksa) nabız gibi büyüyüp küçülme. Sonuç görselin ebeveyn uzayındaki dinlenme
    /// pozuna eklenen <see cref="Offset"/>, <see cref="Rotation"/> ve <see cref="Scale"/>'dir; dönüş ve ölçek görselin orijini
    /// (ayaklar) etrafındadır. Devrilme <see cref="Reset"/>'e kadar kalır.
    /// </summary>
    public sealed class ProceduralPose
    {
        private const float NotPlaying = -1f;
        private const float MinDirectionSqr = 0.0001f;

        private float _lungeTime = NotPlaying;
        private float _shakeTime = NotPlaying;
        private float _toppleTime = NotPlaying;
        private Vector3 _toppleAxis = Vector3.right;
        private float _moveSpeed;
        private float _pulseWeight;
        private float _pulseTime;

        public ProceduralPose(ProceduralPoseSettings settings)
        {
            Settings = settings ?? new ProceduralPoseSettings();
        }

        public ProceduralPoseSettings Settings { get; set; }

        /// <summary>Dinlenme konumuna eklenen kayma (ebeveyn uzayı).</summary>
        public Vector3 Offset { get; private set; }

        /// <summary>Dinlenme rotasyonunun önüne eklenen dönüş (ebeveyn uzayı).</summary>
        public Quaternion Rotation { get; private set; } = Quaternion.identity;

        /// <summary>Dinlenme ölçeğinin çarpanı (yürüme nabzı); dinlenmede 1.</summary>
        public float Scale { get; private set; } = 1f;

        public bool IsLunging => _lungeTime >= 0f;

        public bool IsShaking => _shakeTime >= 0f;

        public bool IsToppled => _toppleTime >= 0f;

        /// <summary>Yürüme nabzı oynuyor veya sönüyor.</summary>
        public bool IsPulsing => _pulseWeight > 0f || WantsPulse;

        /// <summary>Poz dinlenme pozundan farklı olabilir; <see cref="Tick"/> çağrılmalı.</summary>
        public bool IsActive => IsLunging || IsShaking || IsToppled || IsPulsing;

        private bool WantsPulse => Settings.WalkPulseScale > 0f && _moveSpeed > Settings.WalkPulseMinSpeed;

        /// <summary>Her şeyi durdurur, dinlenme pozuna döner (havuzdan çıkış, yeniden doğma).</summary>
        public void Reset()
        {
            _lungeTime = NotPlaying;
            _shakeTime = NotPlaying;
            _toppleTime = NotPlaying;
            _moveSpeed = 0f;
            _pulseWeight = 0f;
            _pulseTime = 0f;
            Offset = Vector3.zero;
            Rotation = Quaternion.identity;
            Scale = 1f;
        }

        /// <summary>Yatay yürüme hızı (m/sn). Nabız (<see cref="ProceduralPoseSettings.WalkPulseScale"/> &gt; 0 ise) bununla açılır/söner.</summary>
        public void SetMoveSpeed(float speed)
        {
            _moveSpeed = speed;
        }

        /// <summary>Öne (+Z) atılıp geri döner. Oynarken çağrılırsa baştan başlar.</summary>
        public void PlayLunge()
        {
            _lungeTime = 0f;
        }

        /// <summary>Yana (X) sönümlenen sarsılma. Oynarken çağrılırsa baştan başlar.</summary>
        public void PlayShake()
        {
            _shakeTime = 0f;
        }

        /// <summary>
        /// Verilen yatay yöne devrilir (ebeveyn uzayı). Yön sıfırsa öne. Zaten devrilmişse bir şey yapmaz.
        /// </summary>
        public void PlayTopple(Vector3 direction)
        {
            if (IsToppled)
            {
                return;
            }

            direction.y = 0f;
            if (direction.sqrMagnitude < MinDirectionSqr)
            {
                direction = Vector3.forward;
            }

            // Yukarı ekseni bu eksen etrafında döndürmek onu 'direction'a yatırır.
            _toppleAxis = Vector3.Cross(Vector3.up, direction.normalized).normalized;
            _toppleTime = 0f;
        }

        /// <summary>Zamanı ilerletir ve <see cref="Offset"/> / <see cref="Rotation"/>'ı yeniden hesaplar.</summary>
        public void Tick(float deltaTime)
        {
            Vector3 offset = Vector3.zero;
            Quaternion rotation = Quaternion.identity;

            if (IsLunging)
            {
                _lungeTime += deltaTime;
                float t = Progress(_lungeTime, Settings.LungeDuration);
                if (t >= 1f)
                {
                    _lungeTime = NotPlaying;
                }
                else
                {
                    offset += Vector3.forward * (Settings.LungeDistance * Mathf.Sin(Mathf.PI * t));
                }
            }

            if (IsShaking)
            {
                _shakeTime += deltaTime;
                float t = Progress(_shakeTime, Settings.ShakeDuration);
                if (t >= 1f)
                {
                    _shakeTime = NotPlaying;
                }
                else
                {
                    float wave = Mathf.Sin(2f * Mathf.PI * Settings.ShakeFrequency * _shakeTime);
                    offset += Vector3.right * (Settings.ShakeAmplitude * wave * (1f - t));
                }
            }

            if (IsToppled)
            {
                _toppleTime = Mathf.Min(_toppleTime + deltaTime, Settings.ToppleDuration);
                float t = Progress(_toppleTime, Settings.ToppleDuration);
                float fall = t * t; // Düşüş gibi hızlanır.
                rotation = Quaternion.AngleAxis(Settings.ToppleAngle * fall, _toppleAxis);
                offset += Vector3.up * (Settings.ToppleLift * fall);
            }

            Offset = offset;
            Rotation = rotation;
            Scale = TickPulse(deltaTime);
        }

        /// <summary>Nabız: ağırlık yürürken 1'e, durunca 0'a kayar; ölçek 1 → 1 + oran → 1 ((1 − cos) / 2 dalgası).</summary>
        private float TickPulse(float deltaTime)
        {
            float target = WantsPulse ? 1f : 0f;
            _pulseWeight = Mathf.MoveTowards(_pulseWeight, target, deltaTime / Mathf.Max(0.01f, Settings.WalkPulseBlendTime));
            if (_pulseWeight <= 0f)
            {
                _pulseTime = 0f;
                return 1f;
            }

            _pulseTime += deltaTime;
            float beat = 0.5f * (1f - Mathf.Cos(2f * Mathf.PI * Settings.WalkPulseFrequency * _pulseTime));
            return 1f + Settings.WalkPulseScale * _pulseWeight * beat;
        }

        private static float Progress(float elapsed, float duration)
        {
            return duration > 0f ? Mathf.Clamp01(elapsed / duration) : 1f;
        }
    }
}
