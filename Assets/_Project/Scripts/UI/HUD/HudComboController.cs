using LastHaven.Core;
using LastHaven.Economy;
using UnityEngine;
using UnityEngine.UIElements;

namespace LastHaven.UI
{
    /// <summary>
    /// HUD'un kombo bölümü (GDD §10, §16): sağ ortada büyük öldürme sayısı, çarpan (eşiğe ulaşınca) ve azalan pencere barı.
    /// Kombo yokken gizli. Çarpan yükselince grup <c>hud-combo--pulse</c> sınıfıyla kısa süre büyür: büyüme USS transition'ı
    /// bitince (<see cref="TransitionEndEvent"/>) sınıf kaldırılır ve aynı transition'la geri küçülür (süre USS'te).
    /// HUD'un UIDocument'ını paylaşır; veri sadece <see cref="ComboChangedEvent"/>'ten gelir.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HudComboController : UIDocumentController
    {
        /// <summary>Eşik atlanınca eklenen USS sınıfı (<c>HUD.uss</c>).</summary>
        public const string PulseClass = "hud-combo--pulse";

        private VisualElement _group;
        private Label _count;
        private Label _multiplier;
        private VisualElement _windowFill;
        private int _shownCount;
        private float _shownMultiplier;
        private float _shownRatio;

        protected override void BindElements()
        {
            _group = Require<VisualElement>(HudElementNames.ComboGroup);
            _count = Require<Label>(HudElementNames.ComboCount);
            _multiplier = Require<Label>(HudElementNames.ComboMultiplier);
            _windowFill = Require<VisualElement>(HudElementNames.ComboWindowFill);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (!IsBound)
            {
                return;
            }

            _group.RegisterCallback<TransitionEndEvent>(OnTransitionEnd);
            EventBus<ComboChangedEvent>.Subscribe(OnComboChanged);
            Hide();
        }

        private void OnDisable()
        {
            EventBus<ComboChangedEvent>.Unsubscribe(OnComboChanged);
            _group?.UnregisterCallback<TransitionEndEvent>(OnTransitionEnd);
        }

        private void OnComboChanged(ComboChangedEvent combo)
        {
            if (combo.Count <= 0)
            {
                Hide();
                return;
            }

            _group.SetShown(true);
            if (combo.Count != _shownCount)
            {
                _shownCount = combo.Count;
                _count.text = NumberText.Count(combo.Count);
            }

            if (!Mathf.Approximately(combo.Multiplier, _shownMultiplier))
            {
                bool tierUp = combo.Multiplier > _shownMultiplier;
                _shownMultiplier = combo.Multiplier;
                _multiplier.text = HudFormat.Multiplier(combo.Multiplier);
                _multiplier.SetShown(combo.Multiplier > ComboTracker.BaseMultiplier);
                if (tierUp && combo.Multiplier > ComboTracker.BaseMultiplier)
                {
                    _group.AddToClassList(PulseClass);
                }
            }

            float ratio = Mathf.Clamp01(combo.WindowRatio);
            if (!Mathf.Approximately(ratio, _shownRatio))
            {
                _shownRatio = ratio;
                _windowFill.style.width = Length.Percent(ratio * 100f);
            }
        }

        private void Hide()
        {
            _group.SetShown(false);
            _group.RemoveFromClassList(PulseClass);
            _shownCount = 0;
            _shownMultiplier = ComboTracker.BaseMultiplier;
            _shownRatio = -1f; // geçersiz: bir sonraki oran mutlaka yazılsın
            _multiplier.SetShown(false);
        }

        private void OnTransitionEnd(TransitionEndEvent transitionEnd)
        {
            // Büyüme bitti: sınıfı kaldır, aynı transition geri küçültür.
            if (_group.ClassListContains(PulseClass))
            {
                _group.RemoveFromClassList(PulseClass);
            }
        }
    }
}
