using System;
using System.Collections.Generic;

namespace LastHaven.Building
{
    /// <summary>
    /// İnşa modundaki seçim (saf C#): yuvalardan (1–5) seçili yapı ve önizlemenin dönüşü. Boş (null) yuva seçilemez.
    /// Dönüş seçim değişince korunur.
    /// </summary>
    public sealed class BuildSelection
    {
        private readonly IReadOnlyList<BuildableData> _slots;

        public BuildSelection(IReadOnlyList<BuildableData> slots)
        {
            _slots = slots ?? throw new ArgumentNullException(nameof(slots));
        }

        public int SlotCount => _slots.Count;

        /// <summary>0 tabanlı seçili yuva; seçim yoksa -1.</summary>
        public int SelectedIndex { get; private set; } = -1;

        public BuildableData Selected => SelectedIndex >= 0 ? _slots[SelectedIndex] : null;

        public bool HasSelection => Selected != null;

        /// <summary>Y ekseninde 90°'lik adım sayısı (0–3).</summary>
        public int RotationSteps { get; private set; }

        /// <summary>Yuva var ve içinde yapı var mı.</summary>
        public bool IsAvailable(int slotIndex)
        {
            return slotIndex >= 0 && slotIndex < _slots.Count && _slots[slotIndex] != null;
        }

        /// <summary>Yuvayı seçer. Seçim değiştiyse true; yuva boşsa veya zaten seçiliyse false.</summary>
        public bool Select(int slotIndex)
        {
            if (!IsAvailable(slotIndex) || slotIndex == SelectedIndex)
            {
                return false;
            }

            SelectedIndex = slotIndex;
            return true;
        }

        /// <summary>
        /// Sıradaki (+1) veya önceki (-1) dolu yuvaya geçer, boşları atlar ve başa sarar. Seçim yokken +1 ilk, -1 son dolu yuvayı
        /// seçer. Seçim değiştiyse true.
        /// </summary>
        public bool Cycle(int direction)
        {
            int count = _slots.Count;
            if (direction == 0 || count == 0)
            {
                return false;
            }

            int step = direction > 0 ? 1 : -1;
            int index = SelectedIndex >= 0 ? SelectedIndex : (step > 0 ? -1 : count);
            for (int i = 0; i < count; i++)
            {
                index = ((index + step) % count + count) % count;
                if (IsAvailable(index))
                {
                    return Select(index);
                }
            }

            return false;
        }

        /// <summary>Seçimi kaldırır. Seçim vardıysa true.</summary>
        public bool Clear()
        {
            if (SelectedIndex < 0)
            {
                return false;
            }

            SelectedIndex = -1;
            return true;
        }

        /// <summary>Önizlemeyi 90° döndürür (yukarıdan bakınca saat yönü).</summary>
        public void Rotate()
        {
            RotationSteps = (RotationSteps + 1) % 4;
        }
    }
}
