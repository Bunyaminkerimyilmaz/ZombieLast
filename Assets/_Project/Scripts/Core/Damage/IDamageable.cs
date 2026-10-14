namespace LastHaven.Core
{
    /// <summary>Hasar alabilen her şey (oyuncu, düşman, yapı, jeneratör).</summary>
    public interface IDamageable
    {
        bool IsAlive { get; }

        void TakeDamage(DamageInfo damage);
    }
}
