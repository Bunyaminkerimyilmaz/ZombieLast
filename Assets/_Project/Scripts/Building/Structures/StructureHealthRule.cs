using System;
using LastHaven.Core;

namespace LastHaven.Building
{
    /// <summary>
    /// Yapının maks. canı (saf, GDD §11 + §13): seviyenin canı; barikatta <see cref="IRunModifiers"/> BarricadeMaxHealth ile (Sağlam
    /// Duvar +%30). Diğer yapılar run çarpanı almaz.
    /// </summary>
    public static class StructureHealthRule
    {
        // Health.SetMax pozitif ister; negatif modifier'lar canı buraya kırpar.
        private const float MinMaxHealth = 1f;

        /// <summary>Bu türün canı run çarpanından etkilenir mi (sadece barikat).</summary>
        public static bool UsesModifiers(TargetType type)
        {
            return type == TargetType.Barricade;
        }

        /// <summary>Etkili maks. can; çarpan yoksa veya tür etkilenmiyorsa seviyenin canı.</summary>
        public static float GetMaxHealth(TargetType type, float levelMaxHealth, IRunModifiers modifiers)
        {
            if (modifiers == null || !UsesModifiers(type))
            {
                return levelMaxHealth;
            }

            return Math.Max(MinMaxHealth, modifiers.Get(StatType.BarricadeMaxHealth, levelMaxHealth));
        }
    }
}
