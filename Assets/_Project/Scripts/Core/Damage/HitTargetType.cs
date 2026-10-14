namespace LastHaven.Core
{
    /// <summary>Bir mermi isabetinin neye geldiği. Vuruş efekti ve sesi buna göre seçilir.</summary>
    /// <remarks>Değerler SO'larda int olarak saklanabilir: sırayı değiştirme, yenisini sona ekle.</remarks>
    public enum HitTargetType
    {
        /// <summary>Zemin, engel veya başka bir yüzey.</summary>
        Surface = 0,

        Enemy = 1,

        /// <summary>Barikat, taret, tuzak, jeneratör.</summary>
        Structure = 2,

        Player = 3,
    }

    /// <summary><see cref="HitTargetType"/> yardımcıları.</summary>
    public static class HitTargetTypes
    {
        /// <summary>Vurulan collider'ın layer'ından türü bulur. Proje layer'ı olmayan her şey yüzeydir.</summary>
        public static HitTargetType FromLayer(int layer)
        {
            return layer switch
            {
                GameLayers.Enemy => HitTargetType.Enemy,
                GameLayers.Structure => HitTargetType.Structure,
                GameLayers.Player => HitTargetType.Player,
                _ => HitTargetType.Surface,
            };
        }
    }
}
