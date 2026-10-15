using System;
using System.Collections.Generic;
using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>
    /// Vuruş flaşı: <c>Visual</c> altındaki tüm Renderer'ların rengini kısa süre MaterialPropertyBlock ile değiştirir
    /// (<c>_BaseColor</c> ve <c>_EmissionColor</c>; materyalde olmayan özellik, hiçbiri yoksa renderer atlanır).
    /// Süre bitince blok temizlenir; materyallere dokunulmaz. Renk ve süre prefab'da (düşman beyaz, oyuncu kırmızı).
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
        private MaterialPropertyBlock _block;
        private FlashTarget[] _targets;

        public Color Color => _color;

        public float Duration => _duration;

        public bool IsFlashing => _timer.IsRunning;

        /// <summary>Flaşın uygulanabildiği renderer sayısı.</summary>
        public int TargetCount
        {
            get
            {
                EnsureInitialized();
                return _targets.Length;
            }
        }

        /// <summary>Prefab'daki renk ve süreyle flaş. Flaş sürerken çağrılırsa süre baştan başlar.</summary>
        public void Flash()
        {
            Flash(_color, _duration);
        }

        public void Flash(Color color, float duration)
        {
            EnsureInitialized();
            if (_targets.Length == 0 || duration <= 0f)
            {
                return;
            }

            for (int i = 0; i < _targets.Length; i++)
            {
                _targets[i].Apply(_block, color);
            }

            _timer.Start(duration);
        }

        /// <summary>Flaşı hemen bitirir ve renkleri geri verir.</summary>
        public void Stop()
        {
            _timer.Stop();
            if (_targets == null)
            {
                return;
            }

            for (int i = 0; i < _targets.Length; i++)
            {
                _targets[i].Clear();
            }
        }

        public void OnSpawned()
        {
            Stop();
        }

        public void OnDespawned()
        {
            Stop();
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
            Stop();
        }

        private void Update()
        {
            if (_timer.IsRunning)
            {
                Advance(Time.deltaTime);
            }
        }

        // Tembel kurulum: EditMode testlerinde (Awake yokken) de çalışır.
        private void EnsureInitialized()
        {
            if (_targets != null)
            {
                return;
            }

            _block = new MaterialPropertyBlock();
            if (_visual == null)
            {
                _visual = VisualRoot.Find(transform);
            }

            if (_visual == null)
            {
                Debug.LogWarning($"HitFlash: '{name}' altında '{VisualRoot.Name}' bulunamadı; flaş olmayacak.", this);
                _targets = Array.Empty<FlashTarget>();
                return;
            }

            Renderer[] renderers = _visual.GetComponentsInChildren<Renderer>(true);
            var targets = new List<FlashTarget>(renderers.Length);
            for (int i = 0; i < renderers.Length; i++)
            {
                if (FlashTarget.TryCreate(renderers[i], out FlashTarget target))
                {
                    targets.Add(target);
                }
            }

            _targets = targets.ToArray();
        }
    }
}
