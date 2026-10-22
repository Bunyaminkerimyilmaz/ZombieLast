using LastHaven.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace LastHaven.UI
{
    /// <summary>
    /// HUD'un XP bölümü (GDD §13, §16): alt ortada seviye numarası, bu seviyenin ince XP barı ve "XP/eşik". Başlangıç değeri
    /// Start'ta <see cref="IExperienceService"/>'ten, sonrası <see cref="XpChangedEvent"/> ile gelir; Update yok. Seviye atlayınca
    /// (<see cref="LevelUpEvent"/>) numara <c>hud-xp__level--pulse</c> sınıfıyla kısa süre büyür: transition bitince
    /// (<see cref="TransitionEndEvent"/>) sınıf kaldırılır ve aynı transition'la geri küçülür (süre USS'te).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HudXpController : UIDocumentController
    {
        /// <summary>Seviye atlayınca seviye numarasına eklenen USS sınıfı (<c>HUD.uss</c>).</summary>
        public const string PulseClass = "hud-xp__level--pulse";

        private Label _level;
        private VisualElement _fill;
        private Label _value;
        private bool _started;

        protected override void BindElements()
        {
            Require<VisualElement>(HudElementNames.XpGroup);
            _level = Require<Label>(HudElementNames.XpLevel);
            _fill = Require<VisualElement>(HudElementNames.XpFill);
            _value = Require<Label>(HudElementNames.XpValue);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (!IsBound)
            {
                return;
            }

            _level.RegisterCallback<TransitionEndEvent>(OnTransitionEnd);
            EventBus<XpChangedEvent>.Subscribe(OnXpChanged);
            EventBus<LevelUpEvent>.Subscribe(OnLevelUp);

            // Servis Awake'te kurulur; ilk değer Start'ta, sonraki açılışlarda (UIDocument ağacı yeniden kurulur) burada okunur.
            if (_started)
            {
                ShowCurrent();
            }
        }

        private void OnDisable()
        {
            EventBus<XpChangedEvent>.Unsubscribe(OnXpChanged);
            EventBus<LevelUpEvent>.Unsubscribe(OnLevelUp);
            _level?.UnregisterCallback<TransitionEndEvent>(OnTransitionEnd);
        }

        private void Start()
        {
            if (!IsBound)
            {
                return;
            }

            _started = true;
            if (!ShowCurrent())
            {
                Debug.LogWarning("HudXpController: IExperienceService bulunamadı; XP barı olaylarla gelene kadar boş kalacak. "
                                 + "Sahnede [Systems]/Progression var mı?", this);
            }
        }

        private bool ShowCurrent()
        {
            if (!ServiceLocator.TryGet(out IExperienceService experience))
            {
                return false;
            }

            Show(experience.Level, experience.Xp, experience.XpToNextLevel);
            return true;
        }

        private void OnXpChanged(XpChangedEvent changed)
        {
            Show(changed.Level, changed.Xp, changed.XpToNextLevel);
        }

        private void OnLevelUp(LevelUpEvent levelUp)
        {
            _level.AddToClassList(PulseClass);
        }

        private void Show(int level, int xp, int xpToNextLevel)
        {
            _level.text = NumberText.Count(level);
            _value.text = FractionText.Get(xp, xpToNextLevel);
            _fill.style.width = Length.Percent(HudFormat.Ratio(xp, xpToNextLevel) * 100f);
        }

        private void OnTransitionEnd(TransitionEndEvent transitionEnd)
        {
            // Büyüme bitti: sınıfı kaldır, aynı transition geri küçültür.
            if (_level.ClassListContains(PulseClass))
            {
                _level.RemoveFromClassList(PulseClass);
            }
        }
    }
}
