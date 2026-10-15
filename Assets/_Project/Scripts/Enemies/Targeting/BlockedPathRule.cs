using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Enemies
{
    /// <summary>
    /// GDD §7 kural 4: asıl hedefe (kural 1–3) giden yol tıkalıysa (<see cref="PathInfo.IsBlocked"/>) yolun sonuna en yakın yapı
    /// hedeflenir. <see cref="TargetSelector"/>'ün parçası; saf C#.
    /// </summary>
    /// <remarks>
    /// Seçilen engel, asıl hedef değişmedikçe ve engel hedeflenebilir kaldıkça korunur: engele giden yol asıl hedefin yolu
    /// olmadığı için her değerlendirmede asıl hedefe dönülseydi düşman ikisi arasında gidip gelirdi. Engel kırılınca asıl hedefe
    /// dönülür; yol hâlâ tıkalıysa (ikinci duvar) sıradaki değerlendirmede yeni engel seçilir.
    /// </remarks>
    public sealed class BlockedPathRule
    {
        // Yolu tıkayabilen yapılar. Tuzak üzerinden geçilir (NavMesh'i kesmez); yolu tıkayan o olamaz.
        private static readonly TargetType[] BlockingTypes = { TargetType.Barricade, TargetType.Turret, TargetType.Generator };

        private ITargetable _blockedTarget;
        private ITargetable _blocker;

        /// <summary>Kural etkinse yolu tıkalı olan asıl hedef; değilse null.</summary>
        public ITargetable BlockedTarget => _blockedTarget;

        /// <summary>Kural etkinse saldırılan engel; değilse null.</summary>
        public ITargetable Blocker => _blocker;

        /// <summary>
        /// Yolun sonuna en yakın (yatay, merkezler arası, mesafe sınırı yok) hedeflenebilir barikat, taret veya jeneratör.
        /// Tuzaklar sayılmaz. Yoksa null.
        /// </summary>
        public static ITargetable SelectBlockingStructure(ITargetRegistry registry, Vector3 pathEnd)
        {
            if (registry == null)
            {
                return null;
            }

            ITargetable nearest = null;
            float best = float.PositiveInfinity;
            for (int i = 0; i < BlockingTypes.Length; i++)
            {
                ITargetable candidate = registry.FindNearest(BlockingTypes[i], pathEnd, best);
                if (candidate == null)
                {
                    continue;
                }

                float distance = TargetDistance.Planar(pathEnd, candidate.Transform.position);
                if (distance < best)
                {
                    best = distance;
                    nearest = candidate;
                }
            }

            return nearest;
        }

        /// <summary>
        /// Asıl hedefe göre seçilecek hedef: engel sürüyorsa engel; <paramref name="current"/> asıl hedefse ve ona giden yol
        /// (<paramref name="path"/>, seçili hedefin yolu) tıkalıysa yeni engel; değilse asıl hedef.
        /// </summary>
        public ITargetable Resolve(ITargetRegistry registry, ITargetable desired, ITargetable current, in PathInfo path)
        {
            if (desired == null)
            {
                Clear();
                return null;
            }

            if (ReferenceEquals(desired, _blockedTarget) && IsAvailable(_blocker))
            {
                return _blocker;
            }

            if (path.IsBlocked && ReferenceEquals(current, desired))
            {
                ITargetable blocker = SelectBlockingStructure(registry, path.End);
                if (blocker != null && !ReferenceEquals(blocker, desired))
                {
                    _blockedTarget = desired;
                    _blocker = blocker;
                    return blocker;
                }
            }

            Clear();
            return desired;
        }

        public void Clear()
        {
            _blockedTarget = null;
            _blocker = null;
        }

        private static bool IsAvailable(ITargetable target)
        {
            return target != null && !(target is Object unityObject && unityObject == null) && target.IsTargetable;
        }
    }
}
