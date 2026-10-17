namespace LastHaven.Core
{
    /// <summary>
    /// Durum efekti alabilen şey (şimdilik düşmanlar). Yavaşlatanlar: diken tuzağı (GDD §11), Şok Taret (GDD §13).
    /// Okuyanlar: Avcı perk'i (yavaşlamış hedefe ek hasar). Collider'dan bulmak için <see cref="StatusEffectLookup"/>.
    /// </summary>
    public interface IStatusEffectReceiver
    {
        /// <summary>Yavaşlamış mı (en az bir yavaşlatma sürüyor).</summary>
        bool IsSlowed { get; }

        /// <summary>
        /// Yavaşlatır: <paramref name="fraction"/> hızdan düşen oran (0.4 = %40, 0..1'e kırpılır), <paramref name="duration"/> sn.
        /// Aynı anda birden fazla yavaşlatma varsa en güçlüsü geçerlidir; zayıf ama uzun olan, güçlüsü bitince devreye girer.
        /// Ölü alıcıda etkisizdir.
        /// </summary>
        void ApplySlow(float fraction, float duration);
    }
}
