using System;

namespace LastHaven.UI
{
    /// <summary>
    /// Adaylar arasından en yakın <see cref="Capacity"/> tanesini tutar (saf; bellek ayırmaz): her kare <see cref="Clear"/>, sonra
    /// her aday için <see cref="Offer"/>; sonuç yakından uzağa <see cref="GetIndex"/>. Eşit uzaklıkta önce gelen önde kalır.
    /// </summary>
    public sealed class NearestPicker
    {
        private readonly int[] _indices;
        private readonly float[] _distances;

        public NearestPicker(int capacity)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "Kapasite negatif olamaz.");
            }

            _indices = new int[capacity];
            _distances = new float[capacity];
        }

        public int Capacity => _indices.Length;

        /// <summary>Tutulan aday sayısı (en çok <see cref="Capacity"/>).</summary>
        public int Count { get; private set; }

        public void Clear()
        {
            Count = 0;
        }

        /// <summary>Adayı uzaklığına göre sıraya sokar; listede yer yoksa ve en uzaktan yakın değilse atlanır.</summary>
        public void Offer(int index, float distance)
        {
            int position = Count;
            while (position > 0 && distance < _distances[position - 1])
            {
                position--;
            }

            if (position >= Capacity)
            {
                return;
            }

            int last = Count < Capacity ? Count : Capacity - 1;
            for (int i = last; i > position; i--)
            {
                _indices[i] = _indices[i - 1];
                _distances[i] = _distances[i - 1];
            }

            _indices[position] = index;
            _distances[position] = distance;
            if (Count < Capacity)
            {
                Count++;
            }
        }

        /// <summary>Yakınlık sırasındaki adayın indeksi (0 = en yakın).</summary>
        public int GetIndex(int rank)
        {
            if (rank < 0 || rank >= Count)
            {
                throw new ArgumentOutOfRangeException(nameof(rank), rank, $"0..{Count - 1} olmalı.");
            }

            return _indices[rank];
        }
    }
}
