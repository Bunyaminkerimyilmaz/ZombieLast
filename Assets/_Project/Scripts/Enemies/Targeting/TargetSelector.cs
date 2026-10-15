using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Enemies
{
    /// <summary>
    /// Düşmanın hedef seçimi (GDD §7 kural 1–4). Adayları <see cref="ITargetRegistry"/>'den alır ve
    /// her frame değil, <see cref="TargetSelectorSettings.EvaluationInterval"/>'da bir değerlendirir. Kural 4 (yol tıkalı →
    /// yolun sonuna en yakın yapı) <see cref="BlockedPathRule"/>'da.
    /// Saf C#; <see cref="Enemy"/> her frame <see cref="Tick(float, Vector3, in PathInfo)"/> çağırır.
    /// </summary>
    public sealed class TargetSelector
    {
        private readonly ITargetRegistry _registry;
        private readonly BlockedPathRule _blockedPath = new BlockedPathRule();
        private TargetSelectorSettings _settings;
        private float _timer;

        /// <param name="registry">Null olabilir; o zaman hiç hedef seçilmez.</param>
        public TargetSelector(ITargetRegistry registry, TargetSelectorSettings settings)
        {
            _registry = registry;
            _settings = settings;
        }

        /// <summary>Son değerlendirmede seçilen hedef; yoksa null. Bu arada ölmüş veya yok edilmiş olabilir.</summary>
        public ITargetable Current { get; private set; }

        /// <summary>Kural 4 etkinse: yolu tıkalı olan asıl hedef (kural 1–3); değilse null. Seçili hedef o zaman engeldir.</summary>
        public ITargetable BlockedTarget => _blockedPath.BlockedTarget;

        /// <summary>Bir sonraki düzenli değerlendirmeye kalan süre (sn).</summary>
        public float TimeUntilEvaluation => _timer;

        public TargetSelectorSettings Settings => _settings;

        /// <summary>
        /// GDD §7 kuralları: 1) oyuncu aggro yarıçapındaysa oyuncu, 2) değilse tercih edilen tür yarıçapındaysa o,
        /// 3) değilse en yakın jeneratör. Hiçbiri yoksa null. Mesafeler yatay (XZ) ölçülür; sınır dahildir.
        /// </summary>
        public static ITargetable Select(ITargetRegistry registry, in TargetSelectorSettings settings, Vector3 position)
        {
            if (registry == null)
            {
                return null;
            }

            ITargetable player = registry.FindNearest(TargetType.Player, position, settings.AggroRadius);
            if (player != null)
            {
                return player;
            }

            if (settings.PreferredTarget.HasValue)
            {
                ITargetable preferred = registry.FindNearest(settings.PreferredTarget.Value, position, settings.PreferredTargetRadius);
                if (preferred != null)
                {
                    return preferred;
                }
            }

            return registry.FindNearest(TargetType.Generator, position, float.PositiveInfinity);
        }

        /// <summary>
        /// Hedefi siler ve ayarları yeniler (havuzdan çıkışta). İlk değerlendirme <paramref name="initialDelay"/> sn sonra olur;
        /// düşmanlar bunu rastgele vererek aynı frame'de değerlendirme yapmaz.
        /// </summary>
        public void Reset(in TargetSelectorSettings settings, float initialDelay)
        {
            _settings = settings;
            _timer = Mathf.Max(0f, initialDelay);
            Current = null;
            _blockedPath.Clear();
        }

        /// <summary>Zamanlayıcıyı bekletmeden hemen değerlendirir (yol bilgisi yok: yeni engel seçilmez, süren engel korunur).</summary>
        public ITargetable Evaluate(Vector3 position)
        {
            return Evaluate(position, PathInfo.None);
        }

        /// <summary>
        /// Zamanlayıcıyı bekletmeden hemen değerlendirir. <paramref name="path"/> şu anki yolun (seçili hedefe giden) durumudur;
        /// kural 4'e sadece seçili hedef asıl hedefse (kural 1–3) bakılır.
        /// </summary>
        public ITargetable Evaluate(Vector3 position, in PathInfo path)
        {
            ITargetable desired = Select(_registry, _settings, position);
            Current = _blockedPath.Resolve(_registry, desired, Current, path);
            return Current;
        }

        /// <summary>Süreyi ilerletir; süre dolduysa değerlendirir (yol bilgisi yok). Değerlendirdiyse true döner.</summary>
        public bool Tick(float deltaTime, Vector3 position)
        {
            return Tick(deltaTime, position, PathInfo.None);
        }

        /// <summary>
        /// Süreyi ilerletir; süre dolduysa değerlendirir. Seçili hedef bu arada hedeflenemez olduysa (öldü, yok edildi)
        /// beklemeden yeniden değerlendirir; düzenli aralığın fazı değişmez. Değerlendirdiyse true döner.
        /// </summary>
        public bool Tick(float deltaTime, Vector3 position, in PathInfo path)
        {
            _timer -= deltaTime;
            bool due = _timer <= 0f;
            if (due)
            {
                // Faz korunur; uzun bir takılmada kaçırılan değerlendirmeler telafi edilmez.
                _timer = Mathf.Max(_timer + _settings.EvaluationInterval, 0f);
            }

            if (!due && !IsLost(Current))
            {
                return false;
            }

            Evaluate(position, path);
            return true;
        }

        /// <summary>Hedef seçilmişti ama artık hedeflenemiyor mu.</summary>
        private static bool IsLost(ITargetable target)
        {
            if (target == null)
            {
                return false;
            }

            return (target is Object unityObject && unityObject == null) || !target.IsTargetable;
        }
    }
}
