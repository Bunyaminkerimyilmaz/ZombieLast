using LastHaven.Core;

namespace LastHaven.Enemies
{
    /// <summary>Yakın dövüş hasarı hesabı. Saf; testlenebilir.</summary>
    public static class MeleeDamage
    {
        /// <summary>Hedef yapıysa (<see cref="TargetTypes.IsStructure"/>) <paramref name="structureMultiplier"/> uygulanmış hasar.</summary>
        public static float Calculate(float damage, TargetType targetType, float structureMultiplier)
        {
            return targetType.IsStructure() ? damage * structureMultiplier : damage;
        }
    }
}
