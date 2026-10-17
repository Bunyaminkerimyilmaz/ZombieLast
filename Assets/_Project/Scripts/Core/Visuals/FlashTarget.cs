using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>
    /// <see cref="HitFlash"/>'ın renklendirebildiği bir renderer ve materyallerinde bulunan renk özellikleri
    /// (<c>_BaseColor</c>, <c>_EmissionColor</c>). Rengi MaterialPropertyBlock ile yazar; materyale dokunmaz.
    /// </summary>
    internal readonly struct FlashTarget
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        private readonly Renderer _renderer;
        private readonly bool _hasBaseColor;
        private readonly bool _hasEmissionColor;
        private readonly Color _baseColor;

        private FlashTarget(Renderer renderer, bool hasBaseColor, bool hasEmissionColor, Color baseColor)
        {
            _renderer = renderer;
            _hasBaseColor = hasBaseColor;
            _hasEmissionColor = hasEmissionColor;
            _baseColor = baseColor;
        }

        /// <summary>Renderer'ın materyallerinden en az biri renk özelliklerinden birini taşıyorsa true.</summary>
        public static bool TryCreate(Renderer renderer, out FlashTarget target)
        {
            bool hasBase = false;
            bool hasEmission = false;
            Color baseColor = Color.white;
            Material[] materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] == null)
                {
                    continue;
                }

                // Ton için kendi rengi: _BaseColor taşıyan ilk materyalin rengi.
                if (!hasBase && materials[i].HasProperty(BaseColorId))
                {
                    baseColor = materials[i].GetColor(BaseColorId);
                    hasBase = true;
                }

                hasEmission |= materials[i].HasProperty(EmissionColorId);
            }

            target = new FlashTarget(renderer, hasBase, hasEmission, baseColor);
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

        /// <summary>
        /// Kalıcı renk tonu: materyalin kendi <c>_BaseColor</c>'ı <paramref name="tint"/>'e doğru karıştırılır
        /// (<see cref="HitFlash.Tint"/>). Emisyona dokunulmaz. <c>_BaseColor</c> yoksa blok temizlenir.
        /// </summary>
        public void ApplyTint(MaterialPropertyBlock block, Color tint)
        {
            if (_renderer == null)
            {
                return;
            }

            if (!_hasBaseColor)
            {
                Clear();
                return;
            }

            block.Clear();
            block.SetColor(BaseColorId, HitFlash.Tint(_baseColor, tint));
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
