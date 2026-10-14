namespace LastHaven.Core
{
    /// <summary>
    /// Sayısal stat olmayan, davranış değiştiren perk etkileri (GDD §13). İlgili modül <see cref="IRunModifiers.HasFlag"/> ile sorar.
    /// </summary>
    /// <remarks>Değerler SO'larda int olarak saklanır: sırayı değiştirme, yenisini sona ekle.</remarks>
    public enum PerkFlag
    {
        /// <summary>Seken Mermi: vuruştan sonra en yakın ikinci düşmana %50 hasar.</summary>
        Ricochet = 0,

        /// <summary>Vampir: her öldürme +2 can.</summary>
        Vampire = 1,

        /// <summary>Dikenli Duvar: barikatlar yakın dövüş hasarının %30'unu yansıtır.</summary>
        ThornWalls = 2,

        /// <summary>Şok Taret: taret vuruşları 1 sn %25 yavaşlatır.</summary>
        ShockTurret = 3,

        /// <summary>Zincirleme Son: oyuncunun öldürdüğü düşman %10 şansla patlar.</summary>
        ChainExplosion = 4,

        /// <summary>Avcı: yavaşlamış düşmanlara +%50 hasar.</summary>
        Hunter = 5,
    }
}
