using System;
using UnityEngine;

namespace LastHaven.Progression
{
    /// <summary>
    /// Oyuncuyu iyileştirir (GDD §13 Kalın Deri: "ve o kadar iyileştirir"). Maks. can etkisinden sonra listelenmeli: önce maksimum
    /// artar, sonra can dolar. Oyuncu ölüyse etkisiz.
    /// </summary>
    [Serializable]
    public sealed class HealEffect : PerkEffect
    {
        [SerializeField, Min(0f), Tooltip("İyileşen can (maksimumda kırpılır).")]
        private float _amount;

        public HealEffect()
        {
        }

        public HealEffect(float amount)
        {
            _amount = amount;
        }

        public float Amount => _amount;

        public override void Apply(in PerkEffectContext context)
        {
            if (_amount > 0f)
            {
                context.Heal?.Invoke(_amount);
            }
        }
    }
}
