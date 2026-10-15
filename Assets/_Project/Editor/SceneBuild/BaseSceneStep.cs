using LastHaven.Building;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace LastHaven.Editor
{
    /// <summary>
    /// [Gameplay]/Base: harita merkezinde üs. Kökte <see cref="BaseArea"/> (yarıçap <see cref="BaseAreaConfig"/>'ten);
    /// altında zemindeki üs dairesi (sadece görsel) ve merkezde Generator prefab'ının bir örneği.
    /// </summary>
    public sealed class BaseSceneStep : ISceneBuildStep
    {
        public const string BaseName = "Base";
        public const string AreaDiskName = "AreaDisk";

        private const float CylinderPrimitiveHeight = 2f;
        private const float AreaDiskThickness = 0.02f;

        public int Order => 250;

        public void Build(SceneBuildContext context)
        {
            BaseAreaConfig areaConfig = BaseGenerator.LoadOrCreateAreaConfig();
            Generator generatorPrefab = BaseGenerator.LoadOrCreateGeneratorPrefab();

            // Üs harita merkezinde (MapLayoutConfig engel yerleşiminde merkezi boş bırakır).
            GameObject baseObject = context.CreateObject(BaseName, context.Gameplay);
            var area = baseObject.AddComponent<BaseArea>();
            var serialized = new SerializedObject(area);
            serialized.FindProperty("_config").objectReferenceValue = areaConfig;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            BuildAreaDisk(context, baseObject.transform, areaConfig.Radius);

            var generator = (GameObject)PrefabUtility.InstantiatePrefab(generatorPrefab.gameObject, context.Scene);
            generator.transform.SetParent(baseObject.transform, false);
            generator.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        private static void BuildAreaDisk(SceneBuildContext context, Transform parent, float radius)
        {
            // Sadece görsel: collider yok, Default layer'da (NavMesh bake'ine girmez), gölge düşürmez.
            GameObject disk = context.CreatePrimitive(
                PrimitiveType.Cylinder, AreaDiskName, parent, PlaceholderMaterialId.BaseArea, withCollider: false);
            float diameter = radius * 2f;
            disk.transform.localPosition = new Vector3(0f, AreaDiskThickness * 0.5f, 0f);
            disk.transform.localScale = new Vector3(diameter, AreaDiskThickness / CylinderPrimitiveHeight, diameter);
            disk.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
        }
    }
}
