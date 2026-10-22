using LastHaven.Core;

namespace LastHaven.UI
{
    /// <summary>
    /// Bir <see cref="Health"/>'i bir <see cref="BarView"/>'a bağlar: hasar ve iyileşmede (Health'in yerel C# olayları) barı günceller.
    /// Global <see cref="DamagedEvent"/>'i süzmez; yüzlerce düşman vuruşu bu bara uğramaz. <c>SetMax</c> olay göndermediği için
    /// (ör. yeniden doğuş) sahibi <see cref="Refresh"/> çağırır.
    /// </summary>
    public sealed class HealthBarBinding
    {
        private Health _health;
        private BarView _bar;

        /// <summary>Gözlenen can; yoksa null.</summary>
        public Health Health => _health;

        /// <summary>Yerel olaylara abone olur ve hemen günceller. Önceki bağlantı varsa çözülür.</summary>
        public void Attach(Health health, BarView bar)
        {
            Detach();
            _health = health;
            _bar = bar;
            if (_health == null)
            {
                return;
            }

            _health.Damaged += OnDamaged;
            _health.Healed += OnHealed;
            Refresh();
        }

        public void Detach()
        {
            if (_health != null)
            {
                _health.Damaged -= OnDamaged;
                _health.Healed -= OnHealed;
            }

            _health = null;
            _bar = null;
        }

        /// <summary>
        /// Barı can bileşeninin o anki değerleriyle yeniler. Maks. canın tüm "x/maks" metinleri burada hazırlanır
        /// (<see cref="FractionText.Prefill"/>): vuruşlarda string ayrılmaz.
        /// </summary>
        public void Refresh()
        {
            if (_health != null)
            {
                FractionText.Prefill(HudFormat.DisplayHealth(_health.Max));
                _bar.Set(_health.Current, _health.Max);
            }
        }

        private void OnDamaged(DamagedEvent damaged)
        {
            _bar.Set(damaged.CurrentHealth, damaged.MaxHealth);
        }

        private void OnHealed(float amount)
        {
            Refresh();
        }
    }
}
