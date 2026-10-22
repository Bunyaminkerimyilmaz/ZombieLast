using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Enemies
{
    /// <summary>
    /// Yay çizen mermi hesabı (saf): verilen sürede hedef noktaya düşen atış hızı ve zamana göre konum. Yerçekimi aşağı (−Y).
    /// </summary>
    public static class BallisticArc
    {
        /// <summary>Uçuş süresi (sn): yatay mesafe / yatay hız, en az <paramref name="minFlightTime"/>.</summary>
        public static float FlightTime(Vector3 start, Vector3 target, float horizontalSpeed, float minFlightTime)
        {
            float distance = PlanarDistance.Between(start, target);
            float time = horizontalSpeed > 0f ? distance / horizontalSpeed : minFlightTime;
            // İki argümanlı Max: üç argümanlı aşırı yükleme params dizisi ayırır.
            return Mathf.Max(Mathf.Max(minFlightTime, time), 0.01f);
        }

        /// <summary><paramref name="flightTime"/> sn sonra <paramref name="target"/>'a varan başlangıç hızı.</summary>
        public static Vector3 LaunchVelocity(Vector3 start, Vector3 target, float flightTime, float gravity)
        {
            return (target - start) / flightTime + Vector3.up * (0.5f * gravity * flightTime);
        }

        /// <summary>Atıştan <paramref name="time"/> sn sonraki konum.</summary>
        public static Vector3 PositionAt(Vector3 start, Vector3 velocity, float gravity, float time)
        {
            return start + velocity * time + Vector3.down * (0.5f * gravity * time * time);
        }

        /// <summary>Yayın başlangıç–hedef doğrusunun ortasındaki yüksekliği (m): g·T²/8.</summary>
        public static float MidArcHeight(float flightTime, float gravity)
        {
            return gravity * flightTime * flightTime / 8f;
        }
    }
}
