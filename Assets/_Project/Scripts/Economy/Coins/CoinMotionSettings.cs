using System;
using UnityEngine;

namespace LastHaven.Economy
{
    /// <summary>
    /// Jetonun saçılma yayı, dönmesi, yanıp sönmesi ve mıknatısla uçuşu. GDD'de yok; değerler his için seçildi,
    /// <see cref="EconomyConfig"/>'ten ayarlanır. Ömür ve yanıp sönme süresi GDD'de (config'in kendi alanları).
    /// </summary>
    [Serializable]
    public sealed class CoinMotionSettings
    {
        [Header("Saçılma")]
        [SerializeField, Min(0f), Tooltip("Jetonların fırladığı yükseklik: ölüm noktasının (ayak) bu kadar üstü (m).")]
        private float _spawnHeight = 0.8f;

        [SerializeField, Min(0f), Tooltip("Yerdeki jetonun merkezinin zeminden yüksekliği (m). Jeton dik durur.")]
        private float _restHeight = 0.3f;

        [SerializeField, Min(0f), Tooltip("İniş noktasının ölüm noktasına en kısa yatay uzaklığı (m).")]
        private float _scatterMinDistance = 0.6f;

        [SerializeField, Min(0f), Tooltip("İniş noktasının ölüm noktasına en uzun yatay uzaklığı (m).")]
        private float _scatterMaxDistance = 1.6f;

        [SerializeField, Min(0f), Tooltip("Yayın, düz çizginin üstüne en fazla çıktığı yükseklik (m).")]
        private float _arcHeight = 1.2f;

        [SerializeField, Min(0f), Tooltip("Yayın süresi (sn). Bu sürede jeton mıknatısa kapılmaz.")]
        private float _arcDuration = 0.45f;

        [Header("Dönme ve yanıp sönme")]
        [SerializeField, Tooltip("Dikey eksende dönme hızı (derece/sn).")]
        private float _spinSpeed = 180f;

        [SerializeField, Min(0f), Tooltip("Ömrün sonundaki yanıp sönme sıklığı (Hz: saniyedeki aç-kapa sayısı). 0 = yanıp sönmez.")]
        private float _blinkFrequency = 6f;

        [Header("Mıknatıs")]
        [SerializeField, Min(0f), Tooltip("Yarıçapa girince uçuşun başlangıç hızı (m/sn).")]
        private float _attractStartSpeed = 4f;

        [SerializeField, Min(0f), Tooltip("Uçuş ivmesi (m/sn²): jeton oyuncuya giderken hızlanır.")]
        private float _attractAcceleration = 40f;

        [SerializeField, Min(0f), Tooltip("Uçuşun en yüksek hızı (m/sn).")]
        private float _attractMaxSpeed = 25f;

        [SerializeField, Min(0f), Tooltip("Jeton, oyuncunun toplama noktasına bu kadar yaklaşınca toplanır (m).")]
        private float _collectDistance = 0.4f;

        [SerializeField, Min(0f), Tooltip("Toplama noktasının oyuncunun ayağından yüksekliği (m); jeton gövdeye uçar.")]
        private float _collectHeight = 1f;

        public float SpawnHeight => _spawnHeight;

        public float RestHeight => _restHeight;

        public float ScatterMinDistance => _scatterMinDistance;

        /// <summary>En kısa uzaklıktan küçükse en kısa uzaklık sayılır.</summary>
        public float ScatterMaxDistance => Mathf.Max(_scatterMinDistance, _scatterMaxDistance);

        public float ArcHeight => _arcHeight;

        public float ArcDuration => _arcDuration;

        public float SpinSpeed => _spinSpeed;

        public float BlinkFrequency => _blinkFrequency;

        public float AttractStartSpeed => _attractStartSpeed;

        public float AttractAcceleration => _attractAcceleration;

        /// <summary>Başlangıç hızından küçükse başlangıç hızı sayılır.</summary>
        public float AttractMaxSpeed => Mathf.Max(_attractStartSpeed, _attractMaxSpeed);

        public float CollectDistance => _collectDistance;

        public float CollectHeight => _collectHeight;
    }
}
