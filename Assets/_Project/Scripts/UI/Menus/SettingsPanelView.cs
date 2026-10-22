using System;
using LastHaven.Core;
using UnityEngine.UIElements;

namespace LastHaven.UI
{
    /// <summary>
    /// Ayarlar ekranının UI Toolkit tarafı (<c>UI/Menus/Settings.uxml</c>): üç ses kaydırıcısı + yüzde yazısı, kamera preset'i için iki
    /// buton, sarsıntı ve tam ekran onay kutuları. Her değişiklikte yeni <see cref="GameSettings"/>'i bildirir (kaydırıcı sürüklenirken
    /// de: ses hemen duyulur). Odaktaki kaydırıcıda sol / sağ (ok tuşları, D-pad) değeri adım adım değiştirir; odak gezinmesi o yönde
    /// yutulur. Odaktaki satır vurgulanır (<see cref="MenuElementNames.RowFocusedClass"/>). UIDocument her açılışta ağacı yeniden
    /// kurduğu için denetleyici her <c>OnEnable</c>'da yenisini oluşturur.
    /// </summary>
    public sealed class SettingsPanelView
    {
        private readonly VisualElement _screen;
        private readonly SliderInt[] _sliders;
        private readonly Label[] _values;
        private readonly Button _isometric;
        private readonly Button _topDown;
        private readonly Toggle _shake;
        private readonly Toggle _fullscreen;

        private GameSettings _current;
        private Action<GameSettings> _changed;
        private int _step = 1;

        /// <param name="sliders">Genel, müzik, efekt (bu sırayla); <paramref name="values"/> aynı sırada.</param>
        public SettingsPanelView(VisualElement screen, SliderInt[] sliders, Label[] values, Button isometric, Button topDown, Toggle shake,
            Toggle fullscreen)
        {
            _screen = screen;
            _sliders = sliders;
            _values = values;
            _isometric = isometric;
            _topDown = topDown;
            _shake = shake;
            _fullscreen = fullscreen;
        }

        /// <summary>Açılınca odak alan ilk kontrol (genel ses).</summary>
        public Focusable FirstControl => _sliders[0];

        /// <summary>Kontrollere abone olur. <paramref name="step"/>: sol / sağ başına yüzde.</summary>
        public void Bind(Action<GameSettings> changed, int step)
        {
            _changed = changed;
            _step = step;
            for (int i = 0; i < _sliders.Length; i++)
            {
                int index = i;
                _sliders[i].lowValue = VolumePercent.Min;
                _sliders[i].highValue = VolumePercent.Max;
                _sliders[i].RegisterValueChangedCallback(evt => OnVolumeChanged(index, evt.newValue));
            }

            _isometric.clicked += () => SetPreset(CameraPreset.Isometric);
            _topDown.clicked += () => SetPreset(CameraPreset.TopDown);
            _shake.RegisterValueChangedCallback(evt => Notify(_current.WithScreenShake(evt.newValue)));
            _fullscreen.RegisterValueChangedCallback(evt => Notify(_current.WithFullscreen(evt.newValue)));
            _screen.RegisterCallback<NavigationMoveEvent>(OnNavigationMove, TrickleDown.TrickleDown);
            _screen.Query(className: MenuElementNames.RowClass).ForEach(row =>
            {
                row.RegisterCallback<FocusInEvent>(_ => row.AddToClassList(MenuElementNames.RowFocusedClass));
                row.RegisterCallback<FocusOutEvent>(_ => row.RemoveFromClassList(MenuElementNames.RowFocusedClass));
            });
        }

        public void Unbind()
        {
            _changed = null;
            _screen.UnregisterCallback<NavigationMoveEvent>(OnNavigationMove, TrickleDown.TrickleDown);
        }

        /// <summary>Ayarları kontrollere yazar (bildirim göndermeden).</summary>
        public void Show(GameSettings settings)
        {
            _current = settings;
            SetVolume(0, settings.MasterVolume);
            SetVolume(1, settings.MusicVolume);
            SetVolume(2, settings.SfxVolume);
            ShowPreset(settings.CameraPreset);
            _shake.SetValueWithoutNotify(settings.ScreenShake);
            _fullscreen.SetValueWithoutNotify(settings.Fullscreen);
        }

        private void SetVolume(int index, float volume)
        {
            int percent = VolumePercent.FromVolume(volume);
            _sliders[index].SetValueWithoutNotify(percent);
            _values[index].text = HudFormat.Count(percent);
        }

        private void OnVolumeChanged(int index, int percent)
        {
            _values[index].text = HudFormat.Count(percent);
            float volume = VolumePercent.ToVolume(percent);
            Notify(index == 0 ? _current.WithMasterVolume(volume)
                : index == 1 ? _current.WithMusicVolume(volume)
                : _current.WithSfxVolume(volume));
        }

        private void SetPreset(CameraPreset preset)
        {
            ShowPreset(preset);
            Notify(_current.WithCameraPreset(preset));
        }

        private void ShowPreset(CameraPreset preset)
        {
            _isometric.EnableInClassList(MenuElementNames.ChoiceSelectedClass, preset == CameraPreset.Isometric);
            _topDown.EnableInClassList(MenuElementNames.ChoiceSelectedClass, preset == CameraPreset.TopDown);
        }

        private void Notify(GameSettings settings)
        {
            _current = settings;
            _changed?.Invoke(settings);
        }

        private void OnNavigationMove(NavigationMoveEvent evt)
        {
            int direction = evt.direction == NavigationMoveEvent.Direction.Right ? 1
                : evt.direction == NavigationMoveEvent.Direction.Left ? -1
                : 0;
            SliderInt slider = direction != 0 ? FocusedSlider() : null;
            if (slider == null)
            {
                return;
            }

            slider.value = VolumePercent.Step(slider.value, direction, _step);
            evt.StopPropagation();
            _screen.panel?.focusController?.IgnoreEvent(evt);
        }

        private SliderInt FocusedSlider()
        {
            if (!(_screen.panel?.focusController?.focusedElement is VisualElement focused))
            {
                return null;
            }

            for (int i = 0; i < _sliders.Length; i++)
            {
                if (_sliders[i] == focused || _sliders[i].Contains(focused))
                {
                    return _sliders[i];
                }
            }

            return null;
        }
    }
}
