namespace LastHaven.Core
{
    /// <summary>Bir evrenin istediği ışık durumu.</summary>
    public enum LightingTarget
    {
        /// <summary>Işık değişmez (önceki durum sürer).</summary>
        Unchanged = 0,

        /// <summary>Gündüz: güneş, fener ve üs ışıkları kapalı.</summary>
        Day = 1,

        /// <summary>Gece: koyu mavi ambiyans, fener ve üs ışıkları açık.</summary>
        Night = 2,
    }

    /// <summary>
    /// Evre → ışık kuralı (GDD §3, §18). Gün/gece ışığı, fener ve üs ışıkları aynı kuralı kullanır:
    /// alacakaranlık ve gece → gece; gündüz, şafak ve zafer (tahliye sabahı) → gündüz; açılış ve oyun bitti → değişmez.
    /// </summary>
    public static class PhaseLighting
    {
        public static LightingTarget For(GamePhase phase)
        {
            switch (phase)
            {
                case GamePhase.Dusk:
                case GamePhase.Night:
                    return LightingTarget.Night;
                case GamePhase.Day:
                case GamePhase.Dawn:
                case GamePhase.Victory:
                    return LightingTarget.Day;
                default:
                    return LightingTarget.Unchanged;
            }
        }
    }
}
