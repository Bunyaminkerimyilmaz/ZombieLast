using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>
    /// <c>Visual</c> altındaki tüm Renderer'ların rengini MaterialPropertyBlock ile yönetir (<c>_BaseColor</c> ve <c>_EmissionColor</c>;
    /// materyalde olmayan özellik, hiçbiri yoksa renderer atlanır). Materyallere dokunulmaz. İki katman:
    /// <list type="bullet">
    /// <item>Vuruş flaşı (<see cref="Flash()"/>): kısa süre tek renk. Renk ve süre prefab'da (düşman beyaz, oyuncu kırmızı).</item>
    /// <item>Kalıcı renk tonu (<see cref="SetTint"/>, 9.3): durum efektleri (ör. yavaşlamış düşman mavi). Flaş sürerken flaş görünür,
    /// bitince ton geri gelir. Ton yoksa flaş bitince blok temizlenir.</item>
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
        private FlashTargetSet _targets;
        private Color _tint;
        private bool _hasTint;

        public Color Color => _color;

        public float Duration => _duration;

        public bool IsFlashing => _timer.IsRunning;

        /// <summary>Kalıcı renk tonu açık mı.</summary>
        public bool HasTint => _hasTint;

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

        /// <summary>
        /// Kalıcı ton: <c>_BaseColor</c> materyalin rengiyle <paramref name="tint"/> arasında karışır (alfa = karışım oranı; bkz.
        /// <see cref="Tint(Color, Color)"/>). Flaş sürüyorsa flaş bitince görünür. <see cref="ClearTint"/>'e veya havuza dönüşe kadar kalır.
        /// </summary>
        public void SetTint(Color tint)
        {
            EnsureInitialized();
            _tint = tint;
            _hasTint = true;
            if (!_timer.IsRunning)
            {
                RestoreColors();
            }
        }

        /// <summary>Tonu kaldırır; flaş sürmüyorsa renkler hemen materyale döner.</summary>
        public void ClearTint()
        {
            if (!_hasTint)
            {
                return;
            }

            _hasTint = false;
            if (!_timer.IsRunning)
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
            _hasTint = false;
            Stop();
        }

        private void RestoreColors()
        {
            if (_targets == null)
            {
                return;
            }

            if (_hasTint)
            {
                _targets.ApplyTint(_tint);
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
