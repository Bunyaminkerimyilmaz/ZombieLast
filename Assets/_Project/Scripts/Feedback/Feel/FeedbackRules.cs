using System;
using System.Collections.Generic;

namespace LastHaven.Feedback
{
    /// <summary>Oyun hissi eşlemesinin saf kuralları (<see cref="FeedbackConfig"/> çağırır): olay → sarsıntı şiddeti / donma süresi.</summary>
    public static class FeedbackRules
    {
        /// <summary>Silahın atış sarsıntısı: listede id'si (ordinal) olan silahın şiddeti, yoksa <paramref name="fallback"/>.</summary>
        public static float GetShotShake(string weaponId, IReadOnlyList<WeaponShake> overrides, float fallback)
        {
            if (weaponId != null && overrides != null)
            {
                for (int i = 0; i < overrides.Count; i++)
                {
                    if (string.Equals(overrides[i].WeaponId, weaponId, StringComparison.Ordinal))
                    {
                        return overrides[i].Strength;
                    }
                }
            }

            return fallback;
        }

        /// <summary>Düşman ağır mı (Tank, boss): id listede var mı (ordinal).</summary>
        public static bool IsHeavy(string enemyId, IReadOnlyList<string> heavyIds)
        {
            if (enemyId == null || heavyIds == null)
            {
                return false;
            }

            for (int i = 0; i < heavyIds.Count; i++)
            {
                if (string.Equals(heavyIds[i], enemyId, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Ölüm sarsıntısı: ağır düşmanda <paramref name="heavyShake"/> (kim öldürürse öldürsün), normalde <paramref name="normalShake"/>;
        /// <paramref name="normalPlayerKillsOnly"/> açıkken normal ölüm sadece oyuncunun öldürmesinde sarsar.
        /// </summary>
        public static float GetKillShake(bool heavy, bool killedByPlayer, float normalShake, float heavyShake, bool normalPlayerKillsOnly)
        {
            if (heavy)
            {
                return heavyShake;
            }

            return normalPlayerKillsOnly && !killedByPlayer ? 0f : normalShake;
        }

        /// <summary>Ölümün donma süresi: sadece ağır düşmanda; <paramref name="playerKillsOnly"/> açıkken sadece oyuncunun öldürmesinde.</summary>
        public static float GetKillHitStop(bool heavy, bool killedByPlayer, float duration, bool playerKillsOnly)
        {
            if (!heavy || (playerKillsOnly && !killedByPlayer))
            {
                return 0f;
            }

            return Math.Max(0f, duration);
        }
    }
}
