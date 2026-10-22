using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LastHaven.Core
{
    /// <summary>
    /// [Systems]/Pause: <see cref="IPauseService"/> ve <see cref="IHitStopService"/> (Awake'te kaydolur); <c>Time.timeScale</c>'in tek
    /// sahibi. Kaynaklar saf <see cref="PauseSources"/>'ta, vuruş donması saf <see cref="HitStopTimer"/>'da. Oyun ilk dondurulduğunda
    /// (ilk kaynak ya da donma) o anki ölçeği saklayıp 0 yapar; son kaynak çıkınca / donma bitince saklananı geri yazar (0 saklanmaz,
    /// 1 sayılır). Duraklatma donmayı yutar (donma iptal, ölçek duraklatma kalkınca döner); duraklıyken donma istenmez.
    /// Yok edilen Unity nesnesi kaynakların duraklatması her karede kontrol edilip kaldırılır. Bileşen yok olurken (sahne yeniden
    /// yükleme, Play'den çıkış) oyun donuksa zaman ölçeği geri yazılır.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PauseService : MonoBehaviour, IPauseService, IHitStopService
    {
        private const string LogPrefix = "[Core] ";

        private static readonly Predicate<object> IsDestroyedUnityObject = source => source is Object unityObject && unityObject == null;

        private readonly PauseSources _sources = new PauseSources();
        private readonly HitStopTimer _hitStop = new HitStopTimer();
        private float _resumeTimeScale = 1f;

        public bool IsPaused => _sources.IsPaused;

        public bool IsHitStopped => _hitStop.IsActive;

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
        }

        private void Update()
        {
            Advance(Time.unscaledDeltaTime);
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<IPauseService>(this);
            ServiceLocator.Unregister<IHitStopService>(this);
            if (_sources.IsPaused || _hitStop.IsActive)
            {
                _sources.Clear();
                _hitStop.Cancel();
                Time.timeScale = _resumeTimeScale;
            }
        }

        private void SaveTimeScale()
        {
            _resumeTimeScale = Time.timeScale > 0f ? Time.timeScale : 1f;
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
