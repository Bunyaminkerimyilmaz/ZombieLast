using System;
using System.Collections.Generic;
using LastHaven.Core;
using UnityEngine;
using UnityEngine.AI;

namespace LastHaven.Building
{
    /// <summary>
    /// [Systems]/BuildMode: inşa ızgarası (<see cref="GridService"/>) ve yerleştirilmiş yapılar. Yerleşimi değerlendirir
    /// (ızgara kuralları + ayak izinde oyuncu/düşman/engel + para), yapıyı parasını ödeyip havuzdan yerleştirir
    /// (<see cref="Structure.Place"/>) ve kaldırır (satış/yıkılma). Izgara Start'ta <see cref="IBaseArea"/>'dan kurulur;
    /// yapı prefab'ları Start'ta havuzda hazırlanır.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-10)] // BuildModeController'ın Start'ından önce ızgara hazır olsun.
    public sealed class BuildSite : MonoBehaviour
    {
        private const string LogPrefix = "[Building] ";

        [SerializeField, Tooltip("İnşa ayarları: yuvalar, ızgara, çakışma. Üretici bağlar.")]
        private BuildConfig _config;

        private readonly PlacedStructureMap _placed = new PlacedStructureMap();
        private GridService _grid;
        private IWallet _wallet;
        private IPoolService _pool;

        public BuildConfig Config => _config;

        /// <summary>İnşa ızgarası; Start'a kadar (veya üs alanı yoksa) null.</summary>
        public GridService Grid => _grid;

        public bool IsReady => _grid != null && isActiveAndEnabled;

        public int PlacedCount => _placed.Count;

        /// <summary>Yerleşimi değerlendirir: ızgara (alan, jeneratör, doluluk) → çakışma → para. Hiçbir şeyi değiştirmez.</summary>
        public BuildPlacement Evaluate(BuildableData data, Vector3 worldPoint, int rotationSteps)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            int rotation = GridService.NormalizeRotation(rotationSteps);
            RectInt cells = _grid.GetFootprintCells(worldPoint, data.Footprint, rotation);
            Vector3 position = _grid.GetFootprintCenter(cells);

            PlacementStatus status = _grid.CheckPlacement(cells);
            if (status == PlacementStatus.Valid
                && FootprintBlocking.IsBlocked(cells, _grid.CellSize, position, _config.BlockingCheckHeight, _config.BlockingLayers))
            {
                status = PlacementStatus.Blocked;
            }

            if (status == PlacementStatus.Valid && _wallet != null && !_wallet.CanAfford(WalletResource.Money, data.BuildCost))
            {
                status = PlacementStatus.NotEnoughMoney;
            }

            return new BuildPlacement(data, cells, rotation, position, status);
        }

        /// <summary>
        /// Geçerliyse fiyatı öder, yapıyı havuzdan yerleştirir, hücreleri doldurur ve <see cref="StructurePlacedEvent"/> gönderir.
        /// <paramref name="placement"/> değerlendirmenin sonucudur (başarısızlıkta nedeni).
        /// </summary>
        public bool TryPlace(BuildableData data, Vector3 worldPoint, int rotationSteps, out BuildPlacement placement)
        {
            placement = Evaluate(data, worldPoint, rotationSteps);
            if (!placement.IsValid)
            {
                return false;
            }

            if (data.Prefab == null || !data.Prefab.TryGetComponent(out Structure prefab) || _pool == null)
            {
                Debug.LogError($"{LogPrefix}'{data.Id}' yerleştirilemedi: prefab atanmamış, kökünde Structure yok veya IPoolService yok.", data);
                return false;
            }

            int cost = data.BuildCost;
            if (_wallet != null && !_wallet.TrySpend(WalletResource.Money, cost))
            {
                return false;
            }

            Structure structure = _pool.Get(prefab, placement.Position, placement.Rotation);
            if (structure == null)
            {
                _wallet?.Add(WalletResource.Money, cost);
                return false;
            }

            GameObject root = structure.gameObject;
            _grid.TryOccupy(placement.Cells);
            _placed.Add(root, new PlacedStructure(structure, data, placement.Cells, placement.RotationSteps));
            if (root.TryGetComponent(out NavMeshObstacle obstacle))
            {
                obstacle.carving = data.CarvesNavMesh;
            }

            structure.Place(this, data, cost);
            EventBus<StructurePlacedEvent>.Raise(
                new StructurePlacedEvent(root, data.Id, placement.Position, placement.RotationSteps, cost));
            Debug.Log(FormattableString.Invariant(
                $"{LogPrefix}StructurePlacedEvent: {data.DisplayName} ({placement.Position.x:F1}, {placement.Position.z:F1}), {placement.RotationSteps * 90}°, -{cost} para."),
                root);
            return true;
        }

