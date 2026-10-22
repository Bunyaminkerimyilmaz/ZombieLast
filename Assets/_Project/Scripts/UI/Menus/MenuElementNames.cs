namespace LastHaven.UI
{
    /// <summary>
    /// Ana menü, duraklatma menüsü ve ayarlar ekranının UXML element adları (<c>UI/Menus/</c>). EditMode testi kontrol eder.
    /// </summary>
    public static class MenuElementNames
    {
        // MainMenu.uxml
        public const string PlayButton = "play-button";
        public const string SettingsButton = "settings-button";
        public const string QuitButton = "quit-button";

        // PauseMenu.uxml (Ayarlar butonu da settings-button)
        public const string ResumeButton = "resume-button";
        public const string MainMenuButton = "main-menu-button";

        // Settings.uxml
        public const string MasterVolumeSlider = "master-volume-slider";
        public const string MusicVolumeSlider = "music-volume-slider";
        public const string SfxVolumeSlider = "sfx-volume-slider";
        public const string MasterVolumeValue = "master-volume-value";
        public const string MusicVolumeValue = "music-volume-value";
        public const string SfxVolumeValue = "sfx-volume-value";
        public const string IsometricButton = "camera-isometric-button";
        public const string TopDownButton = "camera-top-down-button";
        public const string ShakeToggle = "shake-toggle";
        public const string FullscreenToggle = "fullscreen-toggle";
        public const string BackButton = "settings-back-button";

        /// <summary>Ayarlar ekranındaki her satırın sınıfı; odaktaki satır <see cref="RowFocusedClass"/> alır.</summary>
        public const string RowClass = "settings-row";

        public const string RowFocusedClass = "settings-row--focused";

        /// <summary>Seçili kamera preset'i butonu.</summary>
        public const string ChoiceSelectedClass = "settings-choice--selected";
    }
}
