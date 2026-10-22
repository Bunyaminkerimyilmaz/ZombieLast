#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using LastHaven.Core;
using LastHaven.Progression;
using UnityEngine;
using UnityEngine.UIElements;

namespace LastHaven.DebugTools
{
    /// <summary>
    /// Debug paneli düğmelerinin işleri: çoğu mevcut kısayol bileşenine gider (<see cref="DebugEconomyControls"/>,
    /// <see cref="DebugWaveControls"/>, <see cref="DebugSpawner"/>, <see cref="DebugPerkControls"/>, <see cref="DebugStressTest"/>;
    /// eksik bağlantı sessizce atlanır),
    /// zaman ölçeği <see cref="ITimeScaleService"/>'e, ölümsüzlük oyuncunun Health'ine (<see cref="DamageImmunity"/>). Her işten sonra
    /// verilen geri çağrı (panelin durum satırlarını yeniler) çalışır. <see cref="Bind"/> / <see cref="Unbind"/> görünümün ömrüne bağlıdır.
    /// Kısayol bileşenlerinin metotları gibi sadece editörde ve development build'de derlenir.
    /// </summary>
    public sealed class DebugPanelCommands
    {
        private const string Prefix = "[Debug] ";

        private readonly List<KeyValuePair<Button, Action>> _bindings = new List<KeyValuePair<Button, Action>>();
        private readonly List<string> _perkIds = new List<string>();
        private readonly List<string> _perkNames = new List<string>();
        private readonly DamageImmunity _immunity = new DamageImmunity();
        private readonly DebugSpawner _spawner;
        private readonly DebugWaveControls _waves;
        private readonly DebugEconomyControls _economy;
        private readonly DebugPerkControls _perks;
        private readonly DebugStressTest _stress;
        private readonly UnityEngine.Object _logContext;
        private readonly Action _afterAction;
        private ITimeScaleService _timeScale;
        private Health _playerHealth;
        private DebugPanelView _view;

        public DebugPanelCommands(DebugSpawner spawner, DebugWaveControls waves, DebugEconomyControls economy, DebugPerkControls perks,
            DebugStressTest stress, PerkCatalog perkCatalog, UnityEngine.Object logContext, Action afterAction)
        {
            _spawner = spawner;
            _waves = waves;
            _economy = economy;
            _perks = perks;
            _stress = stress;
            _logContext = logContext;
            _afterAction = afterAction;
            CollectPerks(perkCatalog);
        }

        /// <summary>Perk seçim listesinin görünen adları (katalog sırasıyla).</summary>
        public List<string> PerkNames => _perkNames;

        /// <summary>Ölümsüzlük açık mı.</summary>
        public bool IsInvulnerable => _immunity.IsApplied;

        /// <summary>Runtime servisleri (Start'ta). Null olabilir.</summary>
        public void SetServices(ITimeScaleService timeScale, Health playerHealth)
        {
            _timeScale = timeScale;
            _playerHealth = playerHealth;
        }

        public void Bind(DebugPanelView view)
        {
            Unbind();
            _view = view;
            Bind(view.AddMoney, () => { if (_economy != null) _economy.AddMoney(); });
            Bind(view.AddFuel, () => { if (_economy != null) _economy.AddFuel(); });
            Bind(view.DrainFuel, () => { if (_economy != null) _economy.DrainFuel(); });
            Bind(view.SkipNight, () => { if (_waves != null) _waves.SkipNight(); });
            Bind(view.KillAll, () => { if (_waves != null) _waves.KillAllEnemies(); });
            Bind(view.SpawnHorde, () => { if (_spawner != null) _spawner.SpawnAtRandomEdge(); });
            Bind(view.LevelUp, () => { if (_perks != null) _perks.LevelUp(); });
            Bind(view.RandomPerk, () => { if (_perks != null) _perks.GiveRandomPerk(); });
            Bind(view.GivePerk, GiveSelectedPerk);
            Bind(view.StressStart, () => { if (_stress != null) _stress.Begin(); });
            Bind(view.StressStop, () => { if (_stress != null) _stress.End(); });
            view.Invulnerable.RegisterValueChangedCallback(OnInvulnerableChanged);
        }

        public void Unbind()
        {
            for (int i = 0; i < _bindings.Count; i++)
            {
                _bindings[i].Key.clicked -= _bindings[i].Value;
            }

            _bindings.Clear();
            _view?.Invulnerable.UnregisterValueChangedCallback(OnInvulnerableChanged);
            _view = null;
        }

        /// <summary>Ölümsüzlüğü kaldırır (panel yok olurken).</summary>
        public void Dispose()
        {
            Unbind();
            _immunity.Remove();
        }

        /// <summary>1 tabanlı gecenin gündüzüne atlar (<see cref="DebugWaveControls.GoToNight"/>).</summary>
        public void GoToNight(int nightNumber)
        {
            if (_waves != null)
            {
                _waves.GoToNight(nightNumber);
            }

            _afterAction();
        }

        public void SetTimeScale(float scale)
        {
            if (_timeScale == null)
            {
                Debug.LogWarning(Prefix + "Zaman ölçeği: ITimeScaleService yok ([Systems]/Pause).", _logContext);
            }
            else
            {
                _timeScale.SetTimeScale(scale);
            }

            _afterAction();
        }

        private void Bind(Button button, Action action)
        {
            Action handler = () =>
            {
                action();
                _afterAction();
            };
            button.clicked += handler;
            _bindings.Add(new KeyValuePair<Button, Action>(button, handler));
        }

        private void CollectPerks(PerkCatalog catalog)
        {
            if (catalog == null)
            {
                return;
            }

            IReadOnlyList<PerkData> perks = catalog.Perks;
            for (int i = 0; i < perks.Count; i++)
            {
                if (perks[i] != null)
                {
                    _perkIds.Add(perks[i].Id);
                    _perkNames.Add(perks[i].DisplayName);
                }
            }
        }

        private void GiveSelectedPerk()
        {
            int index = _view != null ? _view.PerkDropdown.index : -1;
            if (_perks != null && index >= 0 && index < _perkIds.Count)
            {
                _perks.GivePerk(_perkIds[index]);
            }
        }

        private void OnInvulnerableChanged(ChangeEvent<bool> change)
        {
            if (!change.newValue)
            {
                _immunity.Remove();
                Debug.Log(Prefix + "Ölümsüzlük kapalı.", _logContext);
                return;
            }

            if (_playerHealth == null)
            {
                Debug.LogWarning(Prefix + "Ölümsüzlük: oyuncunun Health'i bulunamadı (IPlayer yok mu?).", _logContext);
                _view?.Invulnerable.SetValueWithoutNotify(false);
                return;
            }

            _immunity.Apply(_playerHealth);
            Debug.Log(Prefix + "Ölümsüzlük açık: oyuncu hasar almıyor.", _logContext);
        }
    }
}
#endif
