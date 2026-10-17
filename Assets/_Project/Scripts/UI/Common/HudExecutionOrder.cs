namespace LastHaven.UI
{
    /// <summary>Dünyaya bağlı HUD kutularının çalışma sırası.</summary>
    public static class HudExecutionOrder
    {
        /// <summary>
        /// Kamera her kare <c>CinemachineBrain</c>'in LateUpdate'inde (sıra 100) hareket eder; dünya noktasını ekrana yansıtan
        /// bileşenler ondan sonra çalışır, yoksa kutu bir kare geriden gelip titrer.
        /// </summary>
        public const int AfterCamera = 200;
    }
}
