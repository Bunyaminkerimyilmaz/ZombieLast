using LastHaven.Core;

namespace LastHaven.Enemies
{
    /// <summary><see cref="TargetSelector"/> ayarları; <see cref="EnemyData.CreateTargetSelectorSettings"/>'ten gelir.</summary>
    public readonly struct TargetSelectorSettings
    {
        public TargetSelectorSettings(float aggroRadius, TargetType? preferredTarget, float preferredTargetRadius, float evaluationInterval)
        {
            AggroRadius = aggroRadius;
            PreferredTarget = preferredTarget;
            PreferredTargetRadius = preferredTargetRadius;
            EvaluationInterval = evaluationInterval;
        }

        /// <summary>Oyuncu bu yarıçap içindeyse (dahil) oyuncu seçilir (m).</summary>
        public float AggroRadius { get; }

        /// <summary>Tipin tercih ettiği hedef türü; yoksa null.</summary>
        public TargetType? PreferredTarget { get; }

        /// <summary>Tercih edilen hedef bu yarıçap içindeyse (dahil) seçilir (m).</summary>
        public float PreferredTargetRadius { get; }

        /// <summary>İki değerlendirme arası süre (sn).</summary>
        public float EvaluationInterval { get; }
    }
}
