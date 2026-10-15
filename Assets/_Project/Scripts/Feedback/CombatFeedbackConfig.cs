using System.Collections.Generic;
using LastHaven.Combat;
using UnityEngine;

namespace LastHaven.Feedback
{
    /// <summary>
    /// Atış ve yapı yıkılma efektleri (<see cref="FeedbackDirector"/>): prefab'lar, havuz ön ısıtma sayıları ve iz renkleri.
    /// Asset'i üretici oluşturur: <c>Data/Feedback/CombatFeedbackConfig.asset</c>. Efektlerin kendi ayarları prefab'larında.
    /// </summary>
    public sealed class CombatFeedbackConfig : ScriptableObject
    {
        [Header("Prefab'lar")]
        [SerializeField, Tooltip("Mermi izi (LineRenderer).")]
        private Tracer _tracerPrefab;

        [SerializeField, Tooltip("Vuruş kıvılcımı / kanı (ParticleSystem).")]
        private ImpactEffect _impactPrefab;

        [SerializeField, Tooltip("Namlu ışığı (nokta ışık).")]
        private MuzzleFlash _muzzleFlashPrefab;

        [SerializeField, Tooltip("Yapı yıkılınca moloz (ParticleSystem, mesh parçalar).")]
        private DebrisEffect _debrisPrefab;

        [Header("Havuz (oyun başında önceden oluşturulur)")]
        [SerializeField, Min(0), Tooltip("Pompalı tek atışta 8 iz çıkarır.")]
        private int _tracerPrewarm = 16;

        [SerializeField, Min(0), Tooltip("Vuruş efekti ömrü boyunca aynı anda görünebilecek en fazla isabet.")]
        private int _impactPrewarm = 24;

        [SerializeField, Min(0)]
        private int _muzzleFlashPrewarm = 4;

        [SerializeField, Min(0), Tooltip("Aynı anda yıkılabilecek yapı sayısı (efekt ~1.2 sn sürer).")]
        private int _debrisPrewarm = 4;

        [Header("Mermi izi")]
        [SerializeField, Tooltip("İz rengi bu silahların verisinden (Tracer Color) alınır; silah id'siyle eşleşir.")]
        private List<WeaponData> _weapons = new List<WeaponData>();

        [SerializeField, Tooltip("Listede olmayan silahların iz rengi.")]
        private Color _defaultTracerColor = Color.white;

        [SerializeField, Min(0f), Tooltip("Namludan ileriye bundan kısa izler çizilmez (oyuncuya yapışık hedef).")]
        private float _minTracerLength = 0.3f;

        /// <summary>Null olabilir.</summary>
        public Tracer TracerPrefab => _tracerPrefab;

        /// <summary>Null olabilir.</summary>
        public ImpactEffect ImpactPrefab => _impactPrefab;

        /// <summary>Null olabilir.</summary>
        public MuzzleFlash MuzzleFlashPrefab => _muzzleFlashPrefab;

        public int TracerPrewarm => _tracerPrewarm;

        public int ImpactPrewarm => _impactPrewarm;

        public int MuzzleFlashPrewarm => _muzzleFlashPrewarm;

        /// <summary>Null olabilir.</summary>
        public DebrisEffect DebrisPrefab => _debrisPrefab;

        public int DebrisPrewarm => _debrisPrewarm;

        public IReadOnlyList<WeaponData> Weapons => _weapons;

        public Color DefaultTracerColor => _defaultTracerColor;

        public float MinTracerLength => _minTracerLength;
    }
}
