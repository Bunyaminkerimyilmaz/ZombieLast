using UnityEngine;
using UnityEngine.UIElements;

namespace LastHaven.UI
{
    /// <summary>
    /// Bir HUD kutusunu dünyadaki bir nesnenin ekrandaki yerine taşır (yapı ipucu, etkileşim ipucu): kutu nesnenin üstünde, üstte
    /// yer yoksa altında, her durumda ekranın içinde (<see cref="AnchorPlacement"/>). Kutu ekran kökünün çocuğu ve
    /// <c>position: absolute</c> olmalı; boşluk ve ekran kenarı payı kutunun USS <c>margin-bottom</c>'ı (margin-left/top 0 kalmalı:
    /// mutlak konumu kaydırırlar). Kamera her kare hareket ettiği için sahibi her kare (kamera güncellendikten sonra) çağırır;
    /// stil sadece konum değişince yazılır.
    /// </summary>
    public sealed class WorldAnchor
    {
        private readonly VisualElement _element;
        private Vector2 _topLeft = new Vector2(float.NaN, float.NaN);

        public WorldAnchor(VisualElement element)
        {
            _element = element;
        }

        /// <summary>
        /// Kutuyu noktanın üstüne (yer yoksa altına) yerleştirir. Kamera yoksa, kutu panelde değilse veya nokta kameranın
        /// arkasındaysa false.
        /// </summary>
        public bool Place(Camera camera, Vector3 worldPosition)
        {
            if (!CanPlace(camera, worldPosition))
            {
                return false;
            }

            Vector2 point = ToParent(camera, worldPosition);
            Apply(point, point);
            return true;
        }

        /// <summary>
        /// Kutuyu nesnenin kutusunun tepe ortasının üstüne, yer yoksa ekranda en altta kalan taban köşesinin altına yerleştirir
        /// (kutu nesneyi örtmesin). Kamera yoksa, kutu panelde değilse veya tepe kameranın arkasındaysa false.
        /// </summary>
        public bool Place(Camera camera, Bounds bounds)
        {
            Vector3 center = bounds.center;
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;
            var worldTop = new Vector3(center.x, max.y, center.z);
            if (!CanPlace(camera, worldTop))
            {
                return false;
            }

            Vector2 top = ToParent(camera, worldTop);
            float lowest = float.MinValue;
            for (int corner = 0; corner < 4; corner++)
            {
                var worldCorner = new Vector3((corner & 1) == 0 ? min.x : max.x, min.y, (corner & 2) == 0 ? min.z : max.z);
                lowest = Mathf.Max(lowest, ToParent(camera, worldCorner).y);
            }

            Apply(top, new Vector2(top.x, lowest));
            return true;
        }

        private bool CanPlace(Camera camera, Vector3 worldPosition)
        {
            return camera != null && _element.parent != null && _element.panel != null
                   && camera.WorldToViewportPoint(worldPosition).z > 0f;
        }

        private void Apply(Vector2 top, Vector2 bottom)
        {
            float gap = _element.resolvedStyle.marginBottom;
            Vector2 topLeft = AnchorPlacement.AboveOrBelow(top, bottom, _element.layout.size, _element.parent.layout.size, gap, gap);
            if (topLeft != _topLeft)
            {
                _topLeft = topLeft;
                _element.style.left = topLeft.x;
                _element.style.top = topLeft.y;
            }
        }

        private Vector2 ToParent(Camera camera, Vector3 world)
        {
            return _element.parent.WorldToLocal(RuntimePanelUtils.CameraTransformWorldToPanel(_element.panel, world, camera));
        }
    }
}
