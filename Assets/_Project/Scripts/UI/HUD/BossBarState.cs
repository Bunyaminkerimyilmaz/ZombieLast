namespace LastHaven.UI
{
    /// <summary>
    /// HUD boss barının durumu (saf): hangi boss izleniyor, adı ve canı, görünür mü. Doğan boss barı açar (öncekinin yerine geçer);
    /// can ve yenilgi sadece izlenen boss'tan kabul edilir. <typeparamref name="TBoss"/>: boss'u eşleyen kimlik (oyunda GameObject).
    /// </summary>
    public sealed class BossBarState<TBoss> where TBoss : class
    {
        private TBoss _boss;

        public bool IsVisible => _boss != null;

        public string Name { get; private set; } = string.Empty;

        public float Current { get; private set; }

        public float Max { get; private set; }

        /// <summary>Yeni boss: barı tam canla açar.</summary>
        public void Spawn(TBoss boss, string name, float maxHealth)
        {
            _boss = boss;
            Name = name ?? string.Empty;
            Current = maxHealth;
            Max = maxHealth;
        }

        /// <summary>İzlenen boss'un canıysa yazar ve true döner.</summary>
        public bool SetHealth(TBoss boss, float current, float max)
        {
            if (!IsTracking(boss))
            {
                return false;
            }

            Current = current;
            Max = max;
            return true;
        }

        /// <summary>İzlenen boss yenildiyse barı kapatır ve true döner.</summary>
        public bool Defeat(TBoss boss)
        {
            if (!IsTracking(boss))
            {
                return false;
            }

            Hide();
            return true;
        }

        /// <summary>Barı koşulsuz kapatır (ör. yeni gündüz).</summary>
        public void Hide()
        {
            _boss = null;
            Current = 0f;
            Max = 0f;
        }

        private bool IsTracking(TBoss boss)
        {
            return _boss != null && ReferenceEquals(_boss, boss);
        }
    }
}
