using System;
using System.Collections.Generic;
using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Building
{
    /// <summary>
    /// Jeneratör çevresindeki üs ışıkları (GDD §12: 4 nokta ışık, gece üssü aydınlatır). Gece ve güç varken yanar:
    /// <see cref="PhaseChangedEvent"/> + <see cref="PhaseLighting"/> kuralıyla alacakaranlıkta yanar, şafakta söner;
    /// <see cref="PowerChangedEvent"/> ile güç kesilince söner. Oyun gündüz başlar; güç durumu Start'ta <see cref="IPowerService"/>'ten okunur.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BaseLights : MonoBehaviour
    {
        [SerializeField, Tooltip("Gece ve güçle açılıp kapanan ışıklar. Üretici jeneratör prefab'ında doldurur.")]
        private Light[] _lights = Array.Empty<Light>();

        private bool _isPowered = true;
        private bool _isNight;

        /// <summary>Işıklar şu an açık mı.</summary>
        public bool IsLit { get; private set; } = true;

        public IReadOnlyList<Light> Lights => _lights;

        /// <summary>Tüm ışıkları açar veya kapatır. Sonraki evre/güç değişimi durumu yeniden hesaplar.</summary>
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
                _isPowered = power.IsPowered;
            }
            else
            {
                Debug.LogWarning("BaseLights: IPowerService bulunamadı; ışıklar güç durumunu izlemeyecek.", this);
            }

            Refresh();
        }

        private void OnEnable()
        {
            EventBus<PowerChangedEvent>.Subscribe(OnPowerChanged);
            EventBus<PhaseChangedEvent>.Subscribe(OnPhaseChanged);
        }

        private void OnDisable()
        {
            EventBus<PowerChangedEvent>.Unsubscribe(OnPowerChanged);
            EventBus<PhaseChangedEvent>.Unsubscribe(OnPhaseChanged);
        }

        private void OnPowerChanged(PowerChangedEvent changed)
        {
            _isPowered = changed.IsPowered;
            Refresh();
        }

        private void OnPhaseChanged(PhaseChangedEvent changed)
        {
            LightingTarget target = PhaseLighting.For(changed.Phase);
            if (target == LightingTarget.Unchanged)
            {
                return;
            }

            _isNight = target == LightingTarget.Night;
            Refresh();
        }

        private void Refresh()
        {
            SetLit(_isPowered && _isNight);
        }
    }
}
