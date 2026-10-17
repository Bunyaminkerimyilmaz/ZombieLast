namespace LastHaven.Enemies
{
    /// <summary>
    /// Boss öfkesi kuralı (saf, GDD §7): can oranı eşiğin altına ilk düştüğünde bir kez öfkelenir ve doğuş boyunca öyle kalır
    /// (iyileşse de sönmez). Havuzdan her çıkışta <see cref="Reset"/>.
    /// </summary>
    public sealed class BossRage
    {
        public bool IsEnraged { get; private set; }

        /// <summary>
        /// Can oranı <paramref name="threshold"/>'un altındaysa (eşitlik öfkelendirmez) ve henüz öfkeli değilse öfkelenir ve true döner.
        /// Ölü boss (<paramref name="isAlive"/> false) öfkelenmez.
        /// </summary>
        public bool TryEnrage(float healthFraction, float threshold, bool isAlive)
        {
            if (IsEnraged || !isAlive || !(healthFraction < threshold))
            {
                return false;
            }

            IsEnraged = true;
            return true;
        }

        public void Reset()
        {
            IsEnraged = false;
        }
    }
}
