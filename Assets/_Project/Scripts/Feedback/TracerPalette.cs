using System;
using System.Collections.Generic;
using LastHaven.Combat;
using UnityEngine;

namespace LastHaven.Feedback
{
    /// <summary>
    /// Silah id'sinden mermi izi rengi (<see cref="WeaponData.TracerColor"/>). Bilinmeyen veya boş id varsayılan rengi alır.
    /// Id'ler ordinal karşılaştırılır; aynı id iki kez verilirse ilki geçerlidir.
    /// </summary>
    public sealed class TracerPalette
    {
        private readonly Dictionary<string, Color> _colors = new Dictionary<string, Color>(StringComparer.Ordinal);
        private readonly Color _defaultColor;

        public TracerPalette(IReadOnlyList<WeaponData> weapons, Color defaultColor)
        {
            _defaultColor = defaultColor;
            if (weapons == null)
            {
                return;
            }

            for (int i = 0; i < weapons.Count; i++)
            {
                WeaponData weapon = weapons[i];
                if (weapon != null && !string.IsNullOrEmpty(weapon.Id) && !_colors.ContainsKey(weapon.Id))
                {
                    _colors.Add(weapon.Id, weapon.TracerColor);
                }
            }
        }

        public Color Get(string weaponId)
        {
            return weaponId != null && _colors.TryGetValue(weaponId, out Color color) ? color : _defaultColor;
        }
    }
}
