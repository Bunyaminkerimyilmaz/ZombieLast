using LastHaven.Core;

namespace LastHaven.Building
{
    /// <summary>İnşa moduna giriş kararı.</summary>
    public enum BuildModeEntry
    {
        Allowed,
        NotDay,
        PlayerDead,
    }

    /// <summary>
    /// İnşa moduna giriş kuralı (saf): sadece gündüz (GDD §11) ve oyuncu canlıyken (15.3: ölü oyuncu yeniden doğmayı beklerken inşa
    /// edemez). <see cref="BuildModeController"/> kullanır; mod açıkken oyuncu ölürse de kapatır.
    /// </summary>
    public static class BuildModeEntryRule
    {
        /// <param name="phase">Şu anki evre; oyun akışı yoksa null (kısıt yok).</param>
        /// <param name="playerAlive">Oyuncu canlı mı; oyuncu servisi yoksa true ver.</param>
        public static BuildModeEntry Evaluate(GamePhase? phase, bool playerAlive)
        {
            if (phase.HasValue && phase.Value != GamePhase.Day)
            {
                return BuildModeEntry.NotDay;
            }

            return playerAlive ? BuildModeEntry.Allowed : BuildModeEntry.PlayerDead;
        }

        /// <summary>Reddin log metni; <see cref="BuildModeEntry.Allowed"/> için boş.</summary>
        public static string Describe(BuildModeEntry entry)
        {
            switch (entry)
            {
                case BuildModeEntry.NotDay:
                    return "İnşa modu sadece gündüz açılır.";
                case BuildModeEntry.PlayerDead:
                    return "Ölüyken inşa modu açılmaz; yeniden doğmayı bekle.";
                default:
                    return string.Empty;
            }
        }
    }
}
