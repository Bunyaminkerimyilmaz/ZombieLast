using UnityEngine;

namespace LastHaven.Building
{
    /// <summary>
    /// Taret kafasının yatay yönü (saf): kafa sadece Y ekseninde, sabit hızla hedefe döner (GDD §11). Açılar dünya yaw'ı
    /// (derece, 0 = +Z kuzey, saat yönünde artar; [0, 360)).
    /// </summary>
    public sealed class TurretAim
    {
        /// <summary>Kafanın şu anki yaw'ı (derece, [0, 360)).</summary>
        public float Yaw { get; private set; }

        /// <summary>Yönü anında ayarlar (yerleşme, havuzdan çıkış).</summary>
        public void Reset(float yaw)
        {
            Yaw = Normalize(yaw);
        }

        /// <summary>Kafayı en kısa yoldan hedef yaw'a doğru en fazla <paramref name="turnSpeed"/> × süre kadar döndürür.</summary>
        /// <param name="turnSpeed">Derece/sn; 0 veya negatifse dönmez.</param>
        public void Tick(float targetYaw, float turnSpeed, float deltaTime)
        {
            if (turnSpeed <= 0f || deltaTime <= 0f)
            {
                return;
            }

            Yaw = Normalize(Mathf.MoveTowardsAngle(Yaw, targetYaw, turnSpeed * deltaTime));
        }

        /// <summary>Kafa hedef yaw'a <paramref name="tolerance"/> dereceden yakın mı (dahil).</summary>
        public bool IsAlignedWith(float targetYaw, float tolerance)
        {
            return Mathf.Abs(Mathf.DeltaAngle(Yaw, targetYaw)) <= tolerance;
        }

        /// <summary>Yatay düzlemde <paramref name="from"/>'dan <paramref name="to"/>'ya bakan yaw (derece, [0, 360)). Aynı noktada 0.</summary>
        public static float YawTowards(Vector3 from, Vector3 to)
        {
            float dx = to.x - from.x;
            float dz = to.z - from.z;
            if (dx * dx + dz * dz < 1e-8f)
            {
                return 0f;
            }

            return Normalize(Mathf.Atan2(dx, dz) * Mathf.Rad2Deg);
        }

        /// <summary>Yaw'ın yatay yön vektörü (birim).</summary>
        public static Vector3 Direction(float yaw)
        {
            float radians = yaw * Mathf.Deg2Rad;
            return new Vector3(Mathf.Sin(radians), 0f, Mathf.Cos(radians));
        }

        private static float Normalize(float degrees)
        {
            float result = Mathf.Repeat(degrees, 360f);
            return result >= 360f ? 0f : result;
        }
    }
}
