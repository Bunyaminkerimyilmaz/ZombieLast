using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Building
{
    /// <summary>
    /// Diken tuzaklarının ortak davranış ayarları (GDD §11). Hasar/sn, yavaşlatma oranı ve tik aralığı seviyede
    /// (<see cref="BuildableLevel"/> statları), dayanıklılık seviyenin canında; burada değil. Asset'i üretici oluşturur
    /// (<c>Data/Building/SpikeTrapConfig.asset</c>); tuzak prefab'ındaki <see cref="SpikeTrap"/> bağlıdır.
    /// </summary>
    public sealed class SpikeTrapConfig : ScriptableObject
    {
        [Header("Algılama")]
        [SerializeField, Tooltip("Tuzağın etkilediği layer'lar (düşmanlar). Alan: tuzağın kökündeki trigger kutusu.")]
        private LayerMask _targetLayers = GameLayers.EnemyMask;

        [SerializeField, Min(1), Tooltip("Bir taramada okunan en fazla collider. Fazlası o tikte etkilenmez.")]
        private int _scanBufferSize = 32;

        [SerializeField, Min(0.02f), Tooltip("Tuzak boşken üstüne bakma aralığı (sn). İlk giren en geç bu kadar sonra ilk tiki alır " +
                                             "(hızlı koşucu 2 m'lik tuzağı tik aralığından kısa sürede geçebilir).")]
        private float _idleScanInterval = 0.1f;

        [Header("Yavaşlatma")]
        [SerializeField, Min(0.01f), Tooltip("Her tikte verilen yavaşlatmanın süresi (sn). Tik aralığından uzun olmalı: tuzaktayken " +
                                             "kesilmez, çıkınca bu kadar sürer.")]
        private float _slowDuration = 1f;

        [Header("Aşınma")]
        [SerializeField, Min(0f), Tooltip("Her hasar tikinde tuzağın dayanıklılığından (canından) düşen miktar. GDD §11: 1. " +
                                         "Tik sadece üstünde düşman varken olur.")]
        private float _wearPerTick = 1f;

        public LayerMask TargetLayers => _targetLayers;

        public int ScanBufferSize => Mathf.Max(1, _scanBufferSize);

        public float IdleScanInterval => _idleScanInterval;

        public float SlowDuration => _slowDuration;

        public float WearPerTick => _wearPerTick;
    }
}
