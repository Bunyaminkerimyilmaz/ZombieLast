using System;
using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Building
{
    /// <summary>
    /// Yapı onarımı (GDD §11): yerleşik ve hasarlı yapı <see cref="IInteractable"/> olarak onarım sunar. Oyuncu ayak izine
    /// <see cref="RepairConfig.Range"/> (2 m) yakınken E'yi basılı tutar; her tur (0.1 sn) 1 para alır, 5 can onarır
    /// (<see cref="RepairSession"/>). Para biterse onarım durur. Evreye bakmaz: gündüz ve gece çalışır. Onarım sürerken yapının
    /// üstünde can barı (<see cref="StructureRepairBar"/>) görünür. Onarım parası satış iadesine sayılmaz (yatırım değil, bakım).
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Structure))]
    public sealed class StructureRepair : MonoBehaviour, IInteractable, IPoolable
    {
        private const string LogPrefix = "[Building] ";

        [SerializeField, Tooltip("Onarım hızı, maliyeti, mesafesi ve ipucu metni (GDD §11). Üretici bağlar.")]
        private RepairConfig _config;

        [SerializeField, Tooltip("Yapının üstündeki onarım barı (opsiyonel). Üretici bağlar.")]
        private StructureRepairBar _bar;

        private Structure _structure;
        private IInteractableRegistry _registry;
        private RepairSession _session;
        private string _prompt = string.Empty;
        private int _promptCost = -1;

        public RepairConfig Config => _config;

        /// <summary>Kesintisiz son onarımda harcanan para (tam onarılınca veya para bitince sıfırlanır).</summary>
        public int SessionSpent => _session != null ? _session.Spent : 0;

        /// <summary>Şimdi tamamen onarmanın parası; onarım gerekmiyorsa 0.</summary>
        public int CostToFull => _config != null ? RepairRule.CostToFull(Owner.Health.Current, Owner.Health.Max, _config.Settings) : 0;

        public Vector3 PromptPosition
        {
            get
            {
                Bounds bounds = Owner.Bounds;
                return new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
            }
        }

        /// <summary>"Onar (12)"; metin sadece maliyet değişince yeniden kurulur.</summary>
        public string Prompt
        {
            get
            {
                int cost = CostToFull;
                if (cost != _promptCost && _config != null)
                {
                    _promptCost = cost;
                    _prompt = _config.FormatPrompt(cost);
                }

                return _prompt;
            }
        }

        public bool CanInteract
        {
            get
            {
                if (_config == null || !enabled || !gameObject.activeInHierarchy || !Owner.IsPlaced)
                {
                    return false;
                }

                Health health = Owner.Health;
                return health.IsAlive && RepairRule.NeedsRepair(health.Current, health.Max);
            }
        }

        public float HoldDuration => _config != null ? _config.Settings.CycleDuration : 0f;

        public float InteractionRange => _config != null ? _config.Range : 0f;

        private Structure Owner => _structure != null ? _structure : _structure = GetComponent<Structure>();

        public float GetDistance(Vector3 point)
        {
            return PlanarDistance.ToBounds(point, Owner.Bounds);
        }

        public void OnHoldProgress(float progress)
        {
            if (progress > 0f)
            {
                ShowBar();
            }
        }

        public bool OnInteract()
        {
            if (_config == null || !Owner.IsPlaced)
            {
                return false;
            }

            ResolveServices();
            Health health = Owner.Health;
            switch (_session.Step(health, _config.Settings))
            {
                case RepairStepResult.Repaired:
                    ShowBar();
                    if (!RepairRule.NeedsRepair(health.Current, health.Max))
                    {
                        LogAndReset("onarıldı");
                    }

                    return true;
                case RepairStepResult.NotEnoughMoney:
                    LogAndReset("onarımı durdu (para yetmiyor)");
                    return false;
                case RepairStepResult.AlreadyFull:
                    return true;
                default:
                    return false;
            }
        }

        public void OnSpawned()
        {
            _session?.Reset();
        }

        public void OnDespawned()
        {
            _session?.Reset();
            if (_bar != null)
            {
                _bar.Hide();
            }
        }

        private void Awake()
        {
            if (_config == null)
            {
                Debug.LogWarning($"StructureRepair: '{name}' için RepairConfig atanmamış; yapı onarılamayacak. Tools/Last Haven/Generate Prefabs çalıştır.", this);
            }
        }

        // Kayıt etkinken: havuzdaki (kapalı) yapılar adaylara girmez. Servisler ilk etkinleşmede (oyun başladıktan sonra) alınır.
        private void OnEnable()
        {
            ResolveServices();
            _registry?.Register(this);
        }

        private void OnDisable()
        {
            _registry?.Unregister(this);
        }

        // Tembel: EditMode testlerinde (OnEnable yokken) ilk OnInteract'te.
        private void ResolveServices()
        {
            if (_session != null)
            {
                return;
            }

            if (!ServiceLocator.TryGet(out _registry))
            {
                Debug.LogWarning("StructureRepair: IInteractableRegistry bulunamadı; yapılar onarılamayacak. Sahnede [Systems]/Interaction var mı?", this);
            }

            if (!ServiceLocator.TryGet(out IWallet wallet))
            {
                Debug.LogWarning("StructureRepair: IWallet bulunamadı; onarım ücretsiz olacak.", this);
            }

            _session = new RepairSession(wallet);
        }

        private void ShowBar()
        {
            if (_bar != null)
            {
                _bar.Show(Owner.Health.Normalized, _config != null ? _config.BarLingerDuration : 0f);
            }
        }

        // Oturum özeti: tam onarılınca veya para bitince (her turda log yok).
        private void LogAndReset(string what)
        {
            Health health = Owner.Health;
            string displayName = Owner.Data != null ? Owner.Data.DisplayName : name;
            Debug.Log(FormattableString.Invariant(
                $"{LogPrefix}{displayName} {what}: can {health.Current:F0}/{health.Max:F0}, -{_session.Spent} para."), this);
            _session.Reset();
        }
    }
}
