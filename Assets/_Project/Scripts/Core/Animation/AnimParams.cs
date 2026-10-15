using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>
    /// <see cref="AnimationDriver"/>'ın Animator'a yazdığı standart parametreler. Animator Controller'larda bu adlar kullanılır;
    /// controller'da olmayan parametre sessizce atlanır.
    /// </summary>
    public static class AnimParams
    {
        /// <summary>float: yatay hız (m/sn). Locomotion blend tree'si (Idle/Walk/Run) bunu kullanır.</summary>
        public const string Speed = "Speed";

        /// <summary>bool: oyuncu atılıyor mu.</summary>
        public const string IsDashing = "IsDashing";

        /// <summary>trigger: yakın dövüş saldırısı.</summary>
        public const string Attack = "Attack";

        /// <summary>trigger: hasar aldı (ölmeden).</summary>
        public const string Hit = "Hit";

        /// <summary>trigger: öldü.</summary>
        public const string Die = "Die";

        /// <summary>trigger: ateş etti.</summary>
        public const string Fire = "Fire";

        /// <summary>trigger: şarjör doldurmaya başladı.</summary>
        public const string Reload = "Reload";

        public static readonly int SpeedId = Animator.StringToHash(Speed);
        public static readonly int IsDashingId = Animator.StringToHash(IsDashing);
        public static readonly int AttackId = Animator.StringToHash(Attack);
        public static readonly int HitId = Animator.StringToHash(Hit);
        public static readonly int DieId = Animator.StringToHash(Die);
        public static readonly int FireId = Animator.StringToHash(Fire);
        public static readonly int ReloadId = Animator.StringToHash(Reload);
    }
}
