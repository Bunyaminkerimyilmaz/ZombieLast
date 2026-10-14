using System.Diagnostics;
using LastHaven.Core;
using Unity.AI.Navigation;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace LastHaven.Enemies
{
    /// <summary>
    /// Aynı objedeki <see cref="NavMeshSurface"/>'i oyun başında, diğer tüm scriptlerden önce bake eder.
    /// Editörde NavMesh asset'i tutulmaz; her Play'de zemin ve engellerden yeniden üretilir.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NavMeshSurface))]
    public sealed class NavMeshBaker : MonoBehaviour
    {
        private NavMeshSurface _surface;

        /// <summary>
        /// Yüzeyi proje sabitlerine göre ayarlar: tüm objeler, Physics Colliders, sadece Ground ve Obstacle layer'ları.
        /// </summary>
        public static void ConfigureSurface(NavMeshSurface surface)
        {
            surface.collectObjects = CollectObjects.All;
            surface.useGeometry = UnityEngine.AI.NavMeshCollectGeometry.PhysicsColliders;
            surface.layerMask = GameLayers.NavMeshBakeMask;
        }

        private void Awake()
        {
            _surface = GetComponent<NavMeshSurface>();
            Bake();
        }

        private void Bake()
        {
            var stopwatch = Stopwatch.StartNew();
            _surface.BuildNavMesh();
            stopwatch.Stop();

            if (_surface.navMeshData == null)
            {
                Debug.LogError("[NavMeshBaker] NavMesh bake edilemedi: Ground/Obstacle layer'ında collider bulunamadı.", this);
                return;
            }

            Debug.Log($"[NavMeshBaker] NavMesh bake edildi: {stopwatch.Elapsed.TotalMilliseconds:F1} ms ({name}).", this);
        }

        private void Reset()
        {
            ConfigureSurface(GetComponent<NavMeshSurface>());
        }
    }
}
