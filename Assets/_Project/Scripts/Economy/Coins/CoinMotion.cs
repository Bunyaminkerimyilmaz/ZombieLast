using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Economy
{
    /// <summary>Jetonun yaşam evresi.</summary>
    public enum CoinState
    {
        /// <summary>Fırlatılmadı, toplandı ya da süresi doldu.</summary>
        Inactive = 0,

        /// <summary>Ölüm noktasından iniş noktasına yay çiziyor; mıknatısa kapılmaz.</summary>
        Scattering = 1,

        /// <summary>Yerde dönüyor; ömür işliyor, sonunda yanıp söner.</summary>
        Resting = 2,

        /// <summary>Mıknatısa kapıldı, hızlanarak oyuncuya uçuyor. Süresi dolmaz.</summary>
        Attracted = 3,
    }

    /// <summary><see cref="CoinMotion.Tick"/> sonucu.</summary>
    public enum CoinStep
    {
        None = 0,

        /// <summary>Oyuncuya değdi: değer cüzdana eklenmeli.</summary>
        Collected = 1,

        /// <summary>Ömrü doldu, toplanmadan kayboldu.</summary>
        Expired = 2,
    }

    /// <summary>
    /// Jetonun hareketi ve ömrü (GDD §10, saf): yay çizerek saçılır, yerde döner, ömrünün son saniyelerinde yanıp söner ve
    /// süresi dolunca biter. Yerdeyken oyuncunun mıknatıs yarıçapına (yatay) girerse hızlanarak toplama noktasına uçar;
    /// uçan jetonun süresi dolmaz. Sadece <see cref="Tick"/> ile ilerler; <c>Coin</c> sonucu Transform'a yazar.
    /// </summary>
    public sealed class CoinMotion
    {
        private const float FullTurn = 360f;
        private const float BlinkOnFraction = 0.5f;

        private readonly Countdown _lifetime = new Countdown();
        private CoinMotionSettings _settings;
        private Vector3 _start;
        private Vector3 _landing;
        private float _arcElapsed;
        private float _blinkDuration;

        public CoinState State { get; private set; }

        /// <summary>Jetonun merkezi (dünya).</summary>
        public Vector3 Position { get; private set; }

        /// <summary>Dikey eksendeki dönüş açısı (derece, 0–360).</summary>
        public float SpinAngle { get; private set; }

        /// <summary>Yanıp sönerken kapalı karelerde false; diğer durumlarda true.</summary>
        public bool IsVisible { get; private set; } = true;

        /// <summary>Yerde ve ömrünün son <c>blinkDuration</c> saniyesinde.</summary>
        public bool IsBlinking => State != CoinState.Attracted && _lifetime.IsRunning && _lifetime.Remaining <= _blinkDuration;

        /// <summary>Kalan ömür (sn). Uçarken ve bittikten sonra 0.</summary>
        public float RemainingLifetime => State == CoinState.Attracted ? 0f : _lifetime.Remaining;

        /// <summary>Uçuş hızı (m/sn); uçmuyorsa 0.</summary>
        public float Speed { get; private set; }

        /// <summary>
        /// Jetonu <paramref name="start"/>'tan <paramref name="landing"/>'e fırlatır. Ömür (sn) şimdi başlar; son
        /// <paramref name="blinkDuration"/> saniyesinde yanıp söner.
        /// </summary>
        public void Launch(
            CoinMotionSettings settings,
            Vector3 start,
            Vector3 landing,
            float lifetime,
            float blinkDuration,
            float spinAngle = 0f)
        {
            _settings = settings ?? new CoinMotionSettings();
            _start = start;
            _landing = landing;
            _arcElapsed = 0f;
            _blinkDuration = Mathf.Max(0f, blinkDuration);
            _lifetime.Start(lifetime);
            Position = start;
            SpinAngle = Mathf.Repeat(spinAngle, FullTurn);
            Speed = 0f;
            IsVisible = true;
            State = CoinState.Scattering;
        }

        /// <summary>Jetonu sonucu bildirmeden durdurur (havuza dönüş).</summary>
        public void Stop()
        {
            _lifetime.Stop();
            Speed = 0f;
            IsVisible = true;
            State = CoinState.Inactive;
        }

        /// <summary>Jetonu ilerletir. Toplandığı ya da süresi dolduğu karede sonucu bir kez döndürür.</summary>
        public CoinStep Tick(float deltaTime, in MagnetTarget magnet)
        {
            if (State == CoinState.Inactive)
            {
                return CoinStep.None;
            }

            SpinAngle = Mathf.Repeat(SpinAngle + _settings.SpinSpeed * deltaTime, FullTurn);
            if (State == CoinState.Attracted)
            {
                return Fly(deltaTime, magnet);
            }

            if (_lifetime.Tick(deltaTime))
            {
                Stop();
                return CoinStep.Expired;
            }

            if (State == CoinState.Scattering)
            {
                AdvanceArc(deltaTime);
            }
            else if (magnet.CanAttract && magnet.IsInRange(Position))
            {
                State = CoinState.Attracted;
                Speed = _settings.AttractStartSpeed;
                IsVisible = true;
                return Fly(deltaTime, magnet);
            }

            IsVisible = GetBlinkVisibility();
            return CoinStep.None;
        }

        private void AdvanceArc(float deltaTime)
        {
            _arcElapsed += deltaTime;
            float duration = _settings.ArcDuration;
            float t = duration > 0f ? Mathf.Clamp01(_arcElapsed / duration) : 1f;
            if (t >= 1f)
            {
                Position = _landing;
                State = CoinState.Resting;
                return;
            }

            // Düz çizgi + parabol: t = 0.5'te düz çizginin ArcHeight üstünde.
            float lift = 4f * _settings.ArcHeight * t * (1f - t);
            Position = Vector3.Lerp(_start, _landing, t) + Vector3.up * lift;
        }

        private CoinStep Fly(float deltaTime, in MagnetTarget magnet)
        {
            Speed = Mathf.Min(Speed + _settings.AttractAcceleration * deltaTime, _settings.AttractMaxSpeed);
            Vector3 toTarget = magnet.Position - Position;
            float distance = toTarget.magnitude;
            float step = Speed * deltaTime;
            if (distance - step <= _settings.CollectDistance)
            {
                Position = magnet.Position;
                Stop();
                return CoinStep.Collected;
            }

            Position += toTarget * (step / distance);
            return CoinStep.None;
        }

        private bool GetBlinkVisibility()
        {
            float frequency = _settings.BlinkFrequency;
            if (!IsBlinking || frequency <= 0f)
            {
                return true;
            }

            // Yanıp sönme açık başlar: her periyodun ilk yarısı görünür.
            float elapsed = _blinkDuration - _lifetime.Remaining;
            float cycle = elapsed * frequency;
            return cycle - Mathf.Floor(cycle) < BlinkOnFraction;
        }
    }
}
