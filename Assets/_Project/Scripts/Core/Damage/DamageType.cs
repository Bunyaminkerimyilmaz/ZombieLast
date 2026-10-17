namespace LastHaven.Core
{
    /// <summary>Hasarın kaynağının türü.</summary>
    public enum DamageType
    {
        Bullet,
        Melee,
        Explosion,
        Trap,

        /// <summary>Düşman mermisi (Tükürücü asidi). Yakın dövüş sayılmaz (barikat yansıtması gibi kurallara girmez).</summary>
        Projectile,
    }
}
