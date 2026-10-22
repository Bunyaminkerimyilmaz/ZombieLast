using LastHaven.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace LastHaven.UI
{
    /// <summary>
    /// MainMenu.unity → [Systems]/UI/MainMenu: ana menü (GDD §16). Oyna oyun sahnesini (<see cref="SceneNames.Game"/>) yükler, Ayarlar
    /// <see cref="SettingsPanelController"/>'ı açar (Esc / B geri döner), Çıkış oyundan çıkar (editörde Play Mode'u kapatır). Girdi baştan
    /// UI modunda; açılışta Oyna odaklıdır (klavye / gamepad ile gezilir, Enter / A basar).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MainMenuController : UIScreen
    {
        private const string LogPrefix = "[UI] ";

        [SerializeField, Tooltip("Ayarlar ekranı ([Systems]/UI/SettingsPanel). Sahne adımı bağlar.")]
        private SettingsPanelController _settingsPanel;

        private Button _playButton;
        private Button _settingsButton;
        private Button _quitButton;
        private IInputReader _input;
        private MenuInputScope _menuInput;
        private bool _leaving;

        protected override void BindElements()
        {
            _playButton = Require<Button>(MenuElementNames.PlayButton);
            _settingsButton = Require<Button>(MenuElementNames.SettingsButton);
            _quitButton = Require<Button>(MenuElementNames.QuitButton);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (!IsBound)
            {
                return;
            }

            _playButton.clicked += Play;
            _settingsButton.clicked += OpenSettings;
            _quitButton.clicked += Quit;
            if (_input != null)
            {
                _input.CancelPressed += OnCancelPressed;
            }
        }

        private void OnDisable()
        {
            if (_playButton != null)
            {
                _playButton.clicked -= Play;
                _settingsButton.clicked -= OpenSettings;
                _quitButton.clicked -= Quit;
            }

            if (_input != null)
            {
                _input.CancelPressed -= OnCancelPressed;
            }
        }

        private void Start()
        {
            if (!IsBound)
            {
                return;
            }

            if (ServiceLocator.TryGet(out _input))
            {
                _menuInput = new MenuInputScope(_input);
                _menuInput.Enter();
                _input.CancelPressed += OnCancelPressed;
            }

            ScreenFocus.FocusLater(ScreenElement, _playButton);
        }

        private void Play()
        {
            if (_leaving || IsSettingsOpen)
            {
                return;
            }

            _leaving = SceneLoader.TryLoad(SceneNames.Game, this);
        }

        private void OpenSettings()
        {
            if (_leaving || _settingsPanel == null || IsSettingsOpen)
            {
                return;
            }

            // Menü ayarların arkasında gizlenir: gamepad gezinmesi arkadaki butonlara atlamasın.
            ScreenFocus.Release(ScreenElement);
            ScreenElement.SetShown(false);
            _settingsPanel.Open(OnSettingsClosed);
        }

        private void OnSettingsClosed()
        {
            ScreenElement?.SetShown(IsShown);
            ScreenFocus.FocusLater(ScreenElement, _settingsButton);
        }

        private void OnCancelPressed()
        {
            if (IsSettingsOpen)
            {
                _settingsPanel.Close();
            }
        }

        private void Quit()
        {
            if (_leaving || IsSettingsOpen)
            {
                return;
            }

            Debug.Log($"{LogPrefix}Oyundan çıkılıyor.", this);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private bool IsSettingsOpen => _settingsPanel != null && _settingsPanel.IsShown;
    }
}
