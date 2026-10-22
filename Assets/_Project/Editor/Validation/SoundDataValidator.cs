using LastHaven.Core;

namespace LastHaven.Editor
{
    /// <summary>
    /// <see cref="SoundData"/> kuralları (Validate Project, <see cref="AudioValidationStep"/>). Klip yoksa ses sessizce atlanır, oyun
    /// bozulmaz: bu yüzden boş klip listesi ve boş klip elemanı <b>uyarı</b>dır (hata değil). Kayıp klip referansı zorunlu referans
    /// adımında hata olarak çıkar.
    /// </summary>
    public static class SoundDataValidator
    {
        /// <summary>Uyarı yazdıysa true (klip yok).</summary>
        public static bool Validate(SoundData sound, ValidationReport report)
        {
            int clips = sound.CountClips();
            if (clips == 0)
            {
                report.Warning($"{sound.name}: klip yok; bu ses sessiz kalır (Clips listesine en az bir AudioClip ekle).", sound);
                return true;
            }

            int empty = sound.Clips.Count - clips;
            if (empty > 0)
            {
                report.Warning($"{sound.name}: klip listesinde {empty} boş eleman (atlanır).", sound);
            }

            return false;
        }
    }
}
