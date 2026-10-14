using System.Collections.Generic;

namespace LastHaven.Core
{
    /// <summary>
    /// Proje tag'leri. Karşılaştırma her zaman <c>CompareTag(GameTags.X)</c> ile yapılır.
    /// </summary>
    public static class GameTags
    {
        // Unity'nin yerleşik tag'leri
        public const string Untagged = "Untagged";
        public const string Player = "Player";
        public const string MainCamera = "MainCamera";

        // Projeye özel tag'ler (Setup Project TagManager'a ekler)
        public const string Generator = "Generator";
        public const string Enemy = "Enemy";
        public const string Structure = "Structure";
        public const string Pickup = "Pickup";

        /// <summary>TagManager'a eklenmesi gereken, yerleşik olmayan tag'ler.</summary>
        public static readonly IReadOnlyList<string> Custom = new[]
        {
            Generator,
            Enemy,
            Structure,
            Pickup,
        };
    }
}
