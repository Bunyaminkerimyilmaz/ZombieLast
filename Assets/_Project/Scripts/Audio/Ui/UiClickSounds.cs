using LastHaven.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace LastHaven.Audio
{
    /// <summary>
    /// Arayüz tıklama sesi ([Systems]/Audio). Tüm ekranlar aynı PanelSettings'i (tek panel) paylaştığı için panelin kökünü dinler:
    /// fare tıklaması (<see cref="ClickEvent"/>) ve Enter / gamepad A (<see cref="NavigationSubmitEvent"/>) etkin bir butona
    /// geldiyse <see cref="AudioEventMap.UiClick"/>'i <see cref="IAudioService.PlayUi"/> ile çalar. UI modülü bu sınıfı bilmez.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class UiClickSounds : MonoBehaviour
    {
        [SerializeField, Tooltip("Ortak paneldeki herhangi bir UIDocument (sahne adımı HUD'u bağlar); panelin kökü buradan bulunur.")]
        private UIDocument _document;

        [SerializeField, Tooltip("Tıklama sesi buradan (UiClick).")]
        private AudioEventMap _map;

        private VisualElement _panelRoot;
        private IAudioService _audio;

        private void OnEnable()
        {
            Attach();
        }

        private void Start()
        {
            // UIDocument paneli henüz kurmadıysa (çalışma sırası) bir kez daha dene.
            if (_panelRoot == null)
            {
                Attach();
                if (_panelRoot == null)
                {
                    Debug.LogWarning("UiClickSounds: UIDocument veya paneli yok; arayüz tıklama sesi çalmayacak.", this);
                }
            }
        }

        private void OnDisable()
        {
            if (_panelRoot == null)
            {
                return;
            }

            _panelRoot.UnregisterCallback<ClickEvent>(OnClick, TrickleDown.TrickleDown);
            _panelRoot.UnregisterCallback<NavigationSubmitEvent>(OnSubmit, TrickleDown.TrickleDown);
            _panelRoot = null;
        }

        private void Attach()
        {
            if (_panelRoot != null || _document == null || _map == null)
            {
                return;
            }

            IPanel panel = _document.rootVisualElement?.panel;
            if (panel == null)
            {
                return;
            }

            // TrickleDown: buton olayı tüketse (StopPropagation) de ses kaçmaz.
            _panelRoot = panel.visualTree;
            _panelRoot.RegisterCallback<ClickEvent>(OnClick, TrickleDown.TrickleDown);
            _panelRoot.RegisterCallback<NavigationSubmitEvent>(OnSubmit, TrickleDown.TrickleDown);
        }

        private void OnClick(ClickEvent evt)
        {
            PlayIfButton(evt.target);
        }

        private void OnSubmit(NavigationSubmitEvent evt)
        {
            PlayIfButton(evt.target);
        }

        private void PlayIfButton(IEventHandler target)
        {
            if (_map.UiClick == null || !UiClickFilter.IsClickable(target))
            {
                return;
            }

            if (_audio != null || ServiceLocator.TryGet(out _audio))
            {
                _audio.PlayUi(_map.UiClick);
            }
        }
    }
}
