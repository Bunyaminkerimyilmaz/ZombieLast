using LastHaven.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace LastHaven.UI
{
    /// <summary>
    /// Boss can barı (GDD §16): <see cref="BossSpawnedEvent"/> gelince ekranın üst ortasında boss adı + büyük can barı açılır,
    /// <see cref="BossHealthChangedEvent"/> ile güncellenir, <see cref="BossDefeatedEvent"/> (ya da yeni gündüz) ile kapanır. Durum
    /// <see cref="BossBarState{TBoss}"/>'te. Ad boss verisinin <c>DisplayName</c>'inden. HUD'un UIDocument'ını paylaşır.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HudBossController : UIDocumentController
    {
        private readonly BossBarState<GameObject> _state = new BossBarState<GameObject>();
        private VisualElement _panel;
        private Label _name;
        private BarView _bar;

        protected override void BindElements()
        {
            _panel = Require<VisualElement>(HudElementNames.BossPanel);
            _name = Require<Label>(HudElementNames.BossName);
            _bar = new BarView(Require<VisualElement>(HudElementNames.BossHealthFill), Require<Label>(HudElementNames.BossHealthValue));
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (!IsBound)
            {
                return;
            }

            EventBus<BossSpawnedEvent>.Subscribe(OnBossSpawned);
            EventBus<BossHealthChangedEvent>.Subscribe(OnBossHealthChanged);
            EventBus<BossDefeatedEvent>.Subscribe(OnBossDefeated);
            EventBus<PhaseChangedEvent>.Subscribe(OnPhaseChanged);

            // UIDocument ağacı her açılışta yeniden kurulur: süren boss barı yeniden yazılsın.
            Apply();
        }

        private void OnDisable()
        {
            EventBus<BossSpawnedEvent>.Unsubscribe(OnBossSpawned);
            EventBus<BossHealthChangedEvent>.Unsubscribe(OnBossHealthChanged);
            EventBus<BossDefeatedEvent>.Unsubscribe(OnBossDefeated);
            EventBus<PhaseChangedEvent>.Unsubscribe(OnPhaseChanged);
        }

        private void OnBossSpawned(BossSpawnedEvent spawned)
        {
            _state.Spawn(spawned.Boss, spawned.DisplayName, spawned.MaxHealth);
            Apply();
        }

        private void OnBossHealthChanged(BossHealthChangedEvent changed)
        {
            if (_state.SetHealth(changed.Boss, changed.CurrentHealth, changed.MaxHealth))
            {
                _bar.Set(_state.Current, _state.Max);
            }
        }

        private void OnBossDefeated(BossDefeatedEvent defeated)
        {
            if (_state.Defeat(defeated.Boss))
            {
                Apply();
            }
        }

        private void OnPhaseChanged(PhaseChangedEvent changed)
        {
            // Güvenlik: boss bir şekilde yenilmeden kaybolduysa bar yeni gündüze taşınmasın.
            if (changed.Phase == GamePhase.Day && _state.IsVisible)
            {
                _state.Hide();
                Apply();
            }
        }

        private void Apply()
        {
            _panel.SetShown(_state.IsVisible);
            if (_state.IsVisible)
            {
                _name.text = _state.Name;
                _bar.Set(_state.Current, _state.Max);
            }
        }
    }
}
