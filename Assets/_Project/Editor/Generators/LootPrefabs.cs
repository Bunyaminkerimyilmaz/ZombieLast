using System;
using LastHaven.Core;
using LastHaven.Economy;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LastHaven.Editor
{
    /// <summary>
    /// Ganimet prefab'ları (<c>Prefabs/Loot/</c>), varsa dokunulmaz:
    /// <list type="bullet">
    /// <item><c>LootCrate</c>: kök Interactable layer'ında, katı BoxCollider (oyuncu içinden geçmez; mermi maskesinde değil) +
    /// <see cref="LootCrate"/>; "Visual" altında turuncu-kahve küp (GDD §18). Pivot zeminde.</item>
    /// <item><c>FuelPickup</c> / <c>AmmoPickup</c>: kök Pickup layer/tag, trigger SphereCollider + <see cref="LootPickup"/>
    /// (ödül türü yazılı); "Visual" altında kırmızı bidon / haki kutu. Kök yerdeki eşyanın merkezi (jetonlar gibi).</item>
    /// </list>
    /// </summary>
    public static class LootPrefabs
    {
        public const string Folder = "Assets/_Project/Prefabs/Loot";
        public const string CratePath = Folder + "/LootCrate.prefab";
        public const string FuelPickupPath = Folder + "/FuelPickup.prefab";
        public const string AmmoPickupPath = Folder + "/AmmoPickup.prefab";
        public const string BodyName = "Body";

        private const string RewardTypeField = "_rewardType";

        /// <summary>Sandık küpünün kenarı (m).</summary>
        public const float CrateSize = 0.9f;

        /// <summary>Toplanabilir eşyaların trigger yarıçapı (m).</summary>
        public const float PickupColliderRadius = 0.3f;

        /// <summary>Yakıt bidonu: dik, ince kutu (m).</summary>
        public static readonly Vector3 FuelBodyScale = new Vector3(0.36f, 0.46f, 0.2f);

        /// <summary>Mermi kutusu: yassı kutu (m).</summary>
        public static readonly Vector3 AmmoBodyScale = new Vector3(0.48f, 0.26f, 0.3f);

        public static LootCrate LoadOrCreateCrate()
        {
            return LoadOrCreate<LootCrate>(CratePath, () => BuildCrate(PlaceholderMaterials.GetOrCreate(PlaceholderMaterialId.LootChest)));
        }

        public static LootPickup LoadOrCreateFuelPickup()
        {
            return LoadOrCreate<LootPickup>(FuelPickupPath, () => BuildPickup(
                "FuelPickup", LootRewardType.Fuel, FuelBodyScale, PlaceholderMaterials.GetOrCreate(PlaceholderMaterialId.FuelPickup)));
        }

        public static LootPickup LoadOrCreateAmmoPickup()
        {
            return LoadOrCreate<LootPickup>(AmmoPickupPath, () => BuildPickup(
                "AmmoPickup", LootRewardType.AmmoPack, AmmoBodyScale, PlaceholderMaterials.GetOrCreate(PlaceholderMaterialId.AmmoPickup)));
        }

        /// <summary>Sandık hiyerarşisini sahnede (kaydetmeden) kurar. Çağıran yok eder; testler de kullanır (materyal null olabilir).</summary>
        public static GameObject BuildCrate(Material material)
        {
            var root = new GameObject("LootCrate") { layer = GameLayers.Interactable };
            var collider = root.AddComponent<BoxCollider>();
            collider.size = Vector3.one * CrateSize;
            collider.center = Vector3.up * (CrateSize * 0.5f);
            root.AddComponent<LootCrate>();

            Transform visual = CreateVisual(root, GameLayers.Interactable);
            GameObject body = PrimitiveFactory.Create(
                PrimitiveType.Cube, BodyName, visual, material, GameLayers.Interactable, withCollider: false);
            body.transform.localPosition = collider.center;
            body.transform.localScale = collider.size;
            return root;
        }

        /// <summary>Toplanabilir eşya hiyerarşisini sahnede (kaydetmeden) kurar. Çağıran yok eder; testler de kullanır.</summary>
        public static GameObject BuildPickup(string name, LootRewardType rewardType, Vector3 bodyScale, Material material)
        {
            var root = new GameObject(name) { layer = GameLayers.Pickup, tag = GameTags.Pickup };
            var collider = root.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = PickupColliderRadius;

            var pickup = root.AddComponent<LootPickup>();
            var serialized = new SerializedObject(pickup);
            serialized.FindProperty(RewardTypeField).intValue = (int)rewardType;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            Transform visual = CreateVisual(root, GameLayers.Pickup);
            GameObject body = PrimitiveFactory.Create(
                PrimitiveType.Cube, BodyName, visual, material, GameLayers.Pickup, withCollider: false);
            body.transform.localScale = bodyScale;
            return root;
        }

        private static Transform CreateVisual(GameObject root, int layer)
        {
            var visual = new GameObject(VisualRoot.Name) { layer = layer };
            visual.transform.SetParent(root.transform, false);
            return visual.transform;
        }

        private static T LoadOrCreate<T>(string path, Func<GameObject> build) where T : Component
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                GameObject root = build();
                try
                {
                    EditorAssetUtility.EnsureFolder(Folder);
                    prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
                    Debug.Log($"[Last Haven] Loot: {path} oluşturuldu.");
                }
                finally
                {
                    Object.DestroyImmediate(root);
                }
            }

            if (!prefab.TryGetComponent(out T component))
            {
                Debug.LogError($"[Last Haven] Loot: {path} kökünde {typeof(T).Name} yok; prefab'ı sil ve Generate Prefabs çalıştır.", prefab);
            }

            return component;
        }
    }
}
