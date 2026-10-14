using System;

namespace LastHaven.Player
{
    /// <summary>
    /// Silah yuvası seçimi (saf C#): açık/kilitli yuvalar, doğrudan seçim ve sırayla değiştirme.
    /// Sırayla değiştirmede kilitli yuvalar atlanır, sona gelince başa sarar.
    /// </summary>
    public sealed class WeaponSlots
    {
        /// <summary>Hiçbir yuva seçili değil.</summary>
        public const int None = -1;

        private readonly bool[] _unlocked;

        public WeaponSlots(int count)
        {
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), count, "Yuva sayısı negatif olamaz.");
            }

            _unlocked = new bool[count];
        }

        public int Count => _unlocked.Length;

        /// <summary>Seçili yuva; hiçbiri seçili değilse <see cref="None"/>.</summary>
        public int Current { get; private set; } = None;

        public bool IsValid(int slot)
        {
            return slot >= 0 && slot < _unlocked.Length;
        }

        public bool IsUnlocked(int slot)
        {
            return IsValid(slot) && _unlocked[slot];
        }

        /// <summary>Yuvayı açar. Geçersizse veya zaten açıksa false. Seçimi değiştirmez.</summary>
        public bool Unlock(int slot)
        {
            if (!IsValid(slot) || _unlocked[slot])
            {
                return false;
            }

            _unlocked[slot] = true;
            return true;
        }

        /// <summary>Açık ve şu ankinden farklı bir yuvaysa seçer.</summary>
        public bool TrySelect(int slot)
        {
            if (!IsUnlocked(slot) || slot == Current)
            {
                return false;
            }

            Current = slot;
            return true;
        }

        /// <summary>
        /// Seçili yuvadan <paramref name="direction"/> yönünde (+1 sıradaki, -1 önceki) ilk açık yuva; başa/sona sarar.
        /// Başka açık yuva yoksa <see cref="Current"/>. Hiçbir yuva seçili değilse baştan (veya sondan) arar.
        /// </summary>
        public int FindNext(int direction)
        {
            int count = _unlocked.Length;
            if (count == 0 || direction == 0)
            {
                return Current;
            }

            int step = direction > 0 ? 1 : -1;
            int start = Current != None ? Current : (step > 0 ? -1 : count);
            for (int i = 1; i <= count; i++)
            {
                int slot = ((start + step * i) % count + count) % count;
                if (_unlocked[slot])
                {
                    return slot;
                }
            }

            return Current;
        }

        /// <summary><see cref="FindNext"/> ile bulunan yuvayı seçer; değiştiyse true.</summary>
        public bool TryCycle(int direction)
        {
            return TrySelect(FindNext(direction));
        }
    }
}
