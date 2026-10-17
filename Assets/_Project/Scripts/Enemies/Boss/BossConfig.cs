using UnityEngine;

namespace LastHaven.Enemies
{
    /// <summary>
    /// Boss davranışının ortak ayarları (GDD §7 Brute): öfke eşiği, öfkeli hız çarpanı ve renk koyulaşması. Asset'i üretici oluşturur
    /// (<c>Data/Enemies/BossConfig.asset</c>); boss prefab'larındaki <see cref="BossBehaviour"/> bağlıdır. Can, hasar, darbe ve geri
    /// itilme direnci her boss'un kendi <see cref="EnemyData"/>'sında.
    /// </summary>
    public sealed class BossConfig : ScriptableObject
    {
        [Header("Öfke")]
        [SerializeField, Range(0f, 1f), Tooltip("Can oranı bunun altına düşünce öfkelenir (bir kez, ölene kadar). GDD §7: 0.5.")]
        private float _enrageHealthFraction = 0.5f;

        [SerializeField, Min(0f), Tooltip("Öfkeliyken yürüme hızı çarpanı (diğer çarpanlarla, ör. yavaşlatma, çarpılır). GDD §7: 1.4.")]
        private float _enragedSpeedMultiplier = 1.4f;

        [SerializeField, Tooltip("Öfkeliyken görselin karıştığı renk (alfa = karışım oranı). Koyu: renk koyulaşır.")]
        private Color _enragedTint = new Color(0.05f, 0f, 0f, 0.6f);

        public float EnrageHealthFraction => _enrageHealthFraction;

        public float EnragedSpeedMultiplier => _enragedSpeedMultiplier;

        public Color EnragedTint => _enragedTint;
    }
}
