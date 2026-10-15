using LastHaven.Core;
using LastHaven.Feedback;
using UnityEngine;

namespace LastHaven.Editor
{
    /// <summary>
    /// Yapı yıkılma efekti prefab'ının kurulumu (<c>Prefabs/Feedback/Debris.prefab</c>, <see cref="FeedbackPrefabs"/> kaydeder):
    /// küp mesh parçacıklar (<see cref="FxMaterials.GetOrCreateDebris"/>), kahve-gri, kutudan yukarı saçılır, dönerek düşer ve
    /// zemine çarpıp seker; ömrün sonunda küçülür. Değerler GDD'de yok; kısa ve okunaklı olsun diye seçildi.
    /// </summary>
    public static class DebrisPrefab
    {
        private const float Lifetime = 1.3f; // En uzun parça ömrünü kapsar.
        private const float ParticleMinLifetime = 0.7f;
        private const float ParticleMaxLifetime = 1.1f;
        private const float MinSpeed = 2.5f;
        private const float MaxSpeed = 5.5f;
        private const float MinSize = 0.12f;
        private const float MaxSize = 0.3f;
        private const float Gravity = 1.6f;
        private const int MaxParticles = 64;
        private const float SpreadAmount = 0.35f; // 0 = dümdüz yukarı, 1 = rastgele yön.
        private const float SpinRadiansPerSecond = 6f;
        private const float ShrinkStart = 0.75f; // Ömrün bu oranından sonra küçülür.
        private const float Bounce = 0.3f;
        private const float Dampen = 0.5f;

        private static readonly Color Brown = new Color(0.42f, 0.3f, 0.2f);
        private static readonly Color Gray = new Color(0.58f, 0.57f, 0.54f);

        public static GameObject Build()
        {
            var root = new GameObject("Debris");
            var particles = root.AddComponent<ParticleSystem>();
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = particles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(ParticleMinLifetime, ParticleMaxLifetime);
            main.startSpeed = new ParticleSystem.MinMaxCurve(MinSpeed, MaxSpeed);
            main.startSize = new ParticleSystem.MinMaxCurve(MinSize, MaxSize);
            main.startRotation3D = true;
            main.startRotationX = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startRotationY = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startRotationZ = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = new ParticleSystem.MinMaxGradient(Brown, Gray);
            main.gravityModifier = Gravity;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = MaxParticles;

            // Parçalar DebrisEffect.Play'de Emit ile bir kerede çıkar; sürekli yayım yok.
            ParticleSystem.EmissionModule emission = particles.emission;
            emission.enabled = false;

            // Kutu X'te -90° döndürülür: +Z (yayım yönü) yukarı bakar. Boyutu Play'de yapının kutusundan.
            ParticleSystem.ShapeModule shape = particles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.rotation = new Vector3(-90f, 0f, 0f);
            shape.randomDirectionAmount = SpreadAmount;

            ParticleSystem.RotationOverLifetimeModule spin = particles.rotationOverLifetime;
            spin.enabled = true;
            spin.separateAxes = true;
            spin.x = new ParticleSystem.MinMaxCurve(-SpinRadiansPerSecond, SpinRadiansPerSecond);
            spin.y = new ParticleSystem.MinMaxCurve(-SpinRadiansPerSecond, SpinRadiansPerSecond);
            spin.z = new ParticleSystem.MinMaxCurve(-SpinRadiansPerSecond, SpinRadiansPerSecond);

            ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = particles.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(
                1f, new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(ShrinkStart, 1f), new Keyframe(1f, 0f)));

            ParticleSystem.CollisionModule collision = particles.collision;
            collision.enabled = true;
            collision.type = ParticleSystemCollisionType.World;
            collision.mode = ParticleSystemCollisionMode.Collision3D;
            collision.collidesWith = GameLayers.GroundMask;
            collision.bounce = Bounce;
            collision.dampen = Dampen;
            collision.quality = ParticleSystemCollisionQuality.Medium;

            var renderer = root.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Mesh;
            renderer.mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            renderer.alignment = ParticleSystemRenderSpace.World;
            renderer.sharedMaterial = FxMaterials.GetOrCreateDebris();
            FeedbackPrefabs.DisableShadowsAndProbes(renderer);

            FeedbackPrefabs.SetLifetime(root.AddComponent<PooledLifetime>(), Lifetime);
            root.AddComponent<DebrisEffect>();
            return root;
        }
    }
}
