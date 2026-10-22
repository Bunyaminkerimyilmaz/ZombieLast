using System;
using LastHaven.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace LastHaven.UI
{
    /// <summary>
    /// [Systems]/UI/SettingsPanel: ayarlar ekranı (GDD §16; ana menüde ve duraklatma menüsünde aynı ekran). Açan menü
    /// <see cref="Open"/> ile açar ve geri dönüşü (Geri butonu veya açanın ilettiği Esc / B) <see cref="Close"/> ile alır. Her değişiklik
    /// hemen <see cref="ISettingsService.Apply"/>'a gider (ses, kamera, sarsıntı, tam ekran anında uygulanır); kapanınca ayarlar
    /// diske yazılır. Kendi girdi aboneliği yok: Esc'i açan menü yönlendirir (iki dinleyici aynı basışta iki ekranı kapatmasın).
    /// Elementler <see cref="SettingsPanelView"/>'da.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SettingsPanelController : UIScreen
    {
        private const string LogPrefix = "[UI] ";

        [SerializeField, Tooltip("Kaydırıcı adımı (Data/UI/MenuConfig.asset). Sahne adımı bağlar.")]
        private MenuConfig _config;

        private SettingsPanelView _view;
        private Button _backButton;
        private ISettingsService _settings;
        private Action _closed;

        protected override void BindElements()
        {
            var sliders = new[]
            {
                Require<SliderInt>(MenuElementNames.MasterVolumeSlider),
                Require<SliderInt>(MenuElementNames.MusicVolumeSlider),
                Require<SliderInt>(MenuElementNames.SfxVolumeSlider),
            };
            var values = new[]
            {
                Require<Label>(MenuElementNames.MasterVolumeValue),
                Require<Label>(MenuElementNames.MusicVolumeValue),
                Require<Label>(MenuElementNames.SfxVolumeValue),
            };
            _view = new SettingsPanelView(ScreenElement, sliders, values, Require<Button>(MenuElementNames.IsometricButton),
                Require<Button>(MenuElementNames.TopDownButton), Require<Toggle>(MenuElementNames.ShakeToggle),
                Require<Toggle>(MenuElementNames.FullscreenToggle));
            _backButton = Require<Button>(MenuElementNames.BackButton);
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

            _view.Bind(OnChanged, _config != null ? _config.VolumeStepPercent : 1);
            _backButton.clicked += Close;
        }

        private void OnDisable()
        {
            Close();
            _view?.Unbind();
            if (_backButton != null)
            {
                _backButton.clicked -= Close;
            }
        }

        private void Start()
        {
            if (!ServiceLocator.TryGet(out _settings))
            {
                Debug.LogWarning("SettingsPanelController: ISettingsService yok; ayarlar ekranı açılmayacak. Sahnede [Systems]/Settings var mı?", this);
            }
        }

        /// <summary>Ekranı geçerli ayarlarla açar; kapanınca <paramref name="closed"/> çağrılır. Servis yoksa açmadan hemen çağrılır.</summary>
        public void Open(Action closed)
        {
            if (IsShown)
            {
                return;
            }

            if (!IsBound || _settings == null)
            {
                closed?.Invoke();
                return;
            }

            _closed = closed;
            _view.Show(_settings.Current);
            SetShown(true);
            ScreenFocus.FocusLater(ScreenElement, _view.FirstControl);
            Debug.Log($"{LogPrefix}Ayarlar açıldı.", this);
        }

        /// <summary>Açıksa kapatır, ayarları diske yazar ve açana döner.</summary>
        public void Close()
        {
            if (!IsShown)
            {
                return;
            }

            ScreenFocus.Release(ScreenElement);
            SetShown(false);
            _settings?.Save();
            Action closed = _closed;
            _closed = null;
            Debug.Log($"{LogPrefix}Ayarlar kapandı.", this);
            closed?.Invoke();
        }

        private void OnChanged(GameSettings settings)
        {
            _settings?.Apply(settings);
        }
    }
}
