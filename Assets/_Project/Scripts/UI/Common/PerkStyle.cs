using System;
using LastHaven.Core;
using UnityEngine.UIElements;

namespace LastHaven.UI
{
    /// <summary>
    /// Perk'lerin arayüzdeki ortak görünüm kuralları (saf; seviye kartları ve HUD perk listesi): nadirlik USS sınıfı
    /// (<c>blok--common</c> / <c>--rare</c> / <c>--epic</c>; renkler <c>Theme.uss</c>'te) ve ikon yoksa gösterilen baş harfler.
    /// </summary>
    public static class PerkStyle
    {
        /// <summary>Nadirlik sınıf ekleri, <see cref="PerkRarity"/> değeriyle sıralı.</summary>
        private static readonly string[] RarityModifiers = { "common", "rare", "epic" };

        /// <summary>Nadirliğin sınıf eki ("rare"); tanımsız değer "common".</summary>
        public static string RarityModifier(PerkRarity rarity)
        {
            int index = (int)rarity;
            return index >= 0 && index < RarityModifiers.Length ? RarityModifiers[index] : RarityModifiers[0];
        }

        /// <summary>Bloğun nadirlik sınıfı: ("perk-card", Epic) → "perk-card--epic".</summary>
        public static string RarityClass(string block, PerkRarity rarity)
        {
            return block + "--" + RarityModifier(rarity);
        }

        /// <summary>Elementte sadece bu nadirliğin sınıfı kalır (diğer nadirlik sınıfları kaldırılır).</summary>
        public static void SetRarity(VisualElement element, string block, PerkRarity rarity)
        {
            string current = RarityModifier(rarity);
            for (int i = 0; i < RarityModifiers.Length; i++)
            {
                element.EnableInClassList(block + "--" + RarityModifiers[i], RarityModifiers[i] == current);
            }
        }

        /// <summary>
        /// İkon yoksa gösterilen kısa ad (GDD §16 "baş harf"): iki kelime veya fazlası → ilk iki kelimenin baş harfleri ("Şok Taret" →
        /// "ŞT"); tek kelime → ilk iki harf ("Avcı" → "Av"; tek harfli baş harfler aynı nadirlikte çakışmasın: Açgözlü / Adrenalin /
        /// Avcı). Harfler veriden olduğu gibi alınır (büyük/küçük harf dönüşümü yok: tr-TR'de "i" sorunu). Boş ad → "?".
        /// </summary>
        public static string Initials(string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                return "?";
            }

            string[] words = displayName.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length >= 2)
            {
                return new string(new[] { words[0][0], words[1][0] });
            }

            string word = words[0];
            return word.Length >= 2 ? word.Substring(0, 2) : word;
        }

        /// <summary>
        /// İkon alanını doldurur: ikon varsa <paramref name="icon"/>'un arka planı olur ve baş harfler gizlenir; yoksa baş harfler yazılır.
        /// </summary>
        public static void ApplyIcon(VisualElement icon, Label initials, IPerkInfo perk)
        {
            bool hasIcon = perk.Icon != null;
            icon.style.backgroundImage = hasIcon ? new StyleBackground(perk.Icon) : new StyleBackground(StyleKeyword.None);
            initials.text = hasIcon ? string.Empty : Initials(perk.DisplayName);
            initials.SetShown(!hasIcon);
        }
    }
}
