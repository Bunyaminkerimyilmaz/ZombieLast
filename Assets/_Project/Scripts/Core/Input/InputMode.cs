namespace LastHaven.Core
{
    /// <summary>
    /// Girdinin hangi action haritasına gittiği. Aynı anda tek mod etkindir; diğer modların girdileri gelmez.
    /// </summary>
    public enum InputMode
    {
        /// <summary>Hareket, nişan, ateş, silah değiştirme, etkileşim.</summary>
        Gameplay,

        /// <summary>Hareket, nişan, yapı seçme/yerleştirme, döndürme, yükseltme, satış.</summary>
        BuildMode,

        /// <summary>Menü veya market açık: onay, iptal, market, duraklatma. Oyuncu hareket etmez.</summary>
        UI,
    }
}
