using LastHaven.Core;
using LastHaven.Economy;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// Jeton prefab'ı (<c>Prefabs/Economy/Coin.prefab</c>). Kök: Pickup layer ve tag, trigger SphereCollider, <see cref="Coin"/>.
    /// "Visual": dik duran altın yassı silindir (GDD §18, collider yok); kök dikey eksende döner. Varsa dokunulmaz.
    /// </summary>
    /// <remarks>
    /// Toplama mesafe ile yapılır (<see cref="CoinMotion"/>); collider trigger'dır ki oyuncunun CharacterController'ı jetona
    /// takılmasın. Pickup layer'ındaki sorgular (ileride) için durur.
    /// </remarks>
    public static class CoinPrefab
    {
        public const string Folder = "Assets/_Project/Prefabs/Economy";
        public const string PrefabPath = Folder + "/Coin.prefab";
        public const string RootName = "Coin";
        public const string BodyName = "Body";

        /// <summary>Trigger yarıçapı: jetonun yarıçapı kadar.</summary>
        public const float ColliderRadius = 0.2f;

        /// <summary>Silindir primitive'i 1 çaplı, 2 yüksekliğinde: 0.4 m çap, 0.06 m kalınlık.</summary>
        public static readonly Vector3 BodyScale = new Vector3(0.4f, 0.03f, 0.4f);

        /// <summary>Silindirin ekseni yataya (Z) yatırılır: jeton dik durur, kök dönünce yüzü döner.</summary>
        public static readonly Vector3 BodyRotation = new Vector3(90f, 0f, 0f);

        /// <summary>Prefab'ı yükler; yoksa oluşturur. Var olan prefab'a dokunulmaz.</summary>
        public static Coin LoadOrCreate()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
            {
                GameObject root = BuildRoot(PlaceholderMaterials.GetOrCreate(PlaceholderMaterialId.Coin));
                try
                {
                    EditorAssetUtility.EnsureFolder(Folder);
                    prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                    Debug.Log($"[Last Haven] Economy: {PrefabPath} oluşturuldu.");
                }
                finally
                {
                    Object.DestroyImmediate(root);
                }
            }

            if (!prefab.TryGetComponent(out Coin coin))
            {
                Debug.LogError($"[Last Haven] Economy: {PrefabPath} kökünde Coin yok; prefab'ı sil ve Generate Prefabs çalıştır.", prefab);
            }

            return coin;
        }

        /// <summary>
        /// Prefab hiyerarşisini sahnede (kaydetmeden) kurar. Çağıran yok etmekle sorumludur. Testler de kullanır
        /// (materyal null olabilir).
        /// </summary>
        public static GameObject BuildRoot(Material material)
        {
            var root = new GameObject(RootName) { layer = GameLayers.Pickup, tag = GameTags.Pickup };

            var collider = root.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = ColliderRadius;

            root.AddComponent<Coin>();

            var visual = new GameObject(VisualRoot.Name) { layer = GameLayers.Pickup };
            visual.transform.SetParent(root.transform, false);

            GameObject body = PrimitiveFactory.Create(
                PrimitiveType.Cylinder, BodyName, visual.transform, material, GameLayers.Pickup, withCollider: false);
            body.transform.localRotation = Quaternion.Euler(BodyRotation);
            body.transform.localScale = BodyScale;
            return root;
        }
    }
}
