using UnityEngine;

namespace LastHaven.Core
{
    /// <summary><see cref="HitFlash"/>'ın kalıcı renk tonu katmanları. Alttan üste uygulanır.</summary>
    public enum TintLayer
    {
        /// <summary>Alt katman: uzun süreli görünüm değişimi (ör. boss öfkesinde koyulaşma).</summary>
        Base = 0,

        /// <summary>Üst katman: durum efekti (ör. yavaşlamış düşman mavi).</summary>
        Status = 1,
    }

    /// <summary>Katman başına bir ton (saf). Bileşik renk: materyalin rengi önce alt, sonra üst tona karışır (<see cref="HitFlash.Tint"/>).</summary>
    internal sealed class TintLayers
    {
        private const int LayerCount = 2;

        private readonly Color[] _colors = new Color[LayerCount];
        private readonly bool[] _active = new bool[LayerCount];

        /// <summary>En az bir katman açık.</summary>
        public bool Any => _active[(int)TintLayer.Base] || _active[(int)TintLayer.Status];

        public bool IsActive(TintLayer layer) => _active[(int)layer];

        public void Set(TintLayer layer, Color color)
        {
            _colors[(int)layer] = color;
            _active[(int)layer] = true;
        }

        /// <summary>Katmanı kapatır; açıktıysa true.</summary>
        public bool Clear(TintLayer layer)
        {
            bool wasActive = _active[(int)layer];
            _active[(int)layer] = false;
            return wasActive;
        }

        public void ClearAll()
        {
            for (int i = 0; i < LayerCount; i++)
            {
                _active[i] = false;
            }
        }

        /// <summary>Açık katmanları alttan üste <paramref name="original"/>'a karıştırır; alfa korunur.</summary>
        public Color Compose(Color original)
        {
            Color color = original;
            for (int i = 0; i < LayerCount; i++)
            {
                if (_active[i])
                {
                    color = HitFlash.Tint(color, _colors[i]);
                }
            }

            return color;
        }
    }
}
