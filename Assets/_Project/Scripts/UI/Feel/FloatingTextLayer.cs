using UnityEngine;
using UnityEngine.UIElements;

namespace LastHaven.UI
{
    /// <summary>
    /// Uçan yazı katmanının etiketleri (<see cref="HudFloatingTextController"/> kullanır): açılışta sabit sayıda <see cref="Label"/>
    /// oluşturur, yuvaları <see cref="FloatingTextSlots"/> ile yönetir ve her kare etkin yazıları dünya noktalarından ekrana
    /// yansıtır (yükselme, boy, opaklık <see cref="FloatingTextMotion"/>'dan). Etiketin left/top'u yazının merkezi (USS translate).
    /// </summary>
    public sealed class FloatingTextLayer
    {
        private readonly VisualElement _layer;
        private readonly FloatingTextSlots _slots;
        private readonly FloatingTextMotion _motion;
        private readonly float _riseHeight;
        private readonly Label[] _labels;
        private readonly Vector3[] _anchors;
        private readonly float[] _sizes;

        /// <param name="classes">Her etikete eklenen USS sınıfları.</param>
        public FloatingTextLayer(VisualElement layer, int size, FloatingTextMotion motion, float riseHeight, params string[] classes)
        {
            _layer = layer;
            _slots = new FloatingTextSlots(size);
            _motion = motion;
            _riseHeight = riseHeight;
            _labels = new Label[size];
            _anchors = new Vector3[size];
            _sizes = new float[size];

            _layer.Clear();
            for (int i = 0; i < size; i++)
            {
                var label = new Label { pickingMode = PickingMode.Ignore };
                foreach (string className in classes)
                {
                    label.AddToClassList(className);
                }

                label.SetShown(false);
                _layer.Add(label);
                _labels[i] = label;
            }
        }

        public bool IsActive(int slot)
        {
            return _slots.IsActive(slot);
        }

        /// <summary>Yazının yaşı (sn).</summary>
        public float Age(int slot)
        {
            return _slots.Age(slot);
        }

        /// <summary>Yuva alır (boş, yoksa en eskisi), boyu 1 yapar. Görünür olması bir sonraki <see cref="Tick"/>'te.</summary>
        public int Acquire(float lifetime)
        {
            int slot = _slots.Acquire(lifetime);
            _sizes[slot] = 1f;
            return slot;
        }

        /// <summary>Yazıyı baştan başlatır (yeniden "vurur").</summary>
        public void Restart(int slot, float lifetime)
        {
            _slots.Restart(slot, lifetime);
        }

        public Label GetLabel(int slot)
        {
            return _labels[slot];
        }

        /// <summary>Yazının başladığı dünya noktası (yükselme bunun üstüne eklenir).</summary>
        public void SetAnchor(int slot, Vector3 worldPosition)
        {
            _anchors[slot] = worldPosition;
        }

        /// <summary>Boy katsayısı (hasar büyüklüğü); vuruş büyümesiyle çarpılır.</summary>
        public void SetSize(int slot, float size)
        {
            _sizes[slot] = size;
        }

        /// <summary>Yaşları ilerletir ve etkin yazıları yerleştirir; biten, kameranın arkasındaki ya da kamerasız yazılar gizlenir.</summary>
        public void Tick(float deltaTime, Camera camera)
        {
            _slots.Tick(deltaTime);
            for (int i = 0; i < _labels.Length; i++)
            {
                if (!_slots.IsActive(i) || !Place(i, camera))
                {
                    _labels[i].SetShown(false);
                }
            }
        }

        private bool Place(int slot, Camera camera)
        {
            FloatingTextPose pose = _motion.Evaluate(_slots.Progress(slot));
            Vector3 world = _anchors[slot] + Vector3.up * (pose.Rise * _riseHeight);
            if (camera == null || _layer.panel == null || camera.WorldToViewportPoint(world).z <= 0f)
            {
                return false;
            }

            Vector2 point = _layer.WorldToLocal(RuntimePanelUtils.CameraTransformWorldToPanel(_layer.panel, world, camera));
            Label label = _labels[slot];
            label.style.left = point.x;
            label.style.top = point.y;
            label.style.opacity = pose.Alpha;
            float scale = pose.Scale * _sizes[slot];
            label.style.scale = new Scale(new Vector2(scale, scale));
            label.SetShown(true);
            return true;
        }
    }
}
