using System;
using System.Collections.Generic;

namespace LastHaven.Core
{
    /// <summary>
    /// Duraklatan kaynakların kümesi (saf C#; <see cref="IPauseService"/>'in mantığı). Kaynaklar referansla karşılaştırılır; aynı kaynak
    /// bir kez sayılır. <see cref="Add"/> / <see cref="Remove"/> duraklatma durumunu değiştirdiyse true döner (ilk kaynak girdi /
    /// son kaynak çıktı); köprü sadece o zaman <c>Time.timeScale</c>'e yazar.
    /// </summary>
    public sealed class PauseSources
    {
        private readonly List<object> _sources = new List<object>();

        public bool IsPaused => _sources.Count > 0;

        public int Count => _sources.Count;

        /// <summary>Kaynağı ekler. Oyun bu çağrıyla durduysa (önceden kaynak yoktu) true; kaynak zaten varsa veya başka kaynak varsa false.</summary>
        public bool Add(object source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (Contains(source))
            {
                return false;
            }

            _sources.Add(source);
            return _sources.Count == 1;
        }

        /// <summary>Kaynağı çıkarır. Oyun bu çağrıyla devam ettiyse (son kaynak çıktı) true; kaynak yoksa veya başkası kaldıysa false.</summary>
        public bool Remove(object source)
        {
            int index = IndexOf(source);
            if (index < 0)
            {
                return false;
            }

            _sources.RemoveAt(index);
            return _sources.Count == 0;
        }

        public bool Contains(object source)
        {
            return IndexOf(source) >= 0;
        }

        /// <summary>
        /// <paramref name="isGone"/>'un true dediği kaynakları (ör. yok edilmiş Unity nesneleri) çıkarır. Oyun bu çağrıyla devam ettiyse
        /// (kaynak vardı, hiç kalmadı) true.
        /// </summary>
        public bool RemoveWhere(Predicate<object> isGone)
        {
            if (_sources.Count == 0)
            {
                return false;
            }

            int removed = _sources.RemoveAll(isGone);
            return removed > 0 && _sources.Count == 0;
        }

        public void Clear()
        {
            _sources.Clear();
        }

        private int IndexOf(object source)
        {
            for (int i = 0; i < _sources.Count; i++)
            {
                if (ReferenceEquals(_sources[i], source))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
