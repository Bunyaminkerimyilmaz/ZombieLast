namespace LastHaven.Building
{
    /// <summary><see cref="Structure.Upgrade"/> sonucu.</summary>
    public enum StructureUpgradeResult
    {
        /// <summary>Fiyat ödendi, yapı sonraki seviyeye geçti.</summary>
        Upgraded,

        /// <summary>Yapı zaten en üst seviyede.</summary>
        MaxLevel,

        /// <summary>Para yetmiyor; hiçbir şey değişmedi.</summary>
        NotEnoughMoney,

        /// <summary>Yapı ızgarada değil (havuzda).</summary>
        NotPlaced,
    }
}
