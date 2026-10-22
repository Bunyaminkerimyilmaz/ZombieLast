using LastHaven.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace LastHaven.UI
{
    /// <summary>
    /// Dünyadan ekrana yansıtılan uçan yazılar (GDD §16): oyuncunun mermisi düşmana isabet edince hasar sayısı
    /// (<see cref="ShotHitEvent"/>; yüksek hasar daha büyük ve turuncu, aynı karede aynı hedefe gelen saçmalar tek sayı) ve jeton
    /// toplanınca altın "+12" (<see cref="CoinCollectedEvent"/>; arka arkaya toplananlar aynı yazıda toplanır). Etiketler açılışta bir
    /// kez oluşturulur ve yeniden kullanılır (<see cref="FloatingTextLayer"/>; oynanış sırasında element oluşmaz). Kameradan sonra
    /// her kare yerleştirilir; oyun zamanıyla ilerler. HUD'un UIDocument'ını paylaşır.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(HudExecutionOrder.AfterCamera)]
    public sealed class HudFloatingTextController : UIDocumentController
    {
        /// <summary>Her uçan yazı (<c>HudFeel.uss</c>).</summary>
        public const string TextClass = "hud-float-text";

        /// <summary>Tema yazı sınıfı (<c>Theme.uss</c>: boy, kalınlık, gölge).</summary>
        public const string ThemeLabelClass = "label-large";

        public const string DamageClass = "hud-float-text--damage";

        /// <summary>Büyük hasar rengi (<see cref="HudFeelConfig.BigDamage"/> ve üstü).</summary>
        public const string BigDamageClass = "hud-float-text--big";

        public const string CoinClass = "hud-float-text--coin";

        [SerializeField, Tooltip("Ömür, boy, yükselme, havuz boyutu. Üretici bağlar.")]
        private HudFeelConfig _config;

        private FloatingTextLayer _texts;
        private float[] _values;
        private bool[] _isCoin;
        private GameObject[] _targets;
        private ICameraService _camera;
        private int _lastDamageSlot = -1;
        private int _lastDamageFrame = -1;
        private int _lastCoinSlot = -1;

        protected override void BindElements()
        {
            Require<VisualElement>(HudElementNames.FloatingTexts);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (!IsBound)
            {
                return;
            }

            if (_config == null)
            {
                Debug.LogError("HudFloatingTextController: HudFeelConfig atanmamış; Tools/Last Haven/Build Game Scene çalıştır.", this);
                return;
            }

            // UIDocument ağacı her açılışta yeniden kurulur: etiketler de.
            int size = _config.FloatingTextPoolSize;
            _texts = new FloatingTextLayer(Root.Q(HudElementNames.FloatingTexts), size, _config.CreateMotion(), _config.RiseHeight,
                ThemeLabelClass, TextClass);
            _values = new float[size];
            _isCoin = new bool[size];
            _targets = new GameObject[size];
            _lastDamageSlot = -1;
            _lastCoinSlot = -1;
            EventBus<ShotHitEvent>.Subscribe(OnShotHit);
            EventBus<CoinCollectedEvent>.Subscribe(OnCoinCollected);
        }

        private void OnDisable()
        {
            EventBus<ShotHitEvent>.Unsubscribe(OnShotHit);
            EventBus<CoinCollectedEvent>.Unsubscribe(OnCoinCollected);
            _texts = null;
        }

        private void Start()
        {
            if (_texts != null && !ServiceLocator.TryGet(out _camera))
            {
                Debug.LogWarning("HudFloatingTextController: ICameraService bulunamadı; uçan yazılar gösterilmeyecek.", this);
            }
        }

        private void LateUpdate()
        {
            _texts?.Tick(Time.deltaTime, _camera?.Camera);
        }

        private void OnShotHit(ShotHitEvent hit)
        {
            if (hit.TargetType != HitTargetType.Enemy || !(hit.Damage > 0f)
                || (_config.DamagePlayerShotsOnly && (hit.Source == null || !hit.Source.CompareTag(GameTags.Player))))
            {
                return;
            }

            int slot = _lastDamageSlot;
            bool merge = slot >= 0 && _lastDamageFrame == Time.frameCount && _texts.IsActive(slot) && !_isCoin[slot]
                         && _targets[slot] == hit.Target;
            if (merge)
            {
                // Aynı karede aynı hedef (pompalı saçmaları): tek, büyük sayı.
                _values[slot] += hit.Damage;
            }
            else
            {
                slot = Begin(_config.DamageLifetime, false);
                _values[slot] = hit.Damage;
                _targets[slot] = hit.Target;
                Vector2 jitter = Random.insideUnitCircle * _config.DamageJitter;
                _texts.SetAnchor(slot, hit.Point + new Vector3(jitter.x, _config.DamageHeight, jitter.y));
                _lastDamageSlot = slot;
                _lastDamageFrame = Time.frameCount;
            }

            float damage = _values[slot];
            Label label = _texts.GetLabel(slot);
            label.text = NumberText.Count(Mathf.Max(1, Mathf.RoundToInt(damage)));
            label.EnableInClassList(BigDamageClass, damage >= _config.BigDamage);
            _texts.SetSize(slot, _config.GetDamageScale(damage));
        }

        private void OnCoinCollected(CoinCollectedEvent collected)
        {
            if (collected.Value <= 0)
            {
                return;
            }

            int slot = _lastCoinSlot;
            if (slot >= 0 && _texts.IsActive(slot) && _isCoin[slot] && _texts.Age(slot) <= _config.CoinMergeWindow)
            {
                _values[slot] += collected.Value;
                _texts.Restart(slot, _config.CoinLifetime);
            }
            else
            {
                slot = Begin(_config.CoinLifetime, true);
                _values[slot] = collected.Value;
                _lastCoinSlot = slot;
            }

            _texts.SetAnchor(slot, collected.Position + Vector3.up * _config.CoinHeight);
            _texts.GetLabel(slot).text = NumberText.Gain(Mathf.RoundToInt(_values[slot]));
        }

        private int Begin(float lifetime, bool isCoin)
        {
            int slot = _texts.Acquire(lifetime);
            if (slot == _lastDamageSlot)
            {
                _lastDamageSlot = -1;
            }

            if (slot == _lastCoinSlot)
            {
                _lastCoinSlot = -1;
            }

            _isCoin[slot] = isCoin;
            _targets[slot] = null;
            Label label = _texts.GetLabel(slot);
            label.EnableInClassList(CoinClass, isCoin);
            label.EnableInClassList(DamageClass, !isCoin);
            label.EnableInClassList(BigDamageClass, false);
            return slot;
        }
    }
}
