using System.Collections.Generic;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// Player Settings ve build çıktısının tek kaynağı (15.3). Sadece editör araçları okur: Setup Project ve
    /// Tools/Last Haven/Build/Apply Player Settings bunu <see cref="PlayerSettingsSetup"/> ile Player Settings'e yazar, build menüleri
    /// (<see cref="PlayerBuilder"/>) önce uygular sonra derler. Asset: <c>Data/Build/PlayerBuildConfig.asset</c>; değiştirip menüyü çalıştır.
    /// </summary>
    public sealed class PlayerBuildConfig : ScriptableObject
    {
        [Header("Kimlik")]
        [SerializeField, Tooltip("Ürün adı: pencere başlığı, kayıt klasörü (AppData/LocalLow/<şirket>/<ürün>) ve PlayerPrefs anahtarı. " +
                                 "Dosya adında geçersiz karakter (: / \\ ? * \" < > |) olmamalı; bu yüzden 'Last Haven: Day & Night' değil.")]
        private string _productName = "Last Haven";

        [SerializeField, Tooltip("Şirket adı: kayıt klasörü ve PlayerPrefs anahtarı. Ürün adıyla aynı karakter kuralı.")]
        private string _companyName = "Last Haven Team";

        [SerializeField, Tooltip("Sürüm (büyük.küçük.yama, ör. 0.1.0): Application.version ve macOS paket sürümü.")]
        private string _version = "0.1.0";

        [SerializeField, Tooltip("Uygulama kimliği (ters alan adı; macOS paketi için şart, ör. com.lasthaven.game).")]
        private string _applicationIdentifier = "com.lasthaven.game";

        [Header("Pencere")]
        [SerializeField, Tooltip("Varsayılan pencere modu. FullScreenWindow: kenarlıksız tam ekran (masaüstü çözünürlüğü). " +
                                 "Ayarlardaki 'Tam ekran' kapatılınca pencereye geçer.")]
        private FullScreenMode _fullScreenMode = FullScreenMode.FullScreenWindow;

        [SerializeField, Min(640), Tooltip("Pencere modunda varsayılan genişlik (px).")]
        private int _windowedWidth = 1600;

        [SerializeField, Min(360), Tooltip("Pencere modunda varsayılan yükseklik (px).")]
        private int _windowedHeight = 900;

        [SerializeField, Tooltip("Pencere modunda kenarından boyutlandırılabilsin.")]
        private bool _resizableWindow = true;

        [Header("Grafik")]
        [SerializeField, Tooltip("Build'de tutulacak sis modları (Graphics Settings → Fog Modes: Custom). Gündüz/gece ışığı ve gece " +
                                 "olayları ExponentialSquared kullanır (LightingValues). Linear sise geçilirse buraya ekle.")]
        private FogMode[] _keptFogModes = { FogMode.ExponentialSquared };

        [Header("Çıktı")]
        [SerializeField, Tooltip("Proje köküne göre build klasörü; platformlar alt klasörlere çıkar (Builds/Windows, Builds/macOS). " +
                                 ".gitignore'da.")]
        private string _outputFolder = "Builds";

        [SerializeField, Tooltip("Çalıştırılabilir dosyanın adı (uzantısız): LastHaven.exe, LastHaven.app.")]
        private string _executableName = "LastHaven";

        public string ProductName => _productName;

        public string CompanyName => _companyName;

        public string Version => _version;

        public string ApplicationIdentifier => _applicationIdentifier;

        public FullScreenMode FullScreenMode => _fullScreenMode;

        public int WindowedWidth => _windowedWidth;

        public int WindowedHeight => _windowedHeight;

        public bool ResizableWindow => _resizableWindow;

        public IReadOnlyList<FogMode> KeptFogModes => _keptFogModes;

        public string OutputFolder => _outputFolder;

        public string ExecutableName => _executableName;
    }
}
