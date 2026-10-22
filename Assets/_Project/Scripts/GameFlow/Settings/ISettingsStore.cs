namespace LastHaven.GameFlow
{
    /// <summary>Ayarların anahtar-değer kaydı (oyunda <see cref="PlayerPrefsSettingsStore"/>; testlerde bellek içi).</summary>
    public interface ISettingsStore
    {
        bool TryGetFloat(string key, out float value);

        bool TryGetInt(string key, out int value);

        void SetFloat(string key, float value);

        void SetInt(string key, int value);

        /// <summary>Yazılanları kalıcı hale getirir (diske).</summary>
        void Save();
    }
}
