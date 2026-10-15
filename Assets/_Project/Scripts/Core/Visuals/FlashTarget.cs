using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>
    /// <see cref="HitFlash"/>'ın flaşlayabildiği bir renderer ve materyallerinde bulunan renk özellikleri
    /// (<c>_BaseColor</c>, <c>_EmissionColor</c>). Rengi MaterialPropertyBlock ile yazar; materyale dokunmaz.
    /// </summary>
    internal readonly struct FlashTarget
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        private readonly Renderer _renderer;
        private readonly bool _hasBaseColor;
        private readonly bool _hasEmissionColor;

        private FlashTarget(Renderer renderer, bool hasBaseColor, bool hasEmissionColor)
        {
            _renderer = renderer;
            _hasBaseColor = hasBaseColor;
            _hasEmissionColor = hasEmissionColor;
        }

        /// <summary>Renderer'ın materyallerinden en az biri renk özelliklerinden birini taşıyorsa true.</summary>
        public static bool TryCreate(Renderer renderer, out FlashTarget target)
        {
            bool hasBase = false;
            bool hasEmission = false;
            Material[] materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] == null)
                {
                    continue;
                }

                hasBase |= materials[i].HasProperty(BaseColorId);
                hasEmission |= materials[i].HasProperty(EmissionColorId);
            }

            target = new FlashTarget(renderer, hasBase, hasEmission);
            return hasBase || hasEmission;
        }

        public void Apply(MaterialPropertyBlock block, Color color)
        {
            if (_renderer == null)
            {
                return;
            }

            block.Clear();
            if (_hasBaseColor)
            {
                block.SetColor(BaseColorId, color);
            }

            // Materyalde emisyon kapalıysa (_EMISSION) etkisizdir; açıksa görsel parlar.
            if (_hasEmissionColor)
            {
                block.SetColor(EmissionColorId, color);
            }

            _renderer.SetPropertyBlock(block);
        }

        /// <summary>Bloğu kaldırır; renderer materyalin kendi rengine döner.</summary>
        public void Clear()
        {
            if (_renderer != null)
            {
                _renderer.SetPropertyBlock(null);
            }
        }
    }
}
