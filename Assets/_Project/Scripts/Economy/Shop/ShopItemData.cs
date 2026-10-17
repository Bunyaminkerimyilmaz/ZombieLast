using System.Collections.Generic;
using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Economy
{
    /// <summary>
    /// Market öğesi (GDD §6, §10). Asset'leri üretici oluşturur: <c>Data/Shop/Items/</c>; markete <see cref="ShopCatalog"/> ile girer.
    /// Silahlara ve diğer modüllere id ile başvurur (<see cref="TargetId"/>); Economy Player/Combat'ı bilmez.
    /// </summary>
    public sealed class ShopItemData : ScriptableObject, IShopItem
    {
        [Header("Kimlik")]
        [SerializeField, Tooltip("Benzersiz id (ordinal karşılaştırılır). Seviye takibi ve olaylar bu id'yi kullanır. Değiştirme.")]
        private string _id;

        [SerializeField, Tooltip("Görünen ad (Türkçe).")]
        private string _displayName;

        [SerializeField, TextArea, Tooltip("Kısa açıklama (market kartı).")]
        private string _description;

        [SerializeField, Tooltip("Market sekmesi.")]
        private ShopCategory _category = ShopCategory.Supplies;

        [Header("Fiyat ve seviye")]
        [SerializeField, Min(0), Tooltip("Seviye başına fiyat: [0] ilk alım, [1] ikinci... Liste bitince son fiyat geçerli (sarf malzemesi).")]
        private List<int> _prices = new List<int>();

        [SerializeField, Min(0), Tooltip("En yüksek seviye; 0 = sınırsız (sarf malzemesi). Seviyeli öğede fiyat sayısı kadar olmalı.")]
        private int _maxLevel = 1;

        [SerializeField, Tooltip("Satın alınabilmesi için en az bir kez alınmış olması gereken öğenin id'si (ör. pompalı " +
                                 "yükseltmesi → pompalı). Boş = koşulsuz.")]
        private string _requiredItemId;

        [Header("Etki")]
        [SerializeField, Tooltip("Satın alınca ne olur.")]
        private ShopEffectType _effect = ShopEffectType.BuyFuel;

        [SerializeField, Tooltip("Silah açma, yükseltme ve mermide silahın id'si (WeaponData.Id). Diğerlerinde boş.")]
        private string _targetId;

        [SerializeField, Tooltip("Sadece silah yükseltmesinde: hangi yükseltme. Seviye başına etkisi WeaponUpgradeConfig'te.")]
        private WeaponUpgradeType _upgradeType;

        [SerializeField, Min(0f), Tooltip("Miktar: mermi adedi, can, yakıt birimi, toplama yarıçapı (m). Açma ve yükseltmede kullanılmaz.")]
        private float _value;

        public string Id => _id;

        public string DisplayName => _displayName;

        public string Description => _description;

        public ShopCategory Category => _category;

        public int MaxLevel => _maxLevel;

        public bool IsRepeatable => _maxLevel <= 0;

        public IReadOnlyList<int> Prices => _prices;

        public ShopEffectType Effect => _effect;

        public string TargetId => _targetId;

        public WeaponUpgradeType UpgradeType => _upgradeType;

        public float Value => _value;

        public string RequiredItemId => _requiredItemId;

        public int GetPrice(int level)
        {
            if (_prices.Count == 0)
            {
                return 0;
            }

            int index = Mathf.Clamp(level, 0, _prices.Count - 1);
            return Mathf.Max(0, _prices[index]);
        }
    }
}