        /// <summary>
        /// Yerleştirilmiş yapıyı kaldırır: hücreler boşalır, <see cref="StructureRemovedEvent"/> gönderilir, obje havuza döner.
        /// Bu alanın yapısı değilse false. Satış (<see cref="Structure.Sell"/>, iade önceden ödenir) ve yıkılma bunu çağırır.
        /// </summary>
        public bool Remove(GameObject structure, StructureRemovalReason reason, int refund)
        {
            if (!_placed.Remove(structure, out PlacedStructure placed))
            {
                return false;
            }

            _grid.Release(placed.Cells);
            Vector3 position = _grid.GetFootprintCenter(placed.Cells);
            EventBus<StructureRemovedEvent>.Raise(new StructureRemovedEvent(structure, placed.Data.Id, position, reason, refund));
            Debug.Log(FormattableString.Invariant($"{LogPrefix}StructureRemovedEvent: {placed.Data.DisplayName} ({reason}, +{refund} para)."), this);
            _pool.Release(structure);
            return true;
        }

        /// <summary>Obje bu alana yerleştirilmiş bir yapıysa kaydını verir.</summary>
        public bool TryGetPlaced(GameObject structure, out PlacedStructure placed)
        {
            return _placed.TryGet(structure, out placed);
        }

        /// <summary>Dünya noktasının hücresini kaplayan yapı (yükseklik yok sayılır).</summary>
        public bool TryGetStructureAt(Vector3 worldPoint, out Structure structure)
        {
            structure = null;
            if (_grid != null && _placed.TryGetAt(_grid.WorldToCell(worldPoint), out _, out PlacedStructure placed))
            {
                structure = placed.Structure;
            }

            return structure != null;
        }

        /// <summary>Collider'ın objesi veya üst objelerinden biri bu alana yerleşmiş bir yapıysa onu verir (ışın isabeti için).</summary>
        public bool TryFindStructure(Transform hit, out Structure structure)
        {
            structure = _placed.TryFind(hit, out PlacedStructure placed) ? placed.Structure : null;
            return structure != null;
        }

        private void Awake()
        {
            if (_config == null)
            {
                Debug.LogError("BuildSite: BuildConfig atanmamış; Tools/Last Haven/Build Game Scene çalıştır.", this);
                enabled = false;
            }
        }

        private void Start()
        {
            if (!ServiceLocator.TryGet(out IBaseArea area))
            {
                Debug.LogError("BuildSite: IBaseArea bulunamadı; inşa ızgarası kurulmadı. Sahnede [Gameplay]/Base var mı?", this);
                enabled = false;
                return;
            }

            if (!ServiceLocator.TryGet(out _wallet))
            {
                Debug.LogWarning("BuildSite: IWallet bulunamadı; yapılar ücretsiz yerleşecek. Sahnede [Systems]/Economy var mı?", this);
            }

            if (!ServiceLocator.TryGet(out _pool))
            {
                Debug.LogWarning("BuildSite: IPoolService bulunamadı; yapı yerleştirilemeyecek. Sahnede [Systems]/Pool var mı?", this);
            }

            _grid = new GridService(area, _config.CellSize, _config.GeneratorKeepOutHalfExtent);
            PrewarmStructures();
        }

        private void PrewarmStructures()
        {
            if (_pool == null || _config.StructurePrewarm <= 0)
            {
                return;
            }

            IReadOnlyList<BuildableData> slots = _config.Slots;
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] != null && slots[i].Prefab != null)
                {
                    _pool.Prewarm(slots[i].Prefab, _config.StructurePrewarm);
                }
            }
        }
    }
}
