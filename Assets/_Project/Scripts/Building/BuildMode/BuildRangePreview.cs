using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Building
{
    /// <summary>
    /// İnşa modunda menzil dairesi (GDD §11 taret): seçili yapının önizlemesinde ve üzerine gelinen yapıda, seviyesinde menzil
    /// statı varsa (<see cref="TurretStats.TryGetRange"/>, run çarpanları dahil) zeminde bir halka gösterir. Halka bir
    /// LineRenderer'dır (yerel uzayda, kök X'te 90° yatırılmış, TransformZ hizalı); noktalar sadece yarıçap değişince yazılır.
    /// </summary>
    public sealed class BuildRangePreview
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        private readonly LineRenderer _line;
        private readonly IRunModifiers _modifiers;
        private readonly Vector3[] _points;
        private float _radius = -1f;

        /// <param name="line">Halka (sahnede, başta kapalı olabilir). Null ise önizleme yok.</param>
        /// <param name="modifiers">Run çarpanları; null ise taban menzil.</param>
        public BuildRangePreview(LineRenderer line, IRunModifiers modifiers, Color color, float width, int segments)
        {
            _line = line;
            _modifiers = modifiers;
            _points = new Vector3[Mathf.Max(8, segments)];
            if (_line == null)
            {
                return;
            }

            _line.useWorldSpace = false;
            _line.loop = true;
            _line.alignment = LineAlignment.TransformZ;
            _line.widthMultiplier = width;
            _line.positionCount = 0;
            var block = new MaterialPropertyBlock();
            block.SetColor(BaseColorId, color);
            _line.SetPropertyBlock(block);
            _line.startColor = color;
            _line.endColor = color;
            Hide();
        }

        /// <summary>Halka şu an görünüyor mu.</summary>
        public bool IsVisible => _line != null && _line.gameObject.activeSelf;

        /// <summary>Gösterilen yarıçap (m); hiç gösterilmediyse -1.</summary>
        public float Radius => _radius;

        /// <summary>Seviyede menzil varsa halkayı <paramref name="center"/>'da gösterir; yoksa gizler. Gösterdiyse true.</summary>
        public bool Show(BuildableLevel level, Vector3 center)
        {
            if (_line == null || !TurretStats.TryGetRange(level, _modifiers, out float range) || range <= 0f)
            {
                Hide();
                return false;
            }

            if (!Mathf.Approximately(range, _radius))
            {
                _radius = range;
                FillCircle(_points, range);
                _line.positionCount = _points.Length;
                _line.SetPositions(_points);
            }

            Transform transform = _line.transform;
            transform.position = new Vector3(center.x, transform.position.y, center.z);
            if (!_line.gameObject.activeSelf)
            {
                _line.gameObject.SetActive(true);
            }

            return true;
        }

        public void Hide()
        {
            // Sahne kapanırken halka kontrolcüden önce yok edilmiş olabilir.
            if (_line != null && _line.gameObject.activeSelf)
            {
                _line.gameObject.SetActive(false);
            }
        }

        /// <summary>Yerel XY düzleminde (kök X'te 90° yatırılınca dünya XZ) eşit aralıklı daire noktaları.</summary>
        public static void FillCircle(Vector3[] points, float radius)
        {
            for (int i = 0; i < points.Length; i++)
            {
                float angle = i * Mathf.PI * 2f / points.Length;
                points[i] = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
            }
        }
    }
}
