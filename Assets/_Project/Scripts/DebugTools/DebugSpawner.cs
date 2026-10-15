using LastHaven.Core;
using LastHaven.Enemies;
using LastHaven.Waves;
using UnityEngine;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine.InputSystem;
#endif

namespace LastHaven.DebugTools
{
    /// <summary>
    /// Geliştirme kısayolları ([Systems]/DebugSpawner): F1 imleçteki noktaya Yürüyen, F2 Koşucu, F3 Tank; F4 rastgele bir spawn
    /// bölgesinden 20 Yürüyen. Sol üstte kısayol listesi (OnGUI). Düşmanlar <see cref="IEnemySpawner"/> ile id üzerinden doğar.
    /// Sadece editörde ve development build'de çalışır; sürüm build'inde bileşen boş kalır (sahnede "missing script" olmasın diye
    /// sınıf ve alanları her zaman var).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DebugSpawner : MonoBehaviour
    {
        [Header("İmleçteki noktaya (F1–F3)")]
        [SerializeField, Tooltip("F1")]
        private EnemyData _walker;

        [SerializeField, Tooltip("F2")]
        private EnemyData _runner;

        [SerializeField, Tooltip("F3")]
        private EnemyData _tank;

        [Header("Rastgele kenardan (F4)")]
        [SerializeField, Tooltip("F4'te doğan tip.")]
        private EnemyData _edgeEnemy;

        [SerializeField, Min(1), Tooltip("F4'te doğan düşman sayısı.")]
        private int _edgeCount = 20;

        [SerializeField, Tooltip("F4 bunlardan birini rastgele seçer; düşmanlar bölgenin dairesi içinde rastgele noktalarda doğar.")]
        private SpawnZone[] _edgeZones = new SpawnZone[0];

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private const string Prefix = "[Debug] ";
        private const float CursorRayDistance = 500f;

        private static readonly Rect PanelRect = new Rect(10f, 10f, 290f, 104f);
        private static readonly Rect PanelTextRect = new Rect(18f, 14f, 274f, 96f);

        private InputAction[] _actions;
        private IEnemySpawner _spawner;
        private ICameraService _camera;
        private string _panelText;

        private void Awake()
        {
            // Oyun girdisi değil, geliştirici kısayolu: IInputReader'a eklenmedi.
            _actions = new[]
            {
                CreateAction("<Keyboard>/f1", () => SpawnAtCursor(_walker)),
                CreateAction("<Keyboard>/f2", () => SpawnAtCursor(_runner)),
                CreateAction("<Keyboard>/f3", () => SpawnAtCursor(_tank)),
                CreateAction("<Keyboard>/f4", SpawnAtRandomEdge),
            };

            _panelText = "Debug Spawner\n"
                         + "F1   " + NameOf(_walker) + " (imleç)\n"
                         + "F2   " + NameOf(_runner) + " (imleç)\n"
                         + "F3   " + NameOf(_tank) + " (imleç)\n"
                         + "F4   " + _edgeCount + " × " + NameOf(_edgeEnemy) + " (rastgele kenar)";
        }

        private void Start()
        {
            if (!ServiceLocator.TryGet(out _spawner))
            {
                Debug.LogWarning("DebugSpawner: IEnemySpawner bulunamadı; kısayollar düşman doğuramayacak. Sahnede [Systems]/EnemySpawner var mı?", this);
            }

            ServiceLocator.TryGet(out _camera);
        }

        private void OnEnable()
        {
            for (int i = 0; i < _actions.Length; i++)
            {
                _actions[i].Enable();
            }
        }

        private void OnDisable()
        {
            for (int i = 0; i < _actions.Length; i++)
            {
                _actions[i].Disable();
            }
        }

        private void OnDestroy()
        {
            for (int i = 0; i < _actions.Length; i++)
            {
                _actions[i].Dispose();
            }
        }

        private void OnGUI()
        {
            GUI.Box(PanelRect, GUIContent.none);
            GUI.Label(PanelTextRect, _panelText);
        }

        private static InputAction CreateAction(string binding, System.Action onPressed)
        {
            var action = new InputAction(binding, InputActionType.Button, binding);
            action.performed += _ => onPressed();
            return action;
        }

        private static string NameOf(EnemyData data)
        {
            return data != null ? data.DisplayName : "—";
        }

        private void SpawnAtCursor(EnemyData data)
        {
            if (!TryGetCursorPoint(out Vector3 point))
            {
                Debug.LogWarning(Prefix + "İmleç dünyada bir noktaya denk gelmiyor (kamera yok mu?).", this);
                return;
            }

            if (Spawn(data, point) != null)
            {
                Debug.Log($"{Prefix}{data.DisplayName} doğdu ({point.x:0.#}, {point.z:0.#})", this);
            }
        }

        private void SpawnAtRandomEdge()
        {
            if (_edgeZones.Length == 0)
            {
                Debug.LogWarning(Prefix + "Spawn bölgesi atanmamış; Tools/Last Haven/Build Game Scene çalıştır.", this);
                return;
            }

            SpawnZone zone = _edgeZones[Random.Range(0, _edgeZones.Length)];
            if (zone == null)
            {
                return;
            }

            int spawned = 0;
            for (int i = 0; i < _edgeCount; i++)
            {
                Vector2 offset = Random.insideUnitCircle * zone.Radius;
                if (Spawn(_edgeEnemy, zone.Center + new Vector3(offset.x, 0f, offset.y)) != null)
                {
                    spawned++;
                }
            }

            Debug.Log($"{Prefix}{spawned} × {NameOf(_edgeEnemy)} doğdu ({zone.Direction} bölgesi)", this);
        }

        private GameObject Spawn(EnemyData data, Vector3 point)
        {
            if (data == null)
            {
                Debug.LogWarning(Prefix + "Bu kısayolun EnemyData'sı boş; Tools/Last Haven/Build Game Scene çalıştır.", this);
                return null;
            }

            return _spawner?.Spawn(data.Id, point);
        }

        /// <summary>İmlecin altındaki zemin/engel noktası; ikisine de değmiyorsa y = 0 düzlemi.</summary>
        private bool TryGetCursorPoint(out Vector3 point)
        {
            Mouse mouse = Mouse.current;
            Camera view = _camera?.Camera;
            if (mouse == null || view == null)
            {
                point = default;
                return false;
            }

            Ray ray = view.ScreenPointToRay(mouse.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, CursorRayDistance, GameLayers.NavMeshBakeMask, QueryTriggerInteraction.Ignore))
            {
                point = hit.point;
                return true;
            }

            var ground = new Plane(Vector3.up, Vector3.zero);
            if (ground.Raycast(ray, out float enter))
            {
                point = ray.GetPoint(enter);
                return true;
            }

            point = default;
            return false;
        }
#endif
    }
}
