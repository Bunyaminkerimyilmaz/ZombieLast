using System;
using System.Collections.Generic;
using UnityEngine;

namespace LastHaven.Building
{
    /// <summary>
    /// Yapının görselini seviyeye göre değiştirir (GDD §18: barikat kahve → gri → beyaz). "Visual" altındaki renderer'ların
    /// prefab'daki materyallerinden <b>herhangi bir seviyenin materyali olanlar</b> hedef seviyenin materyaliyle değişir;
    /// diğer materyallere (kullanıcının modeli) dokunulmaz. Materyaller oluşturulurken bir kez okunur (havuz döngüsünde
    /// <see cref="Restore"/> ile geri verilir).
    /// </summary>
    public sealed class StructureLevelVisual
    {
        private readonly Renderer[] _renderers;
        private readonly Material[][] _original;

        /// <param name="visual">Görsel kök (prefab'ın "Visual" çocuğu). Null olabilir; o zaman hiçbir şey değişmez.</param>
        public StructureLevelVisual(Transform visual)
        {
            _renderers = visual != null ? visual.GetComponentsInChildren<Renderer>(true) : Array.Empty<Renderer>();
            _original = new Material[_renderers.Length][];
            for (int i = 0; i < _renderers.Length; i++)
            {
                _original[i] = _renderers[i].sharedMaterials;
            }
        }

        public int RendererCount => _renderers.Length;

        /// <summary>
        /// Orijinal materyal seviyelerden birinin materyaliyse hedef seviyeninkini (o boşsa orijinali), değilse orijinali döndürür.
        /// </summary>
        public static Material Resolve(Material original, IReadOnlyList<BuildableLevel> levels, int levelIndex)
        {
            if (original == null || levels == null || levelIndex < 0 || levelIndex >= levels.Count)
            {
                return original;
            }

            Material target = levels[levelIndex]?.Material;
            if (target == null || target == original)
            {
                return original;
            }

            for (int i = 0; i < levels.Count; i++)
            {
                if (levels[i] != null && levels[i].Material == original)
                {
                    return target;
                }
            }

            return original;
        }

        /// <summary>Renderer'lara <paramref name="levelIndex"/> seviyesinin materyallerini uygular.</summary>
        public void Apply(IReadOnlyList<BuildableLevel> levels, int levelIndex)
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] == null)
                {
                    continue;
                }

                Material[] original = _original[i];
                var materials = new Material[original.Length];
                for (int j = 0; j < original.Length; j++)
                {
                    materials[j] = Resolve(original[j], levels, levelIndex);
                }

                _renderers[i].sharedMaterials = materials;
            }
        }

        /// <summary>Prefab'daki materyalleri geri verir.</summary>
        public void Restore()
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] != null)
                {
                    _renderers[i].sharedMaterials = _original[i];
                }
            }
        }
    }
}
