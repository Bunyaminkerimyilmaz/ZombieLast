using LastHaven.Core;

namespace LastHaven.Enemies
{
    /// <summary>
    /// Patlayanın fitili (saf): kurulu → (zincirle) gecikmeli → patladı. Bir doğuşta en fazla bir kez patlar; havuzdan her çıkışta
    /// <see cref="Arm"/> ile yeniden kurulur.
    /// </summary>
    public sealed class ExplosiveFuse
    {
        private readonly Countdown _delay = new Countdown();

        /// <summary>Patlayabilir: doğdu ve henüz patlamadı.</summary>
        public bool IsArmed { get; private set; }

        /// <summary>Zincirle tetiklendi; gecikme bitince patlayacak.</summary>
        public bool IsPrimed => _delay.IsRunning;

        public bool HasDetonated { get; private set; }

        /// <summary>Kalan zincir gecikmesi (sn); tetiklenmediyse 0.</summary>
        public float RemainingDelay => _delay.Remaining;

        /// <summary>Doğuş: kurulur, gecikme ve patlama kaydı silinir.</summary>
        public void Arm()
        {
            IsArmed = true;
            HasDetonated = false;
            _delay.Stop();
        }

        /// <summary>Havuza dönüş: patlamadan söner.</summary>
        public void Disarm()
        {
            IsArmed = false;
            _delay.Stop();
        }

        /// <summary>Zincir: <paramref name="delay"/> sn sonra patlayacak şekilde tetikler. Kurulu değilse veya zaten tetiklendiyse false.</summary>
        public bool TryPrime(float delay)
        {
            if (!IsArmed || IsPrimed)
            {
                return false;
            }

            _delay.Start(delay);
            return true;
        }

        /// <summary>Gecikmeyi ilerletir; bu çağrıda dolduysa true (patlatma zamanı).</summary>
        public bool Tick(float deltaTime)
        {
            return IsArmed && _delay.Tick(deltaTime);
        }

        /// <summary>Patlamayı kaydeder. İlk çağrıda true; kurulu değilse veya zaten patladıysa false.</summary>
        public bool TryDetonate()
        {
            if (!IsArmed)
            {
                return false;
            }

            IsArmed = false;
            HasDetonated = true;
            _delay.Stop();
            return true;
        }
    }
}
