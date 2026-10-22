using LastHaven.Core;

namespace LastHaven.DebugTools
{
    /// <summary>
    /// Debug ölümsüzlüğü: bir <see cref="Health"/>'in <see cref="Health.DamageFilter"/>'ını her hasarı engelleyen bu filtreyle değiştirir,
    /// kaldırılınca eski filtreyi (oyuncuda atılma / vuruş dokunulmazlığı) geri koyar. Engellenen hasar olay, flaş, kombo sıfırlaması
    /// üretmez (<see cref="Health"/> filtre kuralı). Sahibinin koduna dokunmaz; aynı anda tek bir Health'e uygulanır.
    /// </summary>
    public sealed class DamageImmunity : IDamageFilter
    {
        private Health _health;
        private IDamageFilter _previous;

        /// <summary>Bir Health'e uygulanmış mı (Health yok edildiyse false).</summary>
        public bool IsApplied => _health != null;

        public bool CanTakeDamage(in DamageInfo damage)
        {
            return false;
        }

        /// <summary><paramref name="health"/>'e uygular (başka birine uygulanmışsa önce oradan kaldırır). Null ise etkisiz.</summary>
        public void Apply(Health health)
        {
            if (health == null || ReferenceEquals(_health, health))
            {
                return;
            }

            Remove();
            _health = health;
            _previous = health.DamageFilter;
            health.DamageFilter = this;
        }

        /// <summary>Eski filtreyi geri koyar. Bu arada filtre başka biriyle değiştirildiyse ona dokunmaz.</summary>
        public void Remove()
        {
            if (_health != null && ReferenceEquals(_health.DamageFilter, this))
            {
                _health.DamageFilter = _previous;
            }

            _health = null;
            _previous = null;
        }
    }
}
