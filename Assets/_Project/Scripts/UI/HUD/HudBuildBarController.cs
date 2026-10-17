using System.Collections.Generic;
using LastHaven.Building;
using LastHaven.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace LastHaven.UI
{
    /// <summary>
    /// HUD'un inşa çubuğu (GDD §16): inşa modunda alt ortada <see cref="BuildConfig"/>'in dolu yuvaları için birer kart
    /// (<see cref="BuildCardView"/>: kısayol numarası, ad, fiyat, ikon ya da renk kutusu). Seçili kart vurgulu, parası yetmeyen
    /// soluk; altında seçili yapının açıklaması ve tuş ipuçları. HUD'un UIDocument'ını paylaşır. Veriler olaylarla:
    /// <see cref="BuildModeChangedEvent"/> (göster/gizle), <see cref="BuildSelectionChangedEvent"/>, <see cref="MoneyChangedEvent"/>;
    /// Update yok.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HudBuildBarController : UIDocumentController
    {
        [SerializeField, Tooltip("Yapı yuvaları (Data/Building/BuildConfig.asset). Sahne adımı bağlar.")]
        private BuildConfig _config;

        [SerializeField, Tooltip("Kart şablonu (UI/HUD/BuildCard.uxml). Sahne adımı bağlar.")]
        private VisualTreeAsset _cardTemplate;

        private readonly List<BuildCardView> _cards = new List<BuildCardView>();
        private VisualElement _bar;
        private VisualElement _cardContainer;
        private Label _description;
        private Label _noSelection;
        private IWallet _wallet;
        private int _selectedSlot = -1;
        private int _money;
        private bool _started;

        protected override void BindElements()
        {
            _bar = Require<VisualElement>(HudElementNames.BuildBar);
            _cardContainer = Require<VisualElement>(HudElementNames.BuildCards);
            _description = Require<Label>(HudElementNames.BuildDescription);
            _noSelection = Require<Label>(HudElementNames.BuildNoSelection);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (!IsBound)
            {
                return;
            }

            _bar.SetShown(false);
            EventBus<BuildModeChangedEvent>.Subscribe(OnBuildModeChanged);
            EventBus<BuildSelectionChangedEvent>.Subscribe(OnSelectionChanged);
            EventBus<MoneyChangedEvent>.Subscribe(OnMoneyChanged);

            // UIDocument her açılışta ağacı yeniden kurar: kartlar yeni elementlere eklenir. İlk kurulumu Start yapar.
            if (_started)
            {
                BuildCards();
            }
        }

        private void OnDisable()
        {
            EventBus<BuildModeChangedEvent>.Unsubscribe(OnBuildModeChanged);
            EventBus<BuildSelectionChangedEvent>.Unsubscribe(OnSelectionChanged);
            EventBus<MoneyChangedEvent>.Unsubscribe(OnMoneyChanged);
        }

        private void Start()
        {
            if (!IsBound)
            {
                return;
            }

            if (_config == null || _cardTemplate == null)
            {
                Debug.LogWarning("HudBuildBarController: BuildConfig veya kart şablonu bağlı değil; inşa çubuğu boş kalacak. "
                                 + "Tools/Last Haven/Build Game Scene çalıştır.", this);
            }

            ServiceLocator.TryGet(out _wallet);
            _started = true;
            ReadMoney();
            BuildCards();
        }

        // Cüzdan yoksa (test sahnesi) hiçbir kart soluk görünmez.
        private void ReadMoney()
        {
            _money = _wallet != null ? _wallet.Money : int.MaxValue;
        }

        private void BuildCards()
        {
            _cards.Clear();
            _cardContainer.Clear();
            if (_config == null || _cardTemplate == null)
            {
                return;
            }

            IReadOnlyList<BuildableData> slots = _config.Slots;
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] == null)
                {
                    continue;
                }

                BuildCardView card = BuildCardView.Create(_cardTemplate, i, slots[i]);
                if (card != null)
                {
                    _cardContainer.Add(card.Root);
                    _cards.Add(card);
                }
            }

            ShowSelection(-1);
        }

        private void OnBuildModeChanged(BuildModeChangedEvent changed)
        {
            _bar.SetShown(changed.IsActive);
            if (changed.IsActive)
            {
                ReadMoney();
                ShowSelection(-1);
            }
        }

        private void OnSelectionChanged(BuildSelectionChangedEvent changed)
        {
            ShowSelection(changed.SlotIndex);
        }

        private void OnMoneyChanged(MoneyChangedEvent changed)
        {
            _money = changed.Money;
            ApplyCards();
        }

        private void ShowSelection(int slotIndex)
        {
            _selectedSlot = slotIndex;
            BuildableData selected = FindCard(slotIndex)?.Data;
            bool hasSelection = selected != null;
            _description.text = hasSelection ? selected.Description : string.Empty;
            _description.SetShown(hasSelection);
            _noSelection.SetShown(!hasSelection);
            ApplyCards();
        }

        private void ApplyCards()
        {
            for (int i = 0; i < _cards.Count; i++)
            {
                _cards[i].Apply(_cards[i].SlotIndex == _selectedSlot, _money);
            }
        }

        private BuildCardView FindCard(int slotIndex)
        {
            for (int i = 0; i < _cards.Count; i++)
            {
                if (_cards[i].SlotIndex == slotIndex)
                {
                    return _cards[i];
                }
            }

            return null;
        }
    }
}
