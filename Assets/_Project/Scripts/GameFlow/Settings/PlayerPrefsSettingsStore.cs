using UnityEngine;

namespace LastHaven.GameFlow
{
    /// <summary>
    /// Ayarları <see cref="PlayerPrefs"/>'e yazar (Windows'ta kayıt defteri, macOS'ta plist; editörde de aynı yer, ürün adına göre).
    /// <c>Set</c> bellekte kalır; <see cref="Save"/> diske yazar (Unity çıkışta da yazar).
    /// </summary>
    public sealed class PlayerPrefsSettingsStore : ISettingsStore
    {
        public bool TryGetFloat(string key, out float value)
        {
            bool found = PlayerPrefs.HasKey(key);
            value = found ? PlayerPrefs.GetFloat(key) : 0f;
            return found;
        }

        public bool TryGetInt(string key, out int value)
        {
            bool found = PlayerPrefs.HasKey(key);
            value = found ? PlayerPrefs.GetInt(key) : 0;
            return found;
        }

        public void SetFloat(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
        }

        public void SetInt(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
        }

        public void Save()
        {
            PlayerPrefs.Save();
        }
    }
}
