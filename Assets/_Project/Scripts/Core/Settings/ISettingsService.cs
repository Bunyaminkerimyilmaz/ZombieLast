namespace LastHaven.Core
{
    /// <summary>
    /// Oyuncu ayarları (GameFlow'daki <c>SettingsService</c>, [Systems]/Settings, Awake'te kaydolur ve kayıtlı ayarları yükler; ana menüde
    /// ve oyun sahnesinde ayrı örnek, kalıcılık PlayerPrefs). Değişiklik <see cref="SettingsChangedEvent"/> ile duyurulur; uygulayanlar
    /// (ses düzeyleri, kamera, tam ekran) başlangıç değerini Start'ta <see cref="Current"/>'tan okur.
    /// </summary>
    public interface ISettingsService
    {
        /// <summary>Geçerli ayarlar (kayıtlı değer yoksa varsayılanlar).</summary>
        GameSettings Current { get; }

        /// <summary>
        /// Ayarları değiştirir: farklıysa hemen uygulanır (<see cref="SettingsChangedEvent"/>) ve kayda yazılır; diske
        /// <see cref="Save"/> ile (ayarlar ekranı kapanınca) ya da en geç oyundan çıkarken yazılır. Aynı değerse hiçbir şey olmaz.
        /// </summary>
        void Apply(GameSettings settings);

        /// <summary>Bekleyen değişiklikleri diske yazar (değişiklik yoksa etkisiz).</summary>
        void Save();
    }
}
