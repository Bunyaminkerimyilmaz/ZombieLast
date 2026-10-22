using System.Collections.Generic;

namespace LastHaven.Audio
{
    /// <summary>
    /// Çalan kopyaların kalan süreleri (saf C#). <see cref="Tick"/> süresi dolanları listeden çıkarıp çağırana verir; çağıran ses
    /// kaynağını havuza iade eder. Bellek ayırmaz (liste yeniden kullanılır).
    /// </summary>
    public sealed class VoiceTimers<TVoice> where TVoice : class
    {
        private readonly List<Entry> _entries = new List<Entry>();

        public int Count => _entries.Count;

        /// <summary>Bir kopya ekler: <paramref name="duration"/> sn sonra süresi dolar.</summary>
        public void Add(TVoice voice, object sound, float duration)
        {
            _entries.Add(new Entry(voice, sound, duration));
        }

        /// <summary>Süreleri <paramref name="deltaTime"/> kadar azaltır; dolanları çıkarıp <paramref name="expired"/>'a ekler.</summary>
        public void Tick(float deltaTime, List<Entry> expired)
        {
            for (int i = _entries.Count - 1; i >= 0; i--)
            {
                Entry entry = _entries[i];
                float remaining = entry.Remaining - deltaTime;
                if (remaining > 0f)
                {
                    _entries[i] = new Entry(entry.Voice, entry.Sound, remaining);
                    continue;
                }

                expired.Add(entry);
                int last = _entries.Count - 1;
                _entries[i] = _entries[last];
                _entries.RemoveAt(last);
            }
        }

        /// <summary>Tümünü çıkarıp <paramref name="removed"/>'a ekler (servis kapanırken).</summary>
        public void Clear(List<Entry> removed)
        {
            removed.AddRange(_entries);
            _entries.Clear();
        }

        public readonly struct Entry
        {
            public Entry(TVoice voice, object sound, float remaining)
            {
                Voice = voice;
                Sound = sound;
                Remaining = remaining;
            }

            public TVoice Voice { get; }

            public object Sound { get; }

            /// <summary>Kalan süre (sn).</summary>
            public float Remaining { get; }
        }
    }
}
