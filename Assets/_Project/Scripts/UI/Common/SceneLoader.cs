using UnityEngine;
using UnityEngine.SceneManagement;

namespace LastHaven.UI
{
    /// <summary>
    /// Menülerin sahne geçişi (ana menü ↔ oyun, yeniden başlatma). Sahne Build Settings'te yoksa hata loglar ve yüklemez. Duraklatma
    /// kalkmadan yüklenebilir: oyun sahnesi kapanırken <c>PauseService</c> zaman ölçeğini geri yazar.
    /// </summary>
    public static class SceneLoader
    {
        private const string LogPrefix = "[UI] ";

        /// <summary>Adı verilen sahneyi tek başına yükler (kare sonunda); yüklenemiyorsa false.</summary>
        public static bool TryLoad(string sceneName, Object context)
        {
            if (string.IsNullOrEmpty(sceneName) || !Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError($"{LogPrefix}'{sceneName}' sahnesi Build Settings'te yok; yüklenemiyor. Tools/Last Haven/Rebuild All çalıştır.",
                    context);
                return false;
            }

            Debug.Log($"{LogPrefix}Sahne yükleniyor: {sceneName}.", context);
            SceneManager.LoadScene(sceneName);
            return true;
        }

        /// <summary>Etkin sahneyi baştan yükler (yeni run).</summary>
        public static bool TryReloadActive(Object context)
        {
            return TryLoad(SceneManager.GetActiveScene().name, context);
        }
    }
}
