namespace LastHaven.Building
{
    /// <summary>
    /// Tuzağın tik zamanlaması (saf C#; GDD §11 "hasar tiki 0.5 sn'de bir"). Tik sadece üstünde düşman varken olur:
    /// <see cref="Advance"/> true dönünce tuzak üstüne bakar; biri varsa tik atar ve <see cref="MarkTick"/> (sonraki tik bir aralık
    /// sonra), yoksa <see cref="MarkIdle"/> (kısa aralıkla yeniden bakılır, ilk giren hemen tik alır). İn-çık ile aralık kısalmaz.
    /// Kare takılmasında devreden süre en fazla bir aralıktır (birikmiş tikler arka arkaya atılmaz).
    /// </summary>
    public sealed class TrapTickTimer
    {
        /// <summary>Sonraki bakışa kalan süre (sn); 0 veya altı = şimdi.</summary>
        public float Cooldown { get; private set; }

        /// <summary>Süreyi ilerletir; tuzağın üstüne bakma zamanı geldiyse true.</summary>
        public bool Advance(float deltaTime)
        {
            Cooldown -= deltaTime;
            return Cooldown <= 0f;
        }

        /// <summary>Tik atıldı: sonraki tik <paramref name="interval"/> sonra (taşan süre düşülerek, en fazla bir aralık).</summary>
        public void MarkTick(float interval)
        {
            float overshoot = Cooldown < -interval ? -interval : Cooldown;
            Cooldown = overshoot + interval;
        }

        /// <summary>Üstünde kimse yok: <paramref name="scanInterval"/> sonra yeniden bakılır.</summary>
        public void MarkIdle(float scanInterval)
        {
            Cooldown = scanInterval;
        }

        /// <summary>Yerleşmede/havuzda: ilk bakış hemen.</summary>
        public void Reset()
        {
            Cooldown = 0f;
        }
    }
}
