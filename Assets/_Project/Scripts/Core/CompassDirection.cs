using System;
using System.Collections.Generic;
using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>Haritanın dört ana yönü. Spawn bölgeleri ve gece verisi bu yönlerle eşleşir.</summary>
    public enum CompassDirection
    {
        North,
        East,
        South,
        West,
    }

    /// <summary><see cref="CompassDirection"/> yardımcıları.</summary>
    public static class CompassDirections
    {
        /// <summary>Dört yön, enum sırasıyla.</summary>
        public static readonly IReadOnlyList<CompassDirection> All = new[]
        {
            CompassDirection.North,
            CompassDirection.East,
            CompassDirection.South,
            CompassDirection.West,
        };

        /// <summary>Yönün dünya uzayındaki yatay birim vektörü: Kuzey +Z, Doğu +X.</summary>
        public static Vector3 ToVector3(this CompassDirection direction)
        {
            return direction switch
            {
                CompassDirection.North => Vector3.forward,
                CompassDirection.East => Vector3.right,
                CompassDirection.South => Vector3.back,
                CompassDirection.West => Vector3.left,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null),
            };
        }
    }
}
