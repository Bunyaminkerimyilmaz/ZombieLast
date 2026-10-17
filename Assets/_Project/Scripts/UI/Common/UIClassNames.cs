namespace LastHaven.UI
{
    /// <summary>Kodun aradığı veya eklediği/kaldırdığı ortak USS sınıfları (<c>UI/Themes/Theme.uss</c>).</summary>
    public static class UIClassNames
    {
        /// <summary>Her ekranın UXML kök elementi (tema değişkenleri burada). <see cref="UIScreen"/> görünürlüğü bunda değiştirir.</summary>
        public const string Screen = "lh-screen";

        /// <summary>Elementi gizler (<c>display: none</c>).</summary>
        public const string Hidden = "hidden";

        /// <summary>
        /// Ekranın kökünde: son kullanılan cihaz gamepad. Tuş ipuçlarından <c>.key--gamepad</c> olanlar görünür, <c>.key--keyboard</c>
        /// olanlar gizlenir (<c>Theme.uss</c>).
        /// </summary>
        public const string Gamepad = "lh-gamepad";
    }
}
