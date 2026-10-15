using System.Collections.Generic;
using UnityEngine;

namespace LastHaven.Enemies
{
    /// <summary>
    /// <see cref="EnemySpawner"/> ayarları: doğurulabilecek düşman tipleri (havuz ön ısıtma sayılarıyla) ve NavMesh'e yerleştirme mesafesi.
    /// Asset'i üretici oluşturur: <c>Data/Enemies/EnemySpawnerConfig.asset</c>. Yeni tipler üretici tarafından listeye eklenir.
    /// </summary>
    public sealed class EnemySpawnerConfig : ScriptableObject
    {
        [Header("Tipler")]
        [SerializeField, Tooltip("Spawn isteği bu listedeki verilerin Id'siyle eşleşir. Listede olmayan tip doğurulamaz.")]
        private List<EnemyPoolEntry> _enemies = new List<EnemyPoolEntry>();

        [Header("Yerleştirme")]
        [SerializeField, Min(0.1f), Tooltip("İstenen nokta NavMesh üzerinde değilse bu yarıçap içindeki en yakın NavMesh noktası kullanılır (m).")]
        private float _navMeshSampleDistance = 3f;

        public IReadOnlyList<EnemyPoolEntry> Enemies => _enemies;

        public float NavMeshSampleDistance => _navMeshSampleDistance;
    }
}
