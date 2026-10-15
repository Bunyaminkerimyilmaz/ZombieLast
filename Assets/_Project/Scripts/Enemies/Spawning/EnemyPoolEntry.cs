using System;
using UnityEngine;

namespace LastHaven.Enemies
{
    /// <summary><see cref="EnemySpawnerConfig"/> satırı: doğurulabilecek bir düşman tipi ve havuzda önceden hazırlanacak örnek sayısı.</summary>
    [Serializable]
    public struct EnemyPoolEntry
    {
        [SerializeField, Tooltip("Düşman tipi. Spawn isteği bu verinin Id'siyle eşleşir.")]
        private EnemyData _data;

        [SerializeField, Min(0), Tooltip("Oyun başında havuzda hazırlanacak örnek sayısı. Aşılırsa oynanışta Instantiate olur (havuz bir kez uyarır).")]
        private int _prewarm;

        public EnemyPoolEntry(EnemyData data, int prewarm)
        {
            _data = data;
            _prewarm = prewarm;
        }

        /// <summary>Null olabilir (inspector'da boş bırakılmış satır).</summary>
        public EnemyData Data => _data;

        public int Prewarm => _prewarm;
    }
}
