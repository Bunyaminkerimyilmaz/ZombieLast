using LastHaven.Core;

namespace LastHaven.Building
{
    /// <summary>Dikenli Duvar (saf, GDD §13): barikata gelen hangi hasar ne kadar yansır.</summary>
    public static class ThornsRule
    {
        /// <summary>Sadece yakın dövüş yansır (patlama, asit mermisi, mermi yansımaz).</summary>
        public static bool Reflects(DamageType type)
        {
            return type == DamageType.Melee;
        }

        /// <summary>Yansıyan hasar: barikatın gerçekte aldığı hasar × oran (negatifler 0).</summary>
        public static float GetReflectedDamage(float damageTaken, float fraction)
        {
            return damageTaken > 0f && fraction > 0f ? damageTaken * fraction : 0f;
        }
    }
}
