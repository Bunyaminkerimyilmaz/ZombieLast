namespace LastHaven.Core
{
    /// <summary>
    /// <see cref="Health"/>'e gelen hasarı engelleyebilen kaynak (ör. atılma sırasında dokunulmazlık).
    /// Sahibi <see cref="Health.DamageFilter"/>'a atar.
    /// </summary>
    public interface IDamageFilter
    {
        /// <summary>false dönerse hasar uygulanmaz ve olay gönderilmez.</summary>
        bool CanTakeDamage(in DamageInfo damage);
    }
}
