using System.Collections.Generic;
using LastHaven.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace LastHaven.UI
{
    /// <summary>
    /// HUD'un gündüz istihbarat paneli (GDD §9, §16; sağ üst): bu gecenin düşman tipleri ve adetleri ("20 Yürüyen", boss satırı
    /// "BOSS" etiketli) ve varsa gece olayı (ad + açıklama, GDD §14). Sadece gündüzde görünür; bilgi <see cref="TonightIntel"/>'den gündüz başında bir kez
    /// okunur. Satır sayısı <c>HUD.uxml</c>'deki <see cref="IntelRowView.RowClass"/> sayısıdır; fazla tip varsa ilkleri gösterilir
    /// (uyarıyla). HUD'un UIDocument'ını paylaşır.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HudIntelController : UIDocumentController
    {
        private readonly List<IntelRowView> _rows = new List<IntelRowView>();
        private VisualElement _panel;
        private VisualElement _enemies;
        private VisualElement _event;
        private Label _eventName;
        private Label _eventDescription;
        private IWaveService _waves;
        private IGameStateService _gameState;
        private bool _started;

        protected override void BindElements()
        {
            _panel = Require<VisualElement>(HudElementNames.IntelPanel);
            _enemies = Require<VisualElement>(HudElementNames.IntelEnemies);
            _event = Require<VisualElement>(HudElementNames.IntelEvent);
            _eventName = Require<Label>(HudElementNames.IntelEventName);
            _eventDescription = Require<Label>(HudElementNames.IntelEventDescription);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (!IsBound)
            {
                return;
            }

            _rows.Clear();
            foreach (VisualElement element in _enemies.Query(className: IntelRowView.RowClass).Build())
            {
                var row = new IntelRowView(element);
                if (!row.IsValid)
                {
                    Debug.LogError($"HudIntelController: '{IntelRowView.RowClass}' içinde '{IntelRowView.CountClass}', " +
                                   $"'{IntelRowView.NameClass}' veya '{IntelRowView.BossClass}' yok.", this);
                    continue;
                }

                _rows.Add(row);
            }

            EventBus<PhaseChangedEvent>.Subscribe(OnPhaseChanged);

            // Servisler Start'ta alınır; sonraki açılışlarda (UIDocument ağacı yeniden kurulur) burada yeniden yazılır.
            if (_started)
            {
                ShowCurrent();
            }
        }

        private void OnDisable()
        {
            EventBus<PhaseChangedEvent>.Unsubscribe(OnPhaseChanged);
        }

        private void Start()
        {
            if (!IsBound)
            {
                return;
            }

            if (!ServiceLocator.TryGet(out _waves))
            {
                Debug.LogWarning("HudIntelController: IWaveService bulunamadı; istihbarat paneli görünmeyecek.", this);
            }

            ServiceLocator.TryGet(out _gameState);
            _started = true;
            ShowCurrent();
        }

        private void ShowCurrent()
        {
            if (_gameState != null)
            {
                Show(TonightIntel.Get(_waves, _gameState.CurrentPhase, _gameState.NightIndex));
            }
            else
            {
                Show(null);
            }
        }

        private void OnPhaseChanged(PhaseChangedEvent changed)
        {
            Show(TonightIntel.Get(_waves, changed.Phase, changed.NightIndex));
        }

        private void Show(NightInfo tonight)
        {
            _panel.SetShown(tonight != null);
            if (tonight == null)
            {
                return;
            }

            IReadOnlyList<NightEnemyInfo> enemies = tonight.Enemies;
            if (enemies.Count > _rows.Count)
            {
                Debug.LogWarning($"HudIntelController: Gece {tonight.NightNumber}'de {enemies.Count} düşman tipi var ama panelde " +
                                 $"{_rows.Count} satır; HUD.uxml'e '{IntelRowView.RowClass}' ekle.", this);
            }

            for (int i = 0; i < _rows.Count; i++)
            {
                if (i < enemies.Count)
                {
                    _rows[i].Show(enemies[i]);
                }
                else
                {
                    _rows[i].Hide();
                }
            }

            _event.SetShown(tonight.HasEvent);
            _eventName.text = tonight.EventName;
            _eventDescription.text = tonight.EventDescription;
            _eventDescription.SetShown(tonight.EventDescription.Length > 0);
        }
    }
}
