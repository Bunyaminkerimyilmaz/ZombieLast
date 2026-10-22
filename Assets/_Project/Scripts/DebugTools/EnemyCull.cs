using System.Collections.Generic;
using LastHaven.Core;
using LastHaven.Enemies;
using UnityEngine;

namespace LastHaven.DebugTools
{
    /// <summary>
    /// Debug: <see cref="EnemySpawner"/>'ın doğurduğu canlı düşmanları sayar ve normal hasarla öldürür (F6, geceyi atla, N. geceye git).
    /// Ölümler olaylara, dalga sayacına ve ödüllere her zamanki gibi gider. Kaynak verilen obje olduğu için (Player tag'i yok)
    /// <c>EnemyKilledEvent.KilledByPlayer</c> false olur. Bellek ayırmaz (tampon liste yeniden kullanılır).
    /// </summary>
    public sealed class EnemyCull
    {
        private readonly List<Enemy> _alive = new List<Enemy>();

        /// <summary>Canlı düşman sayısı (dalga, gündüz gezginleri ve debug spawner'ınkiler; sahneye elle konanlar hariç).</summary>
        public int CountAlive(EnemySpawner spawner)
        {
            if (spawner == null)
            {
                return 0;
            }

            spawner.GetAlive(_alive);
            int count = _alive.Count;
            _alive.Clear();
            return count;
        }

        /// <summary>Hepsini kalan canı kadar hasarla öldürür; ölen sayısını döndürür.</summary>
        public int KillAll(EnemySpawner spawner, GameObject source)
        {
            if (spawner == null)
            {
                return 0;
            }

            // Önce kopyala: ölüm olayları dinleyicileri tetikler, liste öldürürken değişmesin.
            spawner.GetAlive(_alive);
            int killed = KillAll(_alive, source);
            _alive.Clear();
            return killed;
        }

        /// <summary>Listedeki canlıları öldürür (liste değişmez; çağıran ölüm sırasında listeyi değiştirmemeli).</summary>
        public int KillAll(IReadOnlyList<Enemy> enemies, GameObject source)
        {
            int killed = 0;
            for (int i = 0; i < enemies.Count; i++)
            {
                if (enemies[i] != null && Kill(enemies[i], source))
                {
                    killed++;
                }
            }

            return killed;
        }

        private static bool Kill(Enemy enemy, GameObject source)
        {
            // Zincirleme patlama aynı listedeki bir düşmanı önceden öldürmüş olabilir.
            var health = enemy.GetComponent<Health>();
            if (health == null || !health.IsAlive)
            {
                return false;
            }

            // Yön sıfır: geri itilme yok.
            health.TakeDamage(new DamageInfo(health.Current, DamageType.Bullet, source, enemy.transform.position, Vector3.zero));
            return !health.IsAlive;
        }
    }
}
