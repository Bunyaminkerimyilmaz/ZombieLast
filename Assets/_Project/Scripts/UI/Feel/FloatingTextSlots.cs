using System;

namespace LastHaven.UI
{
    /// <summary>
    /// Uçan yazı havuzunun yuvaları (saf C#): sabit sayıda yuva, her birinin yaşı ve ömrü. <see cref="Acquire"/> boş yuvayı, yoksa
    /// sonuna en yakın (en eski) dolu yuvayı verir: havuz büyümez, oynanış sırasında element oluşmaz. Ömrü dolan yuva
    /// <see cref="Tick"/>'te boşalır.
    /// </summary>
    public sealed class FloatingTextSlots
    {
        private readonly float[] _age;
        private readonly float[] _lifetime;
        private readonly bool[] _active;

        public FloatingTextSlots(int capacity)
        {
            if (capacity < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "En az bir yuva gerekir.");
            }

            _age = new float[capacity];
            _lifetime = new float[capacity];
            _active = new bool[capacity];
        }

        public int Capacity => _active.Length;

        public int ActiveCount { get; private set; }

        public bool IsActive(int slot)
        {
            return _active[slot];
        }

        /// <summary>Yuvanın yaşı (sn).</summary>
        public float Age(int slot)
        {
            return _age[slot];
        }

        /// <summary>0 = yeni, 1 = ömrü doldu.</summary>
        public float Progress(int slot)
        {
            return _lifetime[slot] > 0f ? Math.Min(1f, _age[slot] / _lifetime[slot]) : 1f;
        }

        /// <summary>Yuvayı <paramref name="lifetime"/> sn ömürle başlatır: boş yuva, yoksa en eskisi.</summary>
        public int Acquire(float lifetime)
        {
            int slot = -1;
            float oldest = -1f;
            for (int i = 0; i < _active.Length; i++)
            {
                if (!_active[i])
                {
                    slot = i;
                    break;
                }

                float progress = Progress(i);
                if (progress > oldest)
                {
                    oldest = progress;
                    slot = i;
                }
            }

            if (!_active[slot])
            {
                ActiveCount++;
            }

            _active[slot] = true;
            Restart(slot, lifetime);
            return slot;
        }

        /// <summary>Dolu yuvanın yaşını sıfırlar (yazı yeniden "vurur").</summary>
        public void Restart(int slot, float lifetime)
        {
            _age[slot] = 0f;
            _lifetime[slot] = Math.Max(0f, lifetime);
        }

        /// <summary>Yaşları ilerletir; ömrü dolan yuvalar boşalır.</summary>
        public void Tick(float deltaTime)
        {
            for (int i = 0; i < _active.Length; i++)
            {
                if (!_active[i])
                {
                    continue;
                }

                _age[i] += deltaTime;
                if (_age[i] >= _lifetime[i])
                {
                    _active[i] = false;
                    ActiveCount--;
                }
            }
        }

        public void Clear()
        {
            Array.Clear(_active, 0, _active.Length);
            ActiveCount = 0;
        }
    }
}
