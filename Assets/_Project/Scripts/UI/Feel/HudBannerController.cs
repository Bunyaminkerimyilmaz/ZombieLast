using LastHaven.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace LastHaven.UI
{
    /// <summary>
    /// Evre geçişlerinde ekran ortasında büyük başlık (GDD §16): gündüz → "GÜN 2", gece → "GECE 3" (+ varsa gece olayının adı),
    /// şafak → "ŞAFAK", boss doğunca → "BOSS" + adı. Belirir (büyükten iner), durur, söner; üst üste gelenler sırayla
    /// (<see cref="BannerQueue{T}"/>). Oyun bitince (Oyun Bitti / Zafer) başlıklar silinir. Metinler UXML'de; kod sayıyı ve adı yazar.
    /// Oyun zamanıyla ilerler (duraklatmada bekler). HUD'un UIDocument'ını paylaşır.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HudBannerController : UIDocumentController
    {
        [SerializeField, Tooltip("Başlık zamanlaması ve sıra sınırı. Üretici bağlar.")]
        private HudFeelConfig _config;

        private VisualElement _banner;
        private VisualElement _day;
        private VisualElement _night;
        private VisualElement _dawn;
        private VisualElement _boss;
        private Label _dayNumber;
        private Label _nightNumber;
        private Label _subtitle;
        private BannerQueue<Banner> _queue;
        private IWaveService _waves;

        private enum BannerKind
        {
            Day,
            Night,
            Dawn,
            Boss,
        }

        protected override void BindElements()
        {
            _banner = Require<VisualElement>(HudElementNames.Banner);
            _day = Require<VisualElement>(HudElementNames.BannerDay);
            _night = Require<VisualElement>(HudElementNames.BannerNight);
            _dawn = Require<VisualElement>(HudElementNames.BannerDawn);
            _boss = Require<VisualElement>(HudElementNames.BannerBoss);
            _dayNumber = Require<Label>(HudElementNames.BannerDayNumber);
            _nightNumber = Require<Label>(HudElementNames.BannerNightNumber);
            _subtitle = Require<Label>(HudElementNames.BannerSubtitle);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (!IsBound)
            {
                return;
            }

            if (_config == null)
            {
                Debug.LogError("HudBannerController: HudFeelConfig atanmamış; Tools/Last Haven/Build Game Scene çalıştır.", this);
                return;
            }

            _queue ??= new BannerQueue<Banner>(_config.CreateBannerTiming(), _config.BannerQueueLimit);
            EventBus<PhaseChangedEvent>.Subscribe(OnPhaseChanged);
            EventBus<BossSpawnedEvent>.Subscribe(OnBossSpawned);

            // UIDocument ağacı her açılışta yeniden kurulur: süren başlık yeniden yazılsın.
            Apply();
        }

        private void OnDisable()
        {
            EventBus<PhaseChangedEvent>.Unsubscribe(OnPhaseChanged);
            EventBus<BossSpawnedEvent>.Unsubscribe(OnBossSpawned);
        }

        private void Start()
        {
            ServiceLocator.TryGet(out _waves);
        }

        private void Update()
        {
            if (_queue == null || !IsBound)
            {
                return;
            }

            if (_queue.Tick(Time.deltaTime))
            {
                Apply();
            }

            if (_queue.IsShowing)
            {
                BannerPose pose = _queue.Evaluate();
                _banner.style.opacity = pose.Opacity;
                _banner.style.scale = new Scale(new Vector2(pose.Scale, pose.Scale));
            }
        }

        private void OnPhaseChanged(PhaseChangedEvent changed)
        {
            switch (changed.Phase)
            {
                case GamePhase.Day:
                    Show(new Banner(BannerKind.Day, changed.NightIndex + 1, null));
                    break;
                case GamePhase.Night:
                    Show(new Banner(BannerKind.Night, changed.NightIndex + 1, GetEventName(changed.NightIndex)));
                    break;
                case GamePhase.Dawn:
                    Show(new Banner(BannerKind.Dawn, 0, null));
                    break;
                case GamePhase.GameOver:
                case GamePhase.Victory:
                    // Run sonu panelleri kendi başlığını gösterir.
                    _queue.Clear();
                    Apply();
                    break;
            }
        }

        private void OnBossSpawned(BossSpawnedEvent spawned)
        {
            Show(new Banner(BannerKind.Boss, 0, spawned.DisplayName));
        }

        private void Show(Banner banner)
        {
            if (_queue.Enqueue(banner))
            {
                Apply();
            }
        }

        private void Apply()
        {
            bool showing = _queue != null && _queue.IsShowing;
            _banner.SetShown(showing);
            if (!showing)
            {
                return;
            }

            Banner banner = _queue.Current;
            _day.SetShown(banner.Kind == BannerKind.Day);
            _night.SetShown(banner.Kind == BannerKind.Night);
            _dawn.SetShown(banner.Kind == BannerKind.Dawn);
            _boss.SetShown(banner.Kind == BannerKind.Boss);
            _dayNumber.text = NumberText.Count(banner.Number);
            _nightNumber.text = NumberText.Count(banner.Number);
            _subtitle.text = banner.Subtitle ?? string.Empty;
            _subtitle.SetShown(!string.IsNullOrEmpty(banner.Subtitle));

            BannerPose pose = _queue.Evaluate();
            _banner.style.opacity = pose.Opacity;
            _banner.style.scale = new Scale(new Vector2(pose.Scale, pose.Scale));
        }

        private string GetEventName(int nightIndex)
        {
            if (_waves == null || nightIndex < 0 || nightIndex >= _waves.TotalNights)
            {
                return null;
            }

            return _waves.GetTonightInfo(nightIndex).EventName;
        }

        private readonly struct Banner
        {
            public Banner(BannerKind kind, int number, string subtitle)
            {
                Kind = kind;
                Number = number;
                Subtitle = subtitle;
            }

            public BannerKind Kind { get; }

            public int Number { get; }

            public string Subtitle { get; }
        }
    }
}
