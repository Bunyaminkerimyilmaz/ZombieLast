using System;
using System.Collections.Generic;
using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Building
{
    /// <summary>
    /// Jeneratör çevresindeki üs ışıkları (GDD §12: 4 nokta ışık). Güç varken yanar, <see cref="PowerChangedEvent"/> ile açılıp kapanır.
    /// Başlangıç durumu Start'ta <see cref="IPowerService"/>'ten okunur. Gündüz/gece ayrımı henüz yok (Gün 6).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BaseLights : MonoBehaviour
    {
        [SerializeField, Tooltip("Güçle açılıp kapanan ışıklar. Üretici jeneratör prefab'ında doldurur.")]
        private Light[] _lights = Array.Empty<Light>();

        /// <summary>Işıklar şu an açık mı.</summary>
        public bool IsLit { get; private set; } = true;

        public IReadOnlyList<Light> Lights => _lights;

        /// <summary>Tüm ışıkları açar veya kapatır.</summary>
        public void SetLit(bool lit)
        {
            IsLit = lit;
            for (int i = 0; i < _lights.Length; i++)
            {
                if (_lights[i] != null)
                {
                    _lights[i].enabled = lit;
                }
            }
        }

        private void Start()
        {
            if (ServiceLocator.TryGet(out IPowerService power))
            {
                SetLit(power.IsPowered);
            }
            else
            {
                Debug.LogWarning("BaseLights: IPowerService bulunamadı; ışıklar güç durumunu izlemeyecek.", this);
            }
        }

        private void OnEnable()
        {
            EventBus<PowerChangedEvent>.Subscribe(OnPowerChanged);
        }

        private void OnDisable()
        {
            EventBus<PowerChangedEvent>.Unsubscribe(OnPowerChanged);
        }

        private void OnPowerChanged(PowerChangedEvent changed)
        {
            SetLit(changed.IsPowered);
        }
    }
}
