namespace LastHaven.Core
{
    /// <summary>Düşmanların seçebileceği hedef türleri.</summary>
    public enum TargetType
    {
        Player,
        Generator,
        Barricade,
        Turret,
        Trap,
    }

    /// <summary><see cref="TargetType"/> yardımcıları.</summary>
    public static class TargetTypes
    {
        /// <summary>Oyuncu dışındaki her hedef yapıdır: jeneratör, barikat, taret, tuzak (yapı hasarı çarpanı bunlara uygulanır).</summary>
        public static bool IsStructure(this TargetType type)
        {
            return type != TargetType.Player;
        }
    }
}
