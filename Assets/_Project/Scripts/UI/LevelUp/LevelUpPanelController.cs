using LastHaven.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace LastHaven.UI
{
    /// <summary>
    /// [Systems]/UI/LevelUpPanel: seviye kartları (GDD §13, §16). <see cref="LevelUpEvent"/> gelince (veya bekleyen seviye varken
    /// koşullar uygun olunca: <see cref="LevelUpPanelRules"/>) oyun duraklar ve 3 kart açılır; seçim tıklama, 1–3
    /// (<see cref="IInputReader.OptionPressed"/>) veya gamepad D-pad + A ile. Kısa seçim animasyonundan sonra kuyrukta seviye varsa
    /// onun kartları gelir, yoksa oyun devam eder; run biterse kartlar seçimsiz kapanır. Akış saf <see cref="LevelUpSequence"/>'ta
    /// (gerçek zamanla ilerler), elementler <see cref="LevelUpPanelView"/>'da, girdi modu <see cref="MenuInputScope"/>'ta; bu bileşen
    /// servisler, olaylar ve kare döngüsü.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LevelUpPanelController : UIScreen
    {
        private const string LogPrefix = "[UI] ";

        [SerializeField, Tooltip("Giriş gecikmesi ve seçim animasyonu süresi (Data/UI/LevelUpPanelConfig.asset). Sahne adımı bağlar.")]
        private LevelUpPanelConfig _config;

        [SerializeField, Tooltip("Kart şablonu (UI/LevelUp/PerkCard.uxml). Sahne adımı bağlar.")]
        private VisualTreeAsset _cardTemplate;

        private LevelUpPanelView _view;
        private LevelUpSequence _sequence;
        private MenuInputScope _menuInput;
        private IExperienceService _experience;
        private IPerkService _perks;
        private IInputReader _input;
        private IPauseService _pause;
        private IGameStateService _gameState;
        private int _shownFrame;

        /// <summary>Debug ve duman testi için; Start'tan önce veya servis yoksa null.</summary>
        public LevelUpSequence Sequence => _sequence;

        protected override void BindElements()
        {
            // UIDocument her açılışta ağacı yeniden kurar: görünüm her OnEnable'da yeni elementlere bağlanır.
            _view = new LevelUpPanelView(ScreenElement, Require<Label>(LevelUpElementNames.LevelValue),
                Require<VisualElement>(LevelUpElementNames.Cards), Require<VisualElement>(LevelUpElementNames.Pending),
                Require<Label>(LevelUpElementNames.PendingValue));
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

            EventBus<LevelUpEvent>.Subscribe(OnLevelUp);
            EventBus<PhaseChangedEvent>.Subscribe(OnPhaseChanged);
            if (_sequence != null && _input != null) // İlk girdi aboneliği Start'ta (servis orada alınır).
            {
                _input.OptionPressed += OnCardChosen;
            }
        }

        private void OnDisable()
        {
            CloseCards("panel kapandı");
            EventBus<LevelUpEvent>.Unsubscribe(OnLevelUp);
            EventBus<PhaseChangedEvent>.Unsubscribe(OnPhaseChanged);
            if (_input != null)
            {
                _input.OptionPressed -= OnCardChosen;
            }
        }

        private void Start()
        {
            if (!IsBound)
            {
                return;
            }

            if (_config == null || _cardTemplate == null || !ServiceLocator.TryGet(out _experience) || !ServiceLocator.TryGet(out _perks))
            {
                Debug.LogWarning("LevelUpPanelController: config, kart şablonu, IExperienceService veya IPerkService yok; seviye kartları " +
                                 "açılmayacak. Tools/Last Haven/Build Game Scene çalıştır.", this);
                return;
            }

            if (!ServiceLocator.TryGet(out _pause))
            {
                Debug.LogWarning("LevelUpPanelController: IPauseService yok; kartlar açıkken oyun durmayacak. Sahnede [Systems]/Pause var mı?", this);
            }

            ServiceLocator.TryGet(out _input);
            ServiceLocator.TryGet(out _gameState);
            _menuInput = new MenuInputScope(_input);
            _sequence = new LevelUpSequence(_experience, _perks, _pause, _config.InputDelay, _config.ConfirmDuration);
            _sequence.LevelSkipped += level => Debug.Log($"{LogPrefix}Seviye {level}: alınabilecek perk kalmadı; kart açılmadı.", this);
            if (_input != null)
            {
                _input.OptionPressed += OnCardChosen;
            }

            TryOpen();
        }

        private void Update()
        {
            if (_sequence == null)
            {
                return;
            }

            if (!_sequence.IsOpen)
            {
                TryOpen(); // İnşa modu / menü yüzünden bekleyen seviye: koşullar uygun olunca açılır (kuyruk boşsa etkisiz).
                return;
            }

            _view.Tick(_menuInput.IsUsingGamepad, _sequence.Stage == LevelUpStage.Choosing, Time.frameCount > _shownFrame);
            LevelUpTickResult result = _sequence.Tick(Time.unscaledDeltaTime);
            if (result == LevelUpTickResult.NextOffer)
            {
                ShowOffer();
            }
            else if (result == LevelUpTickResult.Closed)
            {
                HidePanel("perk seçildi");
            }
        }

        private void TryOpen()
        {
            if (_sequence == null || _sequence.IsOpen || !IsBound
                || !LevelUpPanelRules.CanOpen(_menuInput.Mode, _pause, _gameState) || !_sequence.TryOpen())
            {
                return;
            }

            _menuInput.Enter();
            SetShown(true);
            ShowOffer();
        }

        private void ShowOffer()
        {
            int pendingAfter = _experience.PendingLevelUps;
            _view.Show(_sequence.Level, pendingAfter, _sequence.Offer, _perks, _cardTemplate, OnCardChosen);
            _shownFrame = Time.frameCount;
            Debug.Log($"{LogPrefix}Seviye {_sequence.Level} kartları: {string.Join(", ", _sequence.Offer)} (sırada {pendingAfter}).", this);
        }

        /// <summary>Kart tıklandı veya 1–3'e basıldı (kabul edilmeyen seçim sessizce yok sayılır).</summary>
        private void OnCardChosen(int index)
        {
            if (_sequence == null || !_sequence.Choose(index))
            {
                return;
            }

            string perkId = _sequence.Offer[index];
            _view.ReleaseFocus();
            _view.PlayChoice(index, _perks.GetStacks(perkId), _perks.GetMaxStacks(perkId));
        }

        private void OnLevelUp(LevelUpEvent levelUp) => TryOpen();

        private void OnPhaseChanged(PhaseChangedEvent changed)
        {
            if (!LevelUpPanelRules.IsRunActive(changed.Phase))
            {
                CloseCards(changed.Phase.ToString());
            }
        }

        /// <summary>Kartları seçimsiz kapatır (run bitti, bileşen kapandı) ve oyunu devam ettirir.</summary>
        private void CloseCards(string reason)
        {
            if (_sequence != null && _sequence.IsOpen)
            {
                _sequence.Close();
                HidePanel(reason);
            }
        }

        private void HidePanel(string reason)
        {
            _view?.ReleaseFocus();
            _view?.Clear();
            SetShown(false);
            _menuInput.Exit();
            Debug.Log($"{LogPrefix}Seviye kartları kapandı ({reason}).", this);
        }
    }
}
