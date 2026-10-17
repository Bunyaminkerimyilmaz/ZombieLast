using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>
    /// <c>Visual</c> altındaki tüm Renderer'ların rengini MaterialPropertyBlock ile yönetir (<c>_BaseColor</c> ve <c>_EmissionColor</c>;
    /// materyalde olmayan özellik, hiçbiri yoksa renderer atlanır). Materyallere dokunulmaz. İki katman:
    /// <list type="bullet">
    /// <item>Vuruş flaşı (<see cref="Flash()"/>): kısa süre tek renk. Renk ve süre prefab'da (düşman beyaz, oyuncu kırmızı).</item>
    /// <item>Kalıcı renk tonu (<see cref="SetTint(TintLayer, Color)"/>, 9.3): katman başına bir ton, alttan üste karışır
    /// (<see cref="TintLayer.Base"/>: boss öfkesi koyulaşması, <see cref="TintLayer.Status"/>: yavaşlamış düşman mavi). Flaş sürerken
    /// flaş görünür, bitince ton geri gelir. Ton yoksa flaş bitince blok temizlenir.</item>
    /// </list>
    /// Aynı renderer'lara iki ayrı bileşen blok yazarsa birbirini silerdi; bu yüzden ikisi de burada.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HitFlash : MonoBehaviour, IPoolable
    {
        [SerializeField, Tooltip("Görsel kök. Boşsa kökün 'Visual' adlı çocuğu kullanılır.")]
        private Transform _visual;

        [SerializeField, Tooltip("Flaş rengi.")]
        private Color _color = Color.white;

        [SerializeField, Min(0f), Tooltip("Flaş süresi (sn). GDD §7: düşmanda 0.08.")]
        private float _duration = 0.08f;

        private readonly Countdown _timer = new Countdown();
        private readonly TintLayers _tints = new TintLayers();
        private FlashTargetSet _targets;

        public Color Color => _color;

        public float Duration => _duration;

        public bool IsFlashing => _timer.IsRunning;

        /// <summary>Herhangi bir kalıcı renk tonu katmanı açık mı.</summary>
        public bool HasTint => _tints.Any;

        public bool HasTintOn(TintLayer layer) => _tints.IsActive(layer);

        /// <summary>Flaşın uygulanabildiği renderer sayısı.</summary>
        public int TargetCount
        {
            get
            {
                EnsureInitialized();
                return _targets.Count;
            }
        }

        /// <summary>
        /// Tonlanmış renk: <paramref name="original"/>'ın RGB'si <paramref name="tint"/>'in RGB'sine tonun alfası oranında karışır
        /// (0 = değişmez, 1 = tamamen ton). Alfa (saydamlık) korunur.
        /// </summary>
        public static Color Tint(Color original, Color tint)
        {
            Color mixed = Color.Lerp(original, tint, Mathf.Clamp01(tint.a));
            mixed.a = original.a;
            return mixed;
        }

        /// <summary>Prefab'daki renk ve süreyle flaş. Flaş sürerken çağrılırsa süre baştan başlar.</summary>
        public void Flash()
        {
            Flash(_color, _duration);
        }

        public void Flash(Color color, float duration)
        {
            EnsureInitialized();
            if (_targets.Count == 0 || duration <= 0f)
            {
                return;
            }

            _targets.Apply(color);
            _timer.Start(duration);
        }

        /// <summary>Flaşı hemen bitirir; ton varsa tona, yoksa materyalin kendi rengine döner.</summary>
        public void Stop()
        {
            _timer.Stop();
            RestoreColors();
        }

        /// <summary>Durum tonu (<see cref="TintLayer.Status"/>).</summary>
        public void SetTint(Color tint) => SetTint(TintLayer.Status, tint);

        /// <summary>Durum tonunu kaldırır (<see cref="TintLayer.Status"/>).</summary>
        public void ClearTint() => ClearTint(TintLayer.Status);

        /// <summary>
        /// Kalıcı ton: <c>_BaseColor</c> materyalin rengiyle <paramref name="tint"/> arasında karışır (alfa = karışım oranı; bkz.
        /// <see cref="Tint(Color, Color)"/>); diğer katman açıksa üst üste. Flaş sürüyorsa flaş bitince görünür. Katman
        /// <see cref="ClearTint(TintLayer)"/>'e veya havuza dönüşe kadar kalır.
        /// </summary>
        public void SetTint(TintLayer layer, Color tint)
        {
            EnsureInitialized();
            _tints.Set(layer, tint);
            if (!_timer.IsRunning)
            {
                RestoreColors();
            }
        }

        /// <summary>Katmanın tonunu kaldırır; flaş sürmüyorsa renkler hemen güncellenir.</summary>
        public void ClearTint(TintLayer layer)
        {
            if (_tints.Clear(layer) && !_timer.IsRunning)
            {
                RestoreColors();
            }
        }

        public void OnSpawned()
        {
            ResetColors();
        }

        public void OnDespawned()
        {
            ResetColors();
        }

        /// <summary>Süreyi ilerletir; bitince renkleri geri verir. Update'ten çağrılır; testler doğrudan çağırır.</summary>
        internal void Advance(float deltaTime)
        {
            if (_timer.Tick(deltaTime))
            {
                Stop();
            }
        }

        private void Awake()
        {
            EnsureInitialized();
        }

        private void OnDisable()
        {
            ResetColors();
        }

        private void Update()
        {
            if (_timer.IsRunning)
            {
                Advance(Time.deltaTime);
            }
        }

        // Flaş ve ton yok: renderer'lar materyalin kendi rengine döner (havuz, kapanma).
        private void ResetColors()
        {
            _tints.ClearAll();
            Stop();
        }

        private void RestoreColors()
        {
            if (_targets == null)
            {
                return;
            }

            if (_tints.Any)
            {
                _targets.ApplyTint(_tints);
            }
            else
            {
                _targets.Clear();
            }
        }

        // Tembel kurulum: EditMode testlerinde (Awake yokken) de çalışır.
        private void EnsureInitialized()
        {
            if (_targets != null)
            {
                return;
            }

            if (_visual == null)
            {
                _visual = VisualRoot.Find(transform);
            }

            if (_visual == null)
            {
                Debug.LogWarning($"HitFlash: '{name}' altında '{VisualRoot.Name}' bulunamadı; flaş olmayacak.", this);
                _targets = FlashTargetSet.Empty;
                return;
            }

            _targets = FlashTargetSet.Create(_visual);
        }
    }
}
