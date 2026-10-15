using System;
using LastHaven.Core;
using LastHaven.Feedback;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace LastHaven.Editor
{
    /// <summary>
    /// Efekt prefab'ları (<c>Prefabs/Feedback/</c>): Tracer, Impact, MuzzleFlash (atış), Debris (yapı yıkılma; ayarları
    /// <see cref="DebrisPrefab"/>'ta). Varsa dokunulmaz; yoksa oluşturulur. Efekt prefab'ları tek katmandır (kök = görsel);
    /// mantık sadece görseli sürer, değiştirilecek model yoktur.
    /// </summary>
    public static class FeedbackPrefabs
    {
        public const string Folder = "Assets/_Project/Prefabs/Feedback";
        public const string TracerPath = Folder + "/Tracer.prefab";
        public const string ImpactPath = Folder + "/Impact.prefab";
        public const string MuzzleFlashPath = Folder + "/MuzzleFlash.prefab";
        public const string DebrisPath = Folder + "/Debris.prefab";

        private const string LifetimeFieldName = "_lifetime";

        // Ömürler (sn): iz ~0.05 (görev tanımı); namlu ışığı da aynı kısalıkta; vuruş en uzun parçacık ömrünü kapsar.
        private const float TracerLifetime = 0.05f;
        private const float MuzzleFlashLifetime = 0.05f;
        private const float ImpactLifetime = 0.6f;

        // Vuruş parçacıkları: türden bağımsız ortak ayarlar (renk, hız, boyut, yerçekimi ImpactProfile'da).
        private const float ParticleMinLifetime = 0.15f;
        private const float ParticleMaxLifetime = 0.4f;
        private const int MaxParticles = 64;
        private const float ConeAngle = 35f;
        private const float ConeRadius = 0.02f;
        private const float StretchVelocityScale = 0.04f;
        private const float StretchLengthScale = 1f;

        // Namlu ışığı: sıcak sarı, kısa menzil.
        private const float MuzzleLightRange = 5f;
        private static readonly Color MuzzleLightColor = new Color(1f, 0.77f, 0.42f);

        public static Tracer LoadOrCreateTracer()
        {
            return LoadOrCreate<Tracer>(TracerPath, BuildTracer);
        }

        public static ImpactEffect LoadOrCreateImpact()
        {
            return LoadOrCreate<ImpactEffect>(ImpactPath, BuildImpact);
        }

        public static MuzzleFlash LoadOrCreateMuzzleFlash()
        {
            return LoadOrCreate<MuzzleFlash>(MuzzleFlashPath, BuildMuzzleFlash);
        }

        public static DebrisEffect LoadOrCreateDebris()
        {
            return LoadOrCreate<DebrisEffect>(DebrisPath, DebrisPrefab.Build);
        }

        /// <summary>Efektin kendini havuza iade süresini yazar.</summary>
        internal static void SetLifetime(PooledLifetime lifetime, float seconds)
        {
            var serialized = new SerializedObject(lifetime);
            serialized.FindProperty(LifetimeFieldName).floatValue = seconds;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        internal static void DisableShadowsAndProbes(Renderer renderer)
        {
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
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
                    Debug.Log($"[Last Haven] Feedback: {path} oluşturuldu.");
                }
                finally
                {
                    Object.DestroyImmediate(root);
                }
            }

            if (!prefab.TryGetComponent(out T component))
            {
                Debug.LogError($"[Last Haven] Feedback: {path} kökünde {typeof(T).Name} yok; prefab'ı sil ve Generate Prefabs çalıştır.", prefab);
            }

            return component;
        }

        private static GameObject BuildTracer()
        {
            var root = new GameObject("Tracer");
            var line = root.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.positionCount = 2;
            line.SetPosition(0, Vector3.zero);
            line.SetPosition(1, Vector3.forward);
            line.alignment = LineAlignment.View;
            line.textureMode = LineTextureMode.Stretch;
            line.numCapVertices = 2;
            line.sharedMaterial = FxMaterials.GetOrCreateAdditive();
            DisableShadowsAndProbes(line);

            SetLifetime(root.AddComponent<PooledLifetime>(), TracerLifetime);
            root.AddComponent<Tracer>();
            return root;
        }

        private static GameObject BuildImpact()
        {
            var root = new GameObject("Impact");
            var particles = root.AddComponent<ParticleSystem>();
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = particles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(ParticleMinLifetime, ParticleMaxLifetime);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = MaxParticles;

            // Parçacıklar ImpactEffect.Play'de Emit ile bir kerede çıkar; sürekli yayım yok.
            ParticleSystem.EmissionModule emission = particles.emission;
            emission.enabled = false;

            ParticleSystem.ShapeModule shape = particles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = ConeAngle;
            shape.radius = ConeRadius;

            ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = particles.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

            var renderer = root.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.velocityScale = StretchVelocityScale;
            renderer.lengthScale = StretchLengthScale;
            renderer.sharedMaterial = FxMaterials.GetOrCreateAdditive();
            DisableShadowsAndProbes(renderer);

            SetLifetime(root.AddComponent<PooledLifetime>(), ImpactLifetime);
            var impact = root.AddComponent<ImpactEffect>();
            var serialized = new SerializedObject(impact);
            serialized.FindProperty("_flesh._material").objectReferenceValue = FxMaterials.GetOrCreateAlpha();
            serialized.FindProperty("_spark._material").objectReferenceValue = FxMaterials.GetOrCreateAdditive();
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return root;
        }

        private static GameObject BuildMuzzleFlash()
        {
            var root = new GameObject("MuzzleFlash");
            var light = root.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = MuzzleLightRange;
            light.color = MuzzleLightColor;
            light.intensity = 0f;
            light.shadows = LightShadows.None;
            light.lightmapBakeType = LightmapBakeType.Realtime;

            SetLifetime(root.AddComponent<PooledLifetime>(), MuzzleFlashLifetime);
            root.AddComponent<MuzzleFlash>();
            return root;
        }
    }
}
