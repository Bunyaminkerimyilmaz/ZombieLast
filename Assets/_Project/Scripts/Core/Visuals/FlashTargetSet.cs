using System;
using System.Collections.Generic;
using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>
    /// <see cref="HitFlash"/>'ın renklendirdiği renderer'lar (<see cref="FlashTarget"/>) ve ortak MaterialPropertyBlock:
    /// hepsine tek renk (flaş), ton veya temizleme. Görsel kökün altındaki renk özelliği taşımayan renderer'lar atlanır.
    /// </summary>
    internal sealed class FlashTargetSet
    {
        public static readonly FlashTargetSet Empty = new FlashTargetSet(Array.Empty<FlashTarget>());

        private readonly MaterialPropertyBlock _block = new MaterialPropertyBlock();
        private readonly FlashTarget[] _targets;

        private FlashTargetSet(FlashTarget[] targets)
        {
            _targets = targets;
        }

        public int Count => _targets.Length;

        /// <summary><paramref name="visual"/> altındaki (kapalılar dahil) renderer'lardan kurar.</summary>
        public static FlashTargetSet Create(Transform visual)
        {
            Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(true);
            var targets = new List<FlashTarget>(renderers.Length);
            for (int i = 0; i < renderers.Length; i++)
            {
                if (FlashTarget.TryCreate(renderers[i], out FlashTarget target))
                {
                    targets.Add(target);
                }
            }

            return new FlashTargetSet(targets.ToArray());
        }

        /// <summary>Hepsine tek renk (<c>_BaseColor</c> ve <c>_EmissionColor</c>).</summary>
        public void Apply(Color color)
        {
            for (int i = 0; i < _targets.Length; i++)
            {
                _targets[i].Apply(_block, color);
            }
        }

        /// <summary>Hepsinin kendi rengini açık ton katmanlarına karıştırır (<see cref="TintLayers.Compose"/>).</summary>
        public void ApplyTint(TintLayers tints)
        {
            for (int i = 0; i < _targets.Length; i++)
            {
                _targets[i].ApplyTint(_block, tints);
            }
        }

        /// <summary>Blokları kaldırır; renderer'lar materyalin kendi rengine döner.</summary>
        public void Clear()
        {
            for (int i = 0; i < _targets.Length; i++)
            {
                _targets[i].Clear();
            }
        }
    }
}
