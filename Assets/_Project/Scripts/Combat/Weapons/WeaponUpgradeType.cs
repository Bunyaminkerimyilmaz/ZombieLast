namespace LastHaven.Combat
{
    /// <summary>Silah yükseltme türleri (GDD §6). Seviyeler her silah için ayrıdır.</summary>
    /// <remarks>Değerler SO'larda int olarak saklanabilir: sırayı değiştirme, yenisini sona ekle.</remarks>
    public enum WeaponUpgradeType
    {
        Damage = 0,
        FireRate = 1,
        MagSize = 2,
    }
}
