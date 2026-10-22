using LastHaven.Core;
using UnityEngine;

namespace LastHaven.GameFlow
{
    /// <summary>
    /// [Systems]/Settings: <see cref="ISettingsService"/> (Awake'te kayıtlı ayarları <see cref="PlayerPrefs"/>'ten yükleyip kaydolur).
    /// Mantık saf <see cref="SettingsModel"/>'de. Değişiklikte <see cref="SettingsChangedEvent"/>; diske ayarlar ekranı kapanınca
    /// (<see cref="Save"/>), sahne kapanırken ve oyundan çıkarken yazılır. Ana menüde ve oyun sahnesinde ayrı örnek; ikisi de aynı
    /// kaydı okur.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SettingsService : MonoBehaviour, ISettingsService
    {
        private const string LogPrefix = "[GameFlow] ";

        [SerializeField, Tooltip("Varsayılan ayarlar (Data/GameFlow/SettingsConfig.asset). Sahne adımı bağlar.")]
        private SettingsConfig _config;

        private SettingsModel _model;

        public GameSettings Current => _model.Current;

        public void Apply(GameSettings settings)
        {
            GameSettings previous = _model.Current;
            if (_model.Apply(settings))
            {
                EventBus<SettingsChangedEvent>.Raise(new SettingsChangedEvent(_model.Current, previous));
            }
        }

        public void Save()
        {
            if (_model != null && _model.Save())
            {
                Debug.Log($"{LogPrefix}Ayarlar kaydedildi ({_model.Current}).", this);
            }
        }

        private void Awake()
        {
            if (_config == null)
            {
                Debug.LogError("SettingsService: SettingsConfig atanmamış; ayarlar yüklenmedi. Tools/Last Haven/Rebuild All çalıştır.", this);
                enabled = false;
                return;
            }

            _model = new SettingsModel(new PlayerPrefsSettingsStore(), _config.Defaults);
            ServiceLocator.Register<ISettingsService>(this);
            Debug.Log($"{LogPrefix}Ayarlar yüklendi ({_model.Current}).", this);
        }

        private void OnApplicationQuit()
        {
            Save();
        }

        private void OnDestroy()
        {
            if (_model == null)
            {
                return;
            }

            Save();
            ServiceLocator.Unregister<ISettingsService>(this);
        }
    }
}
