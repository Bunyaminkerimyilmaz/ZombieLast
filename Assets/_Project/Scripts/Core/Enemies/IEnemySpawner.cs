using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>
    /// Düşman doğurma servisi (Enemies'teki <c>EnemySpawner</c>, [Systems]/EnemySpawner). Core <c>EnemyData</c>'yı bilmediği için
    /// tip, verinin string id'siyle verilir (ör. "walker"). Düşman havuzdan çekilir ve istenen noktaya en yakın NavMesh noktasına konur.
    /// Dalgalar (Waves) ve geliştirici araçları (DebugTools) kullanır.
    /// </summary>
    public interface IEnemySpawner
    {
        /// <summary>
        /// <paramref name="enemyId"/> tipinden bir düşmanı <paramref name="position"/>'a en yakın NavMesh noktasında doğurur.
        /// Bilinmeyen id, yakında NavMesh olmaması veya havuz servisinin olmaması durumunda uyarı loglar ve null döner.
        /// </summary>
        /// <returns>Doğan düşmanın kök objesi; başarısızsa null.</returns>
        GameObject Spawn(string enemyId, Vector3 position);
    }
}
