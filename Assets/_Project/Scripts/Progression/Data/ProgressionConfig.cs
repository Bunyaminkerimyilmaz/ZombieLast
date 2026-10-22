using System.Collections.Generic;
using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Progression
{
    /// <summary>
    /// Seviye ve perk ayarları (GDD §13). Asset'i üretici oluşturur: <c>Data/Progression/ProgressionConfig.asset</c>.
    /// Eşik alanlarını <see cref="ExperienceService"/>, nadirlik ağırlıklarını ve kart ayarlarını <see cref="PerkService"/> kullanır.
    /// </summary>
    public sealed class ProgressionConfig : ScriptableObject
    {
        [Header("Seviye eşiği (GDD §13)")]
        [SerializeField, Min(1), Tooltip("Sonraki seviye için gereken XP = taban + seviye başına × mevcut seviye. Taban.")]
        private int _baseXp = 10;

        [SerializeField, Min(0), Tooltip("Sonraki seviye için gereken XP'ye mevcut seviye başına eklenen miktar.")]
        private int _xpPerLevel = 6;

        [SerializeField, Min(1), Tooltip("Run başındaki seviye.")]
        private int _startingLevel = 1;

        [Header("XP kaynağı (GDD §7, §13)")]
        [SerializeField, Tooltip("Açıksa sadece oyuncunun öldürmeleri XP verir. Kapalıyken (varsayılan) taret, tuzak ve patlama " +
                                 "öldürmeleri de verir (GDD: düşman öldürünce otomatik kazanılır).")]
        private bool _playerKillsOnly;

        [Header("Perk nadirliği (GDD §13)")]
        [SerializeField, Tooltip("Perk kartı çekilişinde nadirlik ağırlıkları. Listede olmayan nadirlik 0 sayılır.")]
        private List<RarityWeight> _rarityWeights = new List<RarityWeight>
        {
            new RarityWeight(PerkRarity.Common, 60),
            new RarityWeight(PerkRarity.Rare, 30),
            new RarityWeight(PerkRarity.Epic, 10),
        };

        [Header("Perk kartları (GDD §13)")]
        [SerializeField, Min(1), Tooltip("Seviye atlayınca sunulan farklı perk kartı sayısı.")]
        private int _offerSize = 3;

        [SerializeField, Tooltip("Perk çekilişinin tohumu. 0 = her run farklı; başka bir değer her run aynı sırayı verir (test, denge).")]
        private int _offerSeed;

        public int BaseXp => _baseXp;

        public int XpPerLevel => _xpPerLevel;

        public int StartingLevel => _startingLevel;

        public bool PlayerKillsOnly => _playerKillsOnly;

        public IReadOnlyList<RarityWeight> RarityWeights => _rarityWeights;

        public int OfferSize => Mathf.Max(1, _offerSize);

        /// <summary>0 = her run farklı tohum.</summary>
        public int OfferSeed => _offerSeed;

        /// <summary>Nadirliğin ağırlığı; listede yoksa 0 (aynı nadirlik iki kez varsa ilki).</summary>
        public int GetRarityWeight(PerkRarity rarity)
        {
            if (_rarityWeights == null)
            {
                return 0;
            }

            for (int i = 0; i < _rarityWeights.Count; i++)
            {
                if (_rarityWeights[i].Rarity == rarity)
                {
                    return Mathf.Max(0, _rarityWeights[i].Weight);
                }
            }

            return 0;
        }

        /// <summary>Seviye eşiği formülü (o anki değerlerle).</summary>
        public LevelCurve CreateLevelCurve()
        {
            return new LevelCurve(_baseXp, _xpPerLevel);
        }

        /// <summary>Run başı XP takipçisi (o anki eşik ve başlangıç seviyesiyle).</summary>
        public ExperienceTracker CreateExperienceTracker()
        {
            return new ExperienceTracker(CreateLevelCurve(), _startingLevel);
        }
    }
}
