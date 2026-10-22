using UnityEngine;

namespace LastHaven.UI
{
    /// <summary>
    /// Seviye kartları panelinin zamanlamaları (GDD §13, §16; GDD sayı vermez). Asset'i üretici oluşturur:
    /// <c>Data/UI/LevelUpPanelConfig.asset</c>. Süreler gerçek zamandır: kartlar açıkken oyun duraklıdır (<c>Time.timeScale</c> 0).
    /// Görsel transition süreleri <c>UI/LevelUp/LevelUp.uss</c>'te.
    /// </summary>
    public sealed class LevelUpPanelConfig : ScriptableObject
    {
        [Header("Zamanlama (gerçek zaman, sn)")]
        [SerializeField, Min(0f), Tooltip("Kartlar açıldıktan sonra seçimin kabul edilmediği süre: oyuncu o an ateş / atılma (A) tuşuna " +
                                          "basıyorsa kazara kart seçmesin. Kartların giriş animasyonu bu sürede oynar.")]
        private float _inputDelay = 0.4f;

        [SerializeField, Min(0f), Tooltip("Kart seçildikten sonra oyun devam etmeden (veya sıradaki seviyenin kartları gelmeden) önce seçim " +
                                          "animasyonu için bekleme. LevelUp.uss'teki seçim transition'ından uzun olmalı.")]
        private float _confirmDuration = 0.6f;

        public float InputDelay => _inputDelay;

        public float ConfirmDuration => _confirmDuration;
    }
}
