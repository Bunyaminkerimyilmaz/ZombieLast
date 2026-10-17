using System.Collections.Generic;
using LastHaven.Core;
using UnityEngine;
using Random = System.Random;

namespace LastHaven.Economy
{
    /// <summary>
    /// [Systems]/Loot: ganimet sandıkları (GDD §9). Her gündüz başında <see cref="LootConfig.CratesPerDay"/> sandığı üs merkezine
    /// 22–38 m uzaklıkta, NavMesh üstünde ve engellerle çakışmayan rastgele noktalara koyar (<see cref="LootCratePlacer"/>);
    /// gece başında açılmamış sandıkları kaldırır. Açılan sandığın ödülünü <see cref="LootDispenser"/> dağıtır. Sandıklar
    /// <see cref="IPoolService"/>'ten gelir. <see cref="ILootCrateService"/> olarak kaydolur (HUD kenar göstergeleri).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LootCrateSpawner : MonoBehaviour, ILootCrateService, ILootCrateOwner
    {
        private const string LogPrefix = "[Economy] ";

        [SerializeField, Tooltip("Sayı, halka, yerleşim ve prefab'lar. Üretici bağlar.")]
        private LootConfig _config;

        [SerializeField, Tooltip("Açılan sandığın ödülünü dağıtan bileşen. Sahne adımı bağlar.")]
        private LootDispenser _dispenser;

        private readonly List<LootCrate> _crates = new List<LootCrate>();
        private readonly List<Vector3> _placed = new List<Vector3>();
        private LootCratePlacer _placer;
        private IPoolService _pool;
        private IInteractableRegistry _registry;
        private IBaseArea _baseArea;

        public int CrateCount => _crates.Count;

        /// <summary>Haritadaki açılmamış sandıklar (debug ve testler için).</summary>
        public IReadOnlyList<LootCrate> Crates => _crates;

        private Vector3 Center => _baseArea != null ? _baseArea.Center : Vector3.zero;

        public void GetCratePositions(List<Vector3> results)
        {
            results.Clear();
            for (int i = 0; i < _crates.Count; i++)
            {
                results.Add(_crates[i].transform.position);
            }
        }

        /// <summary>Var olan sandıkları kaldırıp yenilerini koyar (gündüz başı; debug için de). Konan sayıyı döndürür.</summary>
        public int SpawnCrates()
        {
            ClearCrates();
            if (_pool == null || _placer == null)
            {
                return 0;
            }

            Vector3 center = Center;
            LootTableData table = _config.LootTable;
            int far = 0;
            for (int i = 0; i < _config.CratesPerDay; i++)
            {
                if (!_placer.TryFindPoint(center, _placed, out Vector3 point, out Quaternion rotation))
                {
                    Debug.LogWarning($"{LogPrefix}Ganimet: {i + 1}. sandığa yer bulunamadı ({_config.PlacementAttempts} deneme); " +
                                     $"{_config.CratesPerDay} yerine {i} sandık çıktı.", this);
                    break;
                }

                LootCrate crate = _pool.Get(_config.CratePrefab, point, rotation);
                crate.Setup(this, _config, _registry);
                _crates.Add(crate);
                _placed.Add(point);
                if (table != null && PlanarDistance.Between(center, point) >= table.FarDistance)
                {
                    far++;
                }
            }

            Debug.Log($"{LogPrefix}Ganimet: {_crates.Count} sandık çıktı (yakın {_crates.Count - far}, uzak {far}).", this);
            return _crates.Count;
        }

        /// <summary>Sandık açıldı: ödülü dağıtır, sandığı listeden çıkarıp havuza iade eder.</summary>
        public void Open(LootCrate crate)
        {
            if (!_crates.Remove(crate))
            {
                return;
            }

            Vector3 position = crate.transform.position;
            float distance = PlanarDistance.Between(Center, position);
            LootRoll roll = _dispenser != null ? _dispenser.Dispense(position, distance) : default;
            Debug.Log($"{LogPrefix}Sandık açıldı ({(roll.IsFar ? "uzak" : "yakın")}, {distance:0.#} m): " +
                      $"{roll.Type} {roll.Amount}. Kalan sandık {_crates.Count}.", this);
            _pool.Release(crate.gameObject);
        }

        private void Awake()
        {
            if (_config == null || _config.CratePrefab == null)
            {
                Debug.LogError("LootCrateSpawner: LootConfig veya sandık prefab'ı atanmamış; Tools/Last Haven/Rebuild All çalıştır.", this);
                enabled = false;
                return;
            }

            if (_dispenser == null)
            {
                Debug.LogWarning("LootCrateSpawner: LootDispenser atanmamış; açılan sandıklar ödül vermeyecek.", this);
            }

            _placer = new LootCratePlacer(_config, new Random());
            ServiceLocator.Register<ILootCrateService>(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<ILootCrateService>(this);
        }

        private void Start()
        {
            if (!ServiceLocator.TryGet(out _pool))
            {
                Debug.LogWarning("LootCrateSpawner: IPoolService bulunamadı; sandık çıkmayacak.", this);
            }
            else
            {
                _pool.Prewarm(_config.CratePrefab.gameObject, _config.CratePrewarm);
            }

            if (!ServiceLocator.TryGet(out _registry))
            {
                Debug.LogWarning("LootCrateSpawner: IInteractableRegistry bulunamadı; sandıklar açılamayacak. Sahnede [Systems]/Interaction var mı?", this);
            }

            if (!ServiceLocator.TryGet(out _baseArea))
            {
                Debug.LogWarning("LootCrateSpawner: IBaseArea bulunamadı; halka dünya merkezine göre.", this);
            }
        }

        private void OnEnable()
        {
            EventBus<PhaseChangedEvent>.Subscribe(OnPhaseChanged);
        }

        private void OnDisable()
        {
            EventBus<PhaseChangedEvent>.Unsubscribe(OnPhaseChanged);
        }

        private void OnPhaseChanged(PhaseChangedEvent changed)
        {
            if (changed.Phase == GamePhase.Day)
            {
                SpawnCrates();
            }
            else if (changed.Phase == GamePhase.Night && _crates.Count > 0)
            {
                int removed = _crates.Count;
                ClearCrates();
                Debug.Log($"{LogPrefix}Ganimet: gece başladı, açılmamış {removed} sandık kaldırıldı.", this);
            }
        }

        private void ClearCrates()
        {
            for (int i = 0; i < _crates.Count; i++)
            {
                if (_crates[i] != null)
                {
                    _pool?.Release(_crates[i].gameObject);
                }
            }

            _crates.Clear();
            _placed.Clear();
        }
    }
}
