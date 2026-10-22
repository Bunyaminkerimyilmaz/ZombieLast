using System;
using System.Globalization;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LastHaven.Core
{
    /// <summary>
    /// [Systems]/Pause: <see cref="IPauseService"/>, <see cref="IHitStopService"/> ve <see cref="ITimeScaleService"/> (Awake'te
    /// kaydolur); <c>Time.timeScale</c>'in tek sahibi. Kaynaklar saf <see cref="PauseSources"/>'ta, vuruş donması saf
    /// <see cref="HitStopTimer"/>'da. Oyun ilk dondurulduğunda (ilk kaynak ya da donma) o anki ölçeği saklayıp 0 yapar; son kaynak
    /// çıkınca / donma bitince saklananı geri yazar (0 saklanmaz, 1 sayılır). Duraklatma donmayı yutar (donma iptal, ölçek duraklatma
    /// kalkınca döner); duraklıyken donma istenmez. Normal ölçek (debug hızı) donukken değişirse saklanan değer güncellenir.
    /// Yok edilen Unity nesnesi kaynakların duraklatması her karede kontrol edilip kaldırılır. Bileşen yok olurken (sahne yeniden
    /// yükleme, Play'den çıkış) zaman ölçeği 1'e döner: duraklatma veya debug hızı sonraki sahneye taşınmaz.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PauseService : MonoBehaviour, IPauseService, IHitStopService, ITimeScaleService
    {
        private const string LogPrefix = "[Core] ";
        private const float NormalTimeScale = 1f;

        private static readonly Predicate<object> IsDestroyedUnityObject = source => source is Object unityObject && unityObject == null;

        private readonly PauseSources _sources = new PauseSources();
        private readonly HitStopTimer _hitStop = new HitStopTimer();
        private float _resumeTimeScale = NormalTimeScale;

        public bool IsPaused => _sources.IsPaused;

        public bool IsHitStopped => _hitStop.IsActive;

        /// <inheritdoc />
        public float TimeScale => IsFrozen ? _resumeTimeScale : Time.timeScale;

        private bool IsFrozen => _sources.IsPaused || _hitStop.IsActive;

        /// <inheritdoc />
        public void SetTimeScale(float scale)
        {
            if (!(scale > 0f))
            {
                Debug.LogWarning($"{LogPrefix}Zaman ölçeği pozitif olmalı ({scale}); değişmedi.", this);
                return;
            }

            // Donukken ölçek 0 kalır; devam edince yeni değer yazılır.
            _resumeTimeScale = scale;
            if (!IsFrozen)
            {
                Time.timeScale = scale;
            }

            Debug.Log($"{LogPrefix}Zaman ölçeği ×{scale.ToString("0.##", CultureInfo.InvariantCulture)}.", this);
        }

        public void Pause(object source)
        {
            if (!_sources.Add(source))
            {
                return;
            }

            // Donma sürüyorsa ölçek zaten 0: saklanan (donma öncesi) ölçek korunur.
            if (_hitStop.IsActive)
            {
                _hitStop.Cancel();
            }
            else
            {
                SaveTimeScale();
            }

            Time.timeScale = 0f;
            Debug.Log($"{LogPrefix}Oyun duraklatıldı ({DescribeSource(source)}).", this);
        }

        public void Resume(object source)
        {
            if (_sources.Remove(source))
            {
                RestoreTimeScale(DescribeSource(source));
            }
        }

        public bool IsPausedBy(object source)
        {
            return _sources.Contains(source);
        }

        public void HitStop(float duration)
        {
            if (_sources.IsPaused)
            {
                return;
            }

            if (_hitStop.Request(duration))
            {
                SaveTimeScale();
                Time.timeScale = 0f;
            }
        }

        /// <summary>Kare ilerlemesi (Update gerçek zamanla çağırır; testler doğrudan).</summary>
        internal void Advance(float unscaledDeltaTime)
        {
            if (_sources.RemoveWhere(IsDestroyedUnityObject))
            {
                RestoreTimeScale("kaynak yok edildi");
            }

            if (_hitStop.Tick(unscaledDeltaTime))
            {
                Time.timeScale = _resumeTimeScale;
            }
        }

        private void Awake()
        {
            ServiceLocator.Register<IPauseService>(this);
            ServiceLocator.Register<IHitStopService>(this);
            ServiceLocator.Register<ITimeScaleService>(this);
        }

        private void Update()
        {
            Advance(Time.unscaledDeltaTime);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<IPauseService>(this);
            ServiceLocator.Unregister<IHitStopService>(this);
            ServiceLocator.Unregister<ITimeScaleService>(this);
            _sources.Clear();
            _hitStop.Cancel();
            Time.timeScale = NormalTimeScale;
        }

        private void SaveTimeScale()
        {
            _resumeTimeScale = Time.timeScale > 0f ? Time.timeScale : NormalTimeScale;
        }

        private void RestoreTimeScale(string reason)
        {
            Time.timeScale = _resumeTimeScale;
            Debug.Log($"{LogPrefix}Oyun devam ediyor ({reason}).", this);
        }

        private static string DescribeSource(object source)
        {
            return source is Object unityObject && unityObject != null ? unityObject.name + " (" + source.GetType().Name + ")"
                : source?.GetType().Name ?? "null";
        }
    }
}
