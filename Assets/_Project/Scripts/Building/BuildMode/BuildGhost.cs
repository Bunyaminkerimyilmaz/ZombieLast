using System.Collections.Generic;
using LastHaven.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace LastHaven.Building
{
    /// <summary>
    /// Yapının yarı saydam kopyası (GDD §11): "Visual" kopyası + (varsa) zemindeki ayak izi plakası, tek renkle
    /// (<c>_BaseColor</c>, MaterialPropertyBlock). İnşa önizlemesi (yeşil/kırmızı) ve var olan yapının vurgusu (biraz büyütülmüş
    /// kabuk) bunu kullanır. Kopyalar yapı tipi başına bir kez, <see cref="Prepare"/>'de (oyun başında) oluşturulur; sonra sadece
    /// açılıp kapanır.
    /// </summary>
    /// <remarks>
    /// Kopyada collider ve Behaviour'lar (Animator, ışık, script) kapatılır, gölge yoktur, layer "Ignore Raycast"tır.
    /// Kök (sahne objesi) kapalıyken kopyalanır; kopyadaki script'lerin Awake'i önizleme ilk açılınca çalışır.
    /// </remarks>
    public sealed class BuildGhost
    {
        private const int IgnoreRaycastLayer = 2; // Unity'nin yerleşik layer'ı.

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        private readonly Transform _root;
        private readonly Renderer _footprint;
        private readonly Material _material;
        private readonly float _cellSize;
        private readonly float _visualScale;
        private readonly Dictionary<BuildableData, Renderer[]> _renderers = new Dictionary<BuildableData, Renderer[]>();
        private readonly Dictionary<BuildableData, GameObject> _visuals = new Dictionary<BuildableData, GameObject>();
        private readonly MaterialPropertyBlock _block = new MaterialPropertyBlock();
        private BuildableData _current;
        private Color _color;
        private bool _hasColor;

        /// <param name="root">Kopyanın kökü (sahnede, başta kapalı). Gösterilen noktaya taşınır.</param>
        /// <param name="footprint">Kökün çocuğu, yatay yatırılmış 1×1 quad; ayak izine ölçeklenir. Boş olabilir.</param>
        /// <param name="material">Yarı saydam materyal (<c>_BaseColor</c> ile renklenir). Null ise kopyalar kendi materyalinde kalır.</param>
        /// <param name="visualScale">Kopyanın pivot etrafındaki ölçeği (vurgu kabuğu için 1'den biraz büyük).</param>
        public BuildGhost(Transform root, Renderer footprint, Material material, float cellSize, float visualScale = 1f)
        {
            _root = root;
            _footprint = footprint;
            _material = material;
            _cellSize = cellSize;
            _visualScale = visualScale;
            _root.gameObject.SetActive(false);
        }

        /// <summary>Yapı tipinin önizleme kopyasını (yoksa) oluşturur. Oyun başında her yuva için çağrılır.</summary>
        public void Prepare(BuildableData data)
        {
            if (data == null || _visuals.ContainsKey(data))
            {
                return;
            }

            var container = new GameObject(string.IsNullOrEmpty(data.Id) ? data.name : data.Id) { layer = IgnoreRaycastLayer };
            container.SetActive(false);
            container.transform.SetParent(_root, false);

            Transform visual = data.Prefab != null ? VisualRoot.Find(data.Prefab.transform) : null;
            if (visual != null)
            {
                container.transform.localScale = data.Prefab.transform.localScale * _visualScale;
                GameObject copy = Object.Instantiate(visual.gameObject, container.transform, false);
                copy.transform.SetLocalPositionAndRotation(visual.localPosition, visual.localRotation);
                copy.transform.localScale = visual.localScale;
                StripToVisual(copy);
            }
            else
            {
                Debug.LogWarning($"BuildGhost: '{data.name}' prefab'ında '{VisualRoot.Name}' yok; önizlemede sadece ayak izi görünecek.", data);
            }

            _visuals.Add(data, container);
            _renderers.Add(data, container.GetComponentsInChildren<Renderer>(true));
        }

        /// <summary>Kopyayı noktaya taşır, gerekirse yapı tipini değiştirir ve rengini ayarlar.</summary>
        /// <param name="position">Yapının pivotu (ayak izi merkezi, yer seviyesi).</param>
        public void Show(BuildableData data, Vector3 position, Quaternion rotation, Color color)
        {
            if (data != _current)
            {
                SwitchTo(data);
            }

            _root.SetPositionAndRotation(position, rotation);
            SetColor(color);
            if (!_root.gameObject.activeSelf)
            {
                _root.gameObject.SetActive(true);
            }
        }

        public void Hide()
        {
            // Sahne kapanırken kök kontrolcüden önce yok edilmiş olabilir.
            if (_root != null && _root.gameObject.activeSelf)
            {
                _root.gameObject.SetActive(false);
            }
        }

        private void SwitchTo(BuildableData data)
        {
            if (_current != null && _visuals.TryGetValue(_current, out GameObject previous))
            {
                previous.SetActive(false);
            }

            Prepare(data);
            _current = data;
            _visuals[data].SetActive(true);
            _hasColor = false;

            if (_footprint != null)
            {
                // Plaka X'te 90° yatırılmış quad: yerel X → dünya X, yerel Y → dünya Z (dönmemiş ayak izi).
                Vector2Int size = data.Footprint;
                _footprint.transform.localScale = new Vector3(size.x * _cellSize, size.y * _cellSize, 1f);
            }
        }

        private void SetColor(Color color)
        {
            if (_hasColor && color == _color)
            {
                return;
            }

            _hasColor = true;
            _color = color;
            _block.SetColor(BaseColorId, color);
            Renderer[] renderers = _renderers[_current];
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].SetPropertyBlock(_block);
            }

            if (_footprint != null)
            {
                _footprint.SetPropertyBlock(_block);
            }
        }

        private void StripToVisual(GameObject copy)
        {
            foreach (Transform child in copy.GetComponentsInChildren<Transform>(true))
            {
                child.gameObject.layer = IgnoreRaycastLayer;
            }

            foreach (Collider collider in copy.GetComponentsInChildren<Collider>(true))
            {
                collider.enabled = false;
            }

            foreach (Behaviour behaviour in copy.GetComponentsInChildren<Behaviour>(true))
            {
                behaviour.enabled = false;
            }

            foreach (Renderer renderer in copy.GetComponentsInChildren<Renderer>(true))
            {
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                if (_material != null)
                {
                    var materials = new Material[renderer.sharedMaterials.Length];
                    for (int i = 0; i < materials.Length; i++)
                    {
                        materials[i] = _material;
                    }

                    renderer.sharedMaterials = materials;
                }
            }
        }
    }
}
