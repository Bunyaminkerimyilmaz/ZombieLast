using LastHaven.Core;
using LastHaven.Player;
using UnityEditor;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// PlayerData asset'i ve Player prefab'ı (GDD §5, §18).
    /// Kök: CharacterController + mantık bileşenleri, Player layer ve tag. "Visual": mavi kapsül + burun küpü (collider yok).
    /// "Muzzle": Visual dışında, nişan yüksekliğinde; WeaponController mermileri buradan atar.
    /// Varsa dokunulmaz; sadece eksik bileşen ve boş referanslar tamamlanır.
    /// </summary>
    public sealed class PlayerGenerator : IGenerationStep
    {
        public const string DataPath = "Assets/_Project/Data/Player/PlayerData.asset";
        public const string PrefabFolder = "Assets/_Project/Prefabs/Player";
        public const string PrefabPath = PrefabFolder + "/Player.prefab";
        public const string VisualName = "Visual";
        public const string MuzzleName = "Muzzle";

        private const string DataFieldName = "_data";
        private const string LoadoutFieldName = "_loadout";
        private const string MuzzleFieldName = "_muzzle";

        // Kapsül primitive'i 2 m boy, 0.5 m yarıçap; ayaklar kökün orijininde.
        private const float BodyHeight = 2f;
        private const float BodyRadius = 0.5f;
        private const float StepOffset = 0.3f;

        // Namlu gövde yüzeyinin biraz önünde; yüksekliği PlayerData.AimHeight'tan gelir.
        private const float MuzzleForward = 0.7f;

        private static readonly Vector3 NosePosition = new Vector3(0f, 1.5f, 0.45f);
        private static readonly Vector3 NoseScale = new Vector3(0.3f, 0.2f, 0.4f);

        public int Order => 200;

        public string Name => "Player";

        public static PlayerData LoadOrCreateData()
        {
            var data = EditorAssetUtility.LoadOrCreate<PlayerData>(DataPath, out bool created);
            if (created)
            {
                Debug.Log($"[Last Haven] Player: {DataPath} oluşturuldu.");
            }

            return data;
        }

        /// <summary>
        /// Prefab'ı yükler; yoksa oluşturur. Varsa sadece eksikleri tamamlar: boş PlayerData referansları,
        /// eksik <see cref="WeaponController"/> ve onun boş referansları. Kullanıcının diğer ayarlarına dokunulmaz.
        /// Sahne adımı buradan alır; Generate Prefabs çalışmamış olsa da prefab oluşur.
        /// </summary>
        public static GameObject LoadOrCreatePrefab()
        {
            PlayerData data = LoadOrCreateData();
            WeaponLoadoutData loadout = PlayerWeaponsGenerator.LoadOrCreateLoadout();
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
            {
                return CreatePrefab(data, loadout);
            }

            int changes = EditorAssetUtility.EditPrefab(
                PrefabPath, root => AssignData(root, data) + EnsureWeaponController(root, data, loadout));
            if (changes > 0)
            {
                Debug.Log($"[Last Haven] Player: {PrefabPath} içinde {changes} eksik bileşen/referans tamamlandı.");
            }

            return AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        }

        public void GenerateData()
        {
            LoadOrCreateData();
        }

        public void GeneratePrefabs()
        {
            LoadOrCreatePrefab();
        }

        private static GameObject CreatePrefab(PlayerData data, WeaponLoadoutData loadout)
        {
            var root = new GameObject("Player") { layer = GameLayers.Player, tag = GameTags.Player };
            try
            {
                var controller = root.AddComponent<CharacterController>();
                controller.height = BodyHeight;
                controller.radius = BodyRadius;
                // Controller skin width kadar havada durur; merkezi o kadar yukarı alınca görsel kapsül yere oturur.
                controller.center = new Vector3(0f, BodyHeight * 0.5f + controller.skinWidth, 0f);
                controller.stepOffset = StepOffset;

                root.AddComponent<Health>();
                root.AddComponent<PlayerMotor>();
                root.AddComponent<PlayerAim>();
                root.AddComponent<PlayerCharacter>();
                AssignData(root, data);

                BuildVisual(root.transform);
                EnsureWeaponController(root, data, loadout);

                EditorAssetUtility.EnsureFolder(PrefabFolder);
                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                Debug.Log($"[Last Haven] Player: {PrefabPath} oluşturuldu.");
                return prefab;
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static void BuildVisual(Transform root)
        {
            var visual = new GameObject(VisualName) { layer = GameLayers.Player };
            visual.transform.SetParent(root, false);
            Material material = PlaceholderMaterials.GetOrCreate(PlaceholderMaterialId.Player);

            GameObject body = PrimitiveFactory.Create(
                PrimitiveType.Capsule, "Body", visual.transform, material, GameLayers.Player, withCollider: false);
            body.transform.localPosition = new Vector3(0f, BodyHeight * 0.5f, 0f);

            GameObject nose = PrimitiveFactory.Create(
                PrimitiveType.Cube, "Nose", visual.transform, material, GameLayers.Player, withCollider: false);
            nose.transform.localPosition = NosePosition;
            nose.transform.localScale = NoseScale;
        }

        private static Transform BuildMuzzle(Transform root, PlayerData data)
        {
            var muzzle = new GameObject(MuzzleName) { layer = GameLayers.Player };
            muzzle.transform.SetParent(root, false);
            muzzle.transform.localPosition = new Vector3(0f, data.AimHeight, MuzzleForward);
            return muzzle.transform;
        }

        /// <summary>
        /// WeaponController yoksa ekler; Muzzle yoksa oluşturur; boş loadout ve namlu referanslarını doldurur.
        /// Değişiklik sayısını döndürür.
        /// </summary>
        private static int EnsureWeaponController(GameObject root, PlayerData data, WeaponLoadoutData loadout)
        {
            int changes = 0;
            if (!root.TryGetComponent(out WeaponController weapons))
            {
                weapons = root.AddComponent<WeaponController>();
                changes++;
            }

            Transform muzzle = root.transform.Find(MuzzleName);
            if (muzzle == null)
            {
                muzzle = BuildMuzzle(root.transform, data);
                changes++;
            }

            changes += EditorAssetUtility.AssignIfMissing(weapons, LoadoutFieldName, loadout) ? 1 : 0;
            changes += EditorAssetUtility.AssignIfMissing(weapons, MuzzleFieldName, muzzle) ? 1 : 0;
            return changes;
        }

        /// <summary>Kökteki oyuncu bileşenlerinin boş PlayerData alanlarını doldurur; doldurulan sayısını döndürür.</summary>
        private static int AssignData(GameObject root, PlayerData data)
        {
            return AssignDataTo(root.GetComponent<PlayerCharacter>(), data)
                   + AssignDataTo(root.GetComponent<PlayerMotor>(), data)
                   + AssignDataTo(root.GetComponent<PlayerAim>(), data);
        }

        private static int AssignDataTo(Component component, PlayerData data)
        {
            return component != null && EditorAssetUtility.AssignIfMissing(component, DataFieldName, data) ? 1 : 0;
        }
    }
}
