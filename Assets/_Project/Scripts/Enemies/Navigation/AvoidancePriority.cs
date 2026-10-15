using UnityEngine;

namespace LastHaven.Enemies
{
    /// <summary>
    /// NavMeshAgent kaçınma önceliği (<c>avoidancePriority</c>; 0 = en önemli, 99 = en az). Aynı öncelikli ajanlar birbirine
    /// eşit yol verir ve dar yerde (barikat önü) kilitlenir; her doğuşta aralıktan rastgele öncelik verilir. Saf; testlenebilir.
    /// </summary>
    public static class AvoidancePriority
    {
        public const int Min = 0;
        public const int Max = 99;

        /// <summary>
        /// [<paramref name="a"/>, <paramref name="b"/>] aralığından (sıra fark etmez, 0–99'a kırpılır) bir öncelik.
        /// <paramref name="random01"/> [0, 1) aralığındadır; 1 verilirse üst sınır döner.
        /// </summary>
        public static int Pick(int a, int b, float random01)
        {
            int low = Mathf.Clamp(Mathf.Min(a, b), Min, Max);
            int high = Mathf.Clamp(Mathf.Max(a, b), Min, Max);
            int span = high - low + 1;
            int offset = Mathf.FloorToInt(Mathf.Clamp01(random01) * span);
            return Mathf.Min(low + offset, high);
        }
    }
}
