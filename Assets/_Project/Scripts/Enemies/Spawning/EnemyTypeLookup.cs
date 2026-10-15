using System;
using System.Collections.Generic;

namespace LastHaven.Enemies
{
    /// <summary>
    /// <see cref="EnemyData.Id"/> → <see cref="EnemyData"/> sözlüğü (saf C#). Id'ler ordinal karşılaştırılır (büyük/küçük harf duyarlı,
    /// kültürden bağımsız). Boş id'li ve tekrar eden tipler eklenmez.
    /// </summary>
    public sealed class EnemyTypeLookup
    {
        private readonly Dictionary<string, EnemyData> _types = new Dictionary<string, EnemyData>(StringComparer.Ordinal);

        public int Count => _types.Count;

        /// <summary>Tipi ekler. Veri null, id boş veya bu id zaten kayıtlıysa eklemez ve sebebini döndürür.</summary>
        public EnemyTypeAddResult TryAdd(EnemyData data)
        {
            if (data == null)
            {
                return EnemyTypeAddResult.MissingData;
            }

            if (string.IsNullOrEmpty(data.Id))
            {
                return EnemyTypeAddResult.EmptyId;
            }

            if (_types.ContainsKey(data.Id))
            {
                return EnemyTypeAddResult.DuplicateId;
            }

            _types.Add(data.Id, data);
            return EnemyTypeAddResult.Added;
        }

        /// <summary>Id'si <paramref name="enemyId"/> olan tipi bulur. Null veya boş id için false.</summary>
        public bool TryGet(string enemyId, out EnemyData data)
        {
            if (string.IsNullOrEmpty(enemyId))
            {
                data = null;
                return false;
            }

            return _types.TryGetValue(enemyId, out data);
        }
    }

    /// <summary><see cref="EnemyTypeLookup.TryAdd"/> sonucu.</summary>
    public enum EnemyTypeAddResult
    {
        Added,
        MissingData,
        EmptyId,
        DuplicateId,
    }
}
