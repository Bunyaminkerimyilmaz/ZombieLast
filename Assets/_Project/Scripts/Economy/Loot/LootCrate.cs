using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Economy
{
    /// <summary>Sandığı açılınca ödülü dağıtan ve sandığı kaldıran sahip (<see cref="LootCrateSpawner"/>; testlerde sahte).</summary>
    public interface ILootCrateOwner
    {
        /// <summary>Sandık açıldı: ödülü dağıt, sandığı haritadan kaldır.</summary>
        void Open(LootCrate crate);
    }

    /// <summary>
    /// Havuzlanan ganimet sandığı (GDD §9; kök Interactable layer'ında, model "Visual" altında). <see cref="IInteractable"/>:
    /// oyuncu <see cref="LootConfig.InteractionRange"/> içindeyken E'yi <see cref="LootConfig.OpenHoldDuration"/> (2 sn) basılı
    /// tutunca açılır; bırakırsa ilerleme sıfırlanır (<c>InteractionHold</c>). Açılınca sahibi ödülü dağıtır ve sandığı havuza
    /// iade eder. <see cref="Setup"/> çağrılmadan (sahibi yokken) etkileşim sunmaz.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LootCrate : MonoBehaviour, IInteractable, IPoolable
    {
        private ILootCrateOwner _owner;
        private LootConfig _config;
        private IInteractableRegistry _registry;
        private Collider _collider;

        /// <summary>Bu doğuşta açıldı (havuza dönene kadar).</summary>
        public bool IsOpened { get; private set; }

        public Vector3 PromptPosition
        {
            get
            {
                Bounds bounds = Bounds;
                return new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
            }
        }

        /// <summary>"Aç"; config'teki string (her çağrıda aynı örnek).</summary>
        public string Prompt => _config != null ? _config.OpenPrompt : string.Empty;

        // isActiveAndEnabled EditMode testlerinde false döner; StructureRepair gibi ikisine ayrı bakılır.
        public bool CanInteract => _owner != null && !IsOpened && enabled && gameObject.activeInHierarchy;

        public float HoldDuration => _config != null ? _config.OpenHoldDuration : 0f;

        public float InteractionRange => _config != null ? _config.InteractionRange : 0f;

        private Bounds Bounds
        {
            get
            {
                if (_collider == null)
                {
                    _collider = GetComponent<Collider>();
                }

                return _collider != null ? _collider.bounds : new Bounds(transform.position, Vector3.zero);
            }
        }

        /// <summary>
        /// Havuzdan alındıktan hemen sonra sahibi çağırır: sandık kapalı başlar ve etkileşim kaydına girer
        /// (<paramref name="registry"/> null ise kaydolmaz; testler).
        /// </summary>
        public void Setup(ILootCrateOwner owner, LootConfig config, IInteractableRegistry registry)
        {
            _owner = owner;
            _config = config;
            IsOpened = false;
            _registry = registry;
            _registry?.Register(this);
        }

        public float GetDistance(Vector3 point)
        {
            return PlanarDistance.ToBounds(point, Bounds);
        }

        public void OnHoldProgress(float progress)
        {
            // İlerleme halkası UI'da (InteractionPromptEvent).
        }

        /// <summary>Sandığı açar ve sahibine bildirir. Her durumda false: bir basışta bir sandık (tutma durur).</summary>
        public bool OnInteract()
        {
            if (!CanInteract)
            {
                return false;
            }

            IsOpened = true;
            _owner.Open(this);
            return false;
        }

        public void OnSpawned()
        {
        }

        public void OnDespawned()
        {
            _registry?.Unregister(this);
            _registry = null;
            _owner = null;
            IsOpened = false;
        }

        // Havuz objeyi kapatınca (veya sahne kapanınca) kayıttan çıkar; tekrar açılırsa ve hâlâ sahipliyse geri kaydolur.
        private void OnEnable()
        {
            if (_owner != null)
            {
                _registry?.Register(this);
            }
        }

        private void OnDisable()
        {
            _registry?.Unregister(this);
        }
    }
}
