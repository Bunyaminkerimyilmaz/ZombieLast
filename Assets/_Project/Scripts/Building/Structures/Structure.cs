using System;
using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Building
{
    /// <summary>
    /// Izgaraya yerleşen yapının kökü (GDD §11): can (<see cref="Core.Health"/>), düşman hedefi (<see cref="ITargetable"/>, tür
    /// <see cref="BuildableData.TargetType"/>), seviye ve toplam harcama (<see cref="StructureLevelState"/>). <see cref="Upgrade"/>
    /// sonraki seviyeyi parayla alır (can artar, görsel değişir); <see cref="Sell"/> harcananın iadesini (<see cref="IEconomyRules"/>)
    /// verip yapıyı kaldırır. Canı bitince <see cref="StructureDestroyedEvent"/> gönderir ve ızgaradan kalkar. Havuzdan gelir;
    /// <see cref="BuildSite"/> yerleştirince <see cref="Place"/> çağırır. Gündüz kuralı (GDD §11) inşa modunda; bu bileşen evreye bakmaz.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Health))]
    public sealed class Structure : MonoBehaviour, ITargetable, IPoolable
    {
        private const string LogPrefix = "[Building] ";

        private Health _health;
        private Collider _collider;
        private StructureLevelState _levels;
        private BuildSite _site;
        private BuildableData _data;
        private StructureServices _services;

        /// <summary>Yapı tipi; yerleşmeden önce null.</summary>
        public BuildableData Data => _data;

        public Health Health
        {
            get
            {
                EnsureInitialized();
                return _health;
            }
        }

        /// <summary>Izgarada mı (yerleşti ve henüz satılmadı/yıkılmadı).</summary>
        public bool IsPlaced { get; private set; }

        /// <summary>1 tabanlı seviye.</summary>
        public int Level => Progress.LevelIndex + 1;

        public int MaxLevel => Progress.LevelCount;

        /// <summary>İnşa + yükseltmelere harcanan para.</summary>
        public int TotalSpent => Progress.TotalSpent;

        /// <summary>Şu anki seviye verisi; yerleşmeden önce null.</summary>
        public BuildableLevel CurrentLevel => Progress.Current;

        public bool CanUpgrade => IsPlaced && Progress.CanUpgrade;

        /// <summary>Sonraki seviyenin fiyatı; en üst seviyede 0.</summary>
        public int UpgradeCost => Progress.UpgradeCost;

        /// <summary>Şimdi satılırsa iade (<see cref="IEconomyRules"/> yoksa 0).</summary>
        public int SellRefund => _services?.Rules != null ? _services.Rules.GetSellRefund(TotalSpent) : 0;

        /// <summary>Dünya uzayındaki kutu: kökteki collider; yoksa pivotta boş kutu.</summary>
        public Bounds Bounds => _collider != null && _collider.enabled ? _collider.bounds : new Bounds(transform.position, Vector3.zero);

        public Transform Transform => transform;

        public TargetType TargetType => _data != null ? _data.TargetType : TargetType.Barricade;

        public bool IsTargetable => IsPlaced && isActiveAndEnabled && Health.IsAlive;

        private StructureProgress Progress
        {
            get
            {
                EnsureInitialized();
                return _levels.Progress;
            }
        }

        /// <summary>
        /// <see cref="BuildSite"/> yerleştirince çağırır: 1. seviye, tam can ve görsel; harcama = inşa fiyatı; hedef kaydına girer.
        /// </summary>
        /// <param name="site">Satış ve yıkılmada yapıyı ızgaradan kaldıran alan. Testlerde null olabilir.</param>
        public void Place(BuildSite site, BuildableData data, int buildCost)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            EnsureInitialized();
            _services ??= StructureServices.Resolve(this);
            _site = site;
            _data = data;
            _levels.Reset(data, buildCost, _services.Modifiers);
            IsPlaced = true;
            Bind();
        }

        /// <summary>Sonraki seviyeyi parasıyla alır: maks. can artar (eksik can korunur), görsel değişir, olay gönderilir.</summary>
        public StructureUpgradeResult Upgrade()
        {
            if (!IsPlaced)
            {
                return StructureUpgradeResult.NotPlaced;
            }

            if (!Progress.CanUpgrade)
            {
                return StructureUpgradeResult.MaxLevel;
            }

            int cost = Progress.UpgradeCost;
            if (_services.Wallet != null && !_services.Wallet.TrySpend(WalletResource.Money, cost))
            {
                return StructureUpgradeResult.NotEnoughMoney;
            }

            _levels.Advance();
            EventBus<StructureUpgradedEvent>.Raise(new StructureUpgradedEvent(gameObject, _data.Id, Level, cost));
            Debug.Log(FormattableString.Invariant(
                $"{LogPrefix}StructureUpgradedEvent: {_data.DisplayName} → {CurrentLevel.DisplayName} ({Level}/{MaxLevel}), -{cost} para, can {_health.Current:F0}/{_health.Max:F0}."),
                this);
            return StructureUpgradeResult.Upgraded;
        }

        /// <summary>İadeyi cüzdana ekler ve yapıyı ızgaradan kaldırır (havuza döner). Izgarada değilse false.</summary>
        public bool Sell()
        {
            if (!IsPlaced || _site == null || !_site.TryGetPlaced(gameObject, out _))
            {
                return false;
            }

            int refund = SellRefund;
            _services.Wallet?.Add(WalletResource.Money, refund);
            return _site.Remove(gameObject, StructureRemovalReason.Sold, refund);
        }

        public void OnSpawned()
        {
        }

        public void OnDespawned()
        {
            Unbind();
            IsPlaced = false;
            _site = null;
            _levels?.Restore();
        }

        private void Awake()
        {
            EnsureInitialized();
        }

        private void OnEnable()
        {
            if (IsPlaced)
            {
                Bind();
            }
        }

        private void OnDisable()
        {
            Unbind();
        }

        // Tembel kurulum: EditMode testlerinde (Awake yokken) de çalışır.
        private void EnsureInitialized()
        {
            if (_health != null)
            {
                return;
            }

            _health = GetComponent<Health>();
            _collider = GetComponent<Collider>();
            _levels = new StructureLevelState(_health, VisualRoot.Find(transform));
        }

        // Ölüm dinleyicisi + hedef kaydı. Yerleşince ve (yerleşikken) yeniden etkinleşince; tekrar çağrılması zararsız.
        private void Bind()
        {
            EnsureInitialized();
            _health.Died -= OnDied;
            _health.Died += OnDied;
            _services?.Registry?.Register(this);
        }

        private void Unbind()
        {
            if (_health != null)
            {
                _health.Died -= OnDied;
            }

            _services?.Registry?.Unregister(this);
        }

        private void OnDied(DiedEvent died)
        {
            if (!IsPlaced)
            {
                return;
            }

            EventBus<StructureDestroyedEvent>.Raise(
                new StructureDestroyedEvent(gameObject, _data.Id, transform.position, Bounds, died.KillingBlow));
            GameObject source = died.KillingBlow.Source;
            Debug.Log($"{LogPrefix}StructureDestroyedEvent: {_data.DisplayName} yıkıldı (son vuran: {(source != null ? source.name : "bilinmiyor")}).", this);

            if (_site == null || !_site.Remove(gameObject, StructureRemovalReason.Destroyed, 0))
            {
                OnDespawned();
            }
        }
    }
}
