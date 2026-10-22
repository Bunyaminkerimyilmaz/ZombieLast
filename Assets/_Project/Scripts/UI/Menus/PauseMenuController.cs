using LastHaven.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace LastHaven.UI
{
    /// <summary>
    /// [Systems]/UI/PauseMenu: duraklatma menüsü (GDD §4 Esc / Start, §16). Oyun veya inşa modunda Esc / Start ile açılır
    /// (<see cref="PauseMenuRules"/>): oyun durur (<see cref="IPauseService"/>, kaynak bu bileşen), girdi UI'a geçer
    /// (<see cref="MenuInputScope"/>; kapanınca önceki moda — inşa modunda açıldıysa inşa moduna — döner). Devam, Ayarlar
    /// (<see cref="SettingsPanelController"/>) ve Ana Menü. Esc / B ayarlar açıksa ayarlardan, değilse menüden çıkar; Start menüyü
    /// kapatır. Run biterse kendiliğinden kapanır. Devam / Esc / Start ile kapanış bir sonraki Update'te olur: aynı tuş basışının diğer
    /// dinleyicileri (inşa modunun iptali ve onayı) girdiyi hâlâ UI modunda görsün, seçimi silmesin veya yapı yerleştirmesin.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PauseMenuController : UIScreen
    {
        private const string LogPrefix = "[UI] ";

        [SerializeField, Tooltip("Ayarlar ekranı ([Systems]/UI/SettingsPanel). Sahne adımı bağlar.")]
        private SettingsPanelController _settingsPanel;

        private Button _resumeButton;
        private Button _settingsButton;
        private Button _mainMenuButton;

        private IInputReader _input;
        private IPauseService _pause;
        private IGameStateService _gameState;
        private ISettingsService _settings;
        private MenuInputScope _menuInput;
        private bool _started;
        private bool _leaving;
        private bool _closePending;

        protected override void BindElements()
        {
            _resumeButton = Require<Button>(MenuElementNames.ResumeButton);
            _settingsButton = Require<Button>(MenuElementNames.SettingsButton);
            _mainMenuButton = Require<Button>(MenuElementNames.MainMenuButton);
        }

        private void Awake()
        {
            SetShown(false);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (!IsBound)
            {
                return;
            }

            _resumeButton.clicked += RequestClose;
            _settingsButton.clicked += OpenSettings;
            _mainMenuButton.clicked += GoToMainMenu;
            EventBus<PhaseChangedEvent>.Subscribe(OnPhaseChanged);
            if (_started)
            {
                SubscribeInput();
            }
        }

        private void OnDisable()
        {
            Close();
            if (_resumeButton != null)
            {
                _resumeButton.clicked -= RequestClose;
                _settingsButton.clicked -= OpenSettings;
                _mainMenuButton.clicked -= GoToMainMenu;
            }

            EventBus<PhaseChangedEvent>.Unsubscribe(OnPhaseChanged);
            if (_input != null)
            {
                _input.PausePressed -= OnPausePressed;
                _input.CancelPressed -= OnCancelPressed;
            }
        }

        private void Start()
        {
            if (!IsBound)
            {
                return;
            }

            if (!ServiceLocator.TryGet(out _input) || !ServiceLocator.TryGet(out _pause))
            {
                Debug.LogWarning("PauseMenuController: IInputReader veya IPauseService yok; duraklatma menüsü açılmayacak. " +
                                 "Tools/Last Haven/Build Game Scene çalıştır.", this);
                return;
            }

            ServiceLocator.TryGet(out _gameState);
            ServiceLocator.TryGet(out _settings);
            _menuInput = new MenuInputScope(_input);
            _started = true;
            SubscribeInput();
        }

        private void SubscribeInput()
        {
            _input.PausePressed += OnPausePressed;
            _input.CancelPressed += OnCancelPressed;
        }

        private void Update()
        {
            if (_closePending)
            {
                _closePending = false;
                Close();
            }
        }

        private void OnPausePressed()
        {
            if (IsShown)
            {
                RequestClose();
            }
            else if (PauseMenuRules.CanOpen(_input.Mode, _pause, _gameState))
            {
                Open();
            }
        }

        private void OnCancelPressed()
        {
            if (!IsShown)
            {
                return;
            }

            if (_settingsPanel != null && _settingsPanel.IsShown)
            {
                _settingsPanel.Close();
            }
            else
            {
                RequestClose();
            }
        }

        /// <summary>Kapanışı bir sonraki Update'e bırakır (bkz. sınıf açıklaması).</summary>
        private void RequestClose()
        {
            _closePending = IsShown;
        }

        private void Open()
        {
            _closePending = false;
            _pause.Pause(this);
            _menuInput.Enter();
            SetShown(true);
            ScreenFocus.FocusLater(ScreenElement, _resumeButton);
            Debug.Log($"{LogPrefix}Duraklatma menüsü açıldı.", this);
        }

        /// <summary>Menüyü (ve açıksa ayarları) kapatır, girdiyi önceki moda alır ve oyunu devam ettirir.</summary>
        private void Close()
        {
            _closePending = false;
            if (!IsShown)
            {
                return;
            }

            SetShown(false);
            if (_settingsPanel != null && _settingsPanel.IsShown)
            {
                _settingsPanel.Close();
            }

            ScreenFocus.Release(ScreenElement);
            _menuInput?.Exit();
            if (_pause is Object pauseObject ? pauseObject != null : _pause != null)
            {
                _pause.Resume(this);
            }

            Debug.Log($"{LogPrefix}Duraklatma menüsü kapandı.", this);
        }

        private void OpenSettings()
        {
            // Menü ayarların arkasında gizlenir (IsShown değişmez): gamepad gezinmesi arkadaki butonlara atlamasın.
            if (IsShown && _settingsPanel != null)
            {
                ScreenFocus.Release(ScreenElement);
                ScreenElement.SetShown(false);
                _settingsPanel.Open(OnSettingsClosed);
            }
        }

        private void OnSettingsClosed()
        {
            ScreenElement?.SetShown(IsShown);
            ScreenFocus.FocusLater(ScreenElement, _settingsButton);
        }

        // Oyun duraklıyken sahne değişir; oyun sahnesi kapanırken PauseService zaman ölçeğini geri yazar.
        private void GoToMainMenu()
        {
            if (!IsShown || _leaving)
            {
                return;
            }

            _settings?.Save();
            _leaving = SceneLoader.TryLoad(SceneNames.MainMenu, this);
        }

        private void OnPhaseChanged(PhaseChangedEvent changed)
        {
            if (IsShown && !LevelUpPanelRules.IsRunActive(changed.Phase))
            {
                Close();
            }
        }
    }
}
