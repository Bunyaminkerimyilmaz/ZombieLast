using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Waves
{
    /// <summary>
    /// Harita kenarındaki düşman doğma bölgesi. Merkez bu objenin konumu, alan <see cref="Radius"/> yarıçaplı yatay dairedir.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SpawnZone : MonoBehaviour
    {
        private const int GizmoSegments = 48;
        private const float GizmoFillAlpha = 0.15f;
        private const float GizmoFlatness = 0.01f;
        private const float GizmoMarkerRadius = 0.5f;

        private static readonly Color NorthColor = new Color(0.30f, 0.60f, 1.00f);
        private static readonly Color EastColor = new Color(1.00f, 0.80f, 0.20f);
        private static readonly Color SouthColor = new Color(0.30f, 0.90f, 0.40f);
        private static readonly Color WestColor = new Color(1.00f, 0.35f, 0.35f);

        // Sabit geometri, durum değil.
        private static readonly Vector3[] UnitCircle = CreateUnitCircle();

        [Header("Bölge")]
        [SerializeField, Tooltip("Bölgenin haritadaki yönü. Gece verisi saldırı yönlerini buna göre seçer.")]
        private CompassDirection _direction;

        [SerializeField, Min(0.5f), Tooltip("Düşmanların doğabileceği dairenin yarıçapı (m).")]
        private float _radius = 6f;

        public CompassDirection Direction => _direction;

        public float Radius => _radius;

        public Vector3 Center => transform.position;

        private void OnDrawGizmos()
        {
            Color color = GetGizmoColor(_direction);
            Matrix4x4 previousMatrix = Gizmos.matrix;

            // Yarı saydam dolgu: basık küre yatay disk gibi görünür.
            Gizmos.matrix = Matrix4x4.TRS(Center, Quaternion.identity, new Vector3(1f, GizmoFlatness, 1f));
            Gizmos.color = new Color(color.r, color.g, color.b, GizmoFillAlpha);
            Gizmos.DrawSphere(Vector3.zero, _radius);

            // Kenar çizgisi
            Gizmos.matrix = Matrix4x4.TRS(Center, Quaternion.identity, Vector3.one * _radius);
            Gizmos.color = color;
            Gizmos.DrawLineStrip(UnitCircle, true);

            Gizmos.matrix = previousMatrix;
            Gizmos.DrawSphere(Center, GizmoMarkerRadius);
            Gizmos.DrawLine(Center, new Vector3(0f, Center.y, 0f));
        }

        private static Color GetGizmoColor(CompassDirection direction)
        {
            return direction switch
            {
                CompassDirection.North => NorthColor,
                CompassDirection.East => EastColor,
                CompassDirection.South => SouthColor,
                _ => WestColor,
            };
        }

        private static Vector3[] CreateUnitCircle()
        {
            var points = new Vector3[GizmoSegments];
            for (int i = 0; i < GizmoSegments; i++)
            {
                float angle = i * Mathf.PI * 2f / GizmoSegments;
                points[i] = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            }

            return points;
        }
    }
}
