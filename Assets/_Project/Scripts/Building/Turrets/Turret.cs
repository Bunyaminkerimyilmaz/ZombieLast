using System;
using System.Collections.Generic;
using LastHaven.Combat;
using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Building
{
    /// <summary>
    /// Taret (GDD §11): yapının (<see cref="Structure"/>, hedef türü <see cref="TargetType.Turret"/>) kökünde. Yerleşikken
    /// menzildeki en yakın canlı düşmanı seçer ve kafayı sadece Y ekseninde ona çevirir (<see cref="TurretTracker"/>);
    /// hizalanınca <see cref="HitscanShooter"/> ile ateş eder. <see cref="WeaponFiredEvent"/> ve atıcının olayları namlu ışığını,
    /// mermi izini ve vuruş efektini kendiliğinden oynatır. Hasar/atış hızı/menzil seviyeden × <see cref="IRunModifiers"/>
    /// (<see cref="TurretStats"/>). Güç yokken (<see cref="IPowerService"/>, <see cref="PowerChangedEvent"/>) ateş etmez ve
    /// dönmez; kafa eğilir, gösterge söner (<see cref="TurretVisual"/>). Şok Taret açıksa vuruş yavaşlatır (<see cref="TurretShock"/>).
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Structure))]
    public sealed class Turret : MonoBehaviour, IPoolable
    {
        private const string LogPrefix = "[Building] ";

        [SerializeField, Tooltip("Tarama, dönüş, atış ayarları (tüm taretler). Üretici bağlar.")]
        private TurretConfig _config;

        [SerializeField, Tooltip("Dönen nişan pivotu (Visual dışında, kökün çocuğu). Namlu bunun çocuğu. Üretici bağlar.")]
        private Transform _aimPivot;

        [SerializeField, Tooltip("Namlu ucu (nişan pivotunun çocuğu): mermi izi ve namlu ışığı buradan. Üretici bağlar.")]
        private Transform _muzzle;

        [SerializeField, Tooltip("Görsel köprü (kafa, gösterge). Boş olabilir. Üretici bağlar.")]
        private TurretVisual _visual;

        private readonly TurretCadence _cadence = new TurretCadence();
        private readonly HitscanShooter _shooter = new HitscanShooter();
        private Structure _structure;
        private TurretTracker _tracker;
        private IRunModifiers _modifiers;
        private IPowerService _power;
        private TurretStats _stats;
        private int _loggedLevel = -1;
        private bool _isPowered = true;

        /// <summary>Etkili değerler (seviye × run çarpanları); yerleşikken her kare yenilenir, yerleşmeden önce sıfır.</summary>
        public TurretStats Stats => _stats;

        /// <summary>Kafanın dünya yaw'ı (derece).</summary>
        public float Yaw => _tracker != null ? _tracker.Yaw : transform.eulerAngles.y;

        public bool IsPowered => _isPowered;

        /// <summary>Seçili hedefin kökü; yoksa null.</summary>
        public Transform CurrentTarget => _tracker != null && _tracker.Target.IsValid ? _tracker.Target.Transform : null;

        public void OnSpawned()
        {
            if (_tracker == null)
            {
                return;
            }

            _loggedLevel = -1;
            _cadence.Reset();
            _tracker.Reset(transform.eulerAngles.y, UnityEngine.Random.Range(0f, _config.ScanInterval));
            _isPowered = _power == null || _power.IsPowered;
            ApplyAim();
            if (_visual != null)
            {
                _visual.Snap(_tracker.Yaw, _isPowered);
            }
        }

        public void OnDespawned()
        {
            _tracker?.ClearTarget();
            _stats = default; // Sonraki yerleşmenin ilk karesinde seviyeden yeniden hesaplanır.
        }

        private void Awake()
        {
            _structure = GetComponent<Structure>();
            if (_config == null || _aimPivot == null || _muzzle == null)
            {
                Debug.LogError("Turret: TurretConfig, nişan pivotu veya namlu atanmamış; Tools/Last Haven/Generate Prefabs çalıştır.", this);
                enabled = false;
                return;
            }

            _tracker = new TurretTracker(_config);
            _tracker.Reset(transform.eulerAngles.y);
        }

        private void Start()
        {
            ServiceLocator.TryGet(out _modifiers);
            if (!ServiceLocator.TryGet(out _power))
            {
                Debug.LogWarning("Turret: IPowerService bulunamadı; taret güç durumunu izlemeyecek (hep güçlü).", this);
            }

            _isPowered = _power == null || _power.IsPowered;
        }

        private void OnEnable()
        {
            EventBus<PowerChangedEvent>.Subscribe(OnPowerChanged);
        }

        private void OnDisable()
        {
            EventBus<PowerChangedEvent>.Unsubscribe(OnPowerChanged);
            _tracker?.ClearTarget();
        }

        private void Update()
        {
            if (!_structure.IsPlaced)
            {
                return;
            }

            float deltaTime = Time.deltaTime;
            RefreshStats();
            _cadence.Tick(deltaTime);
            if (_isPowered)
            {
                Vector3 axis = GetAxisPoint();
                bool aligned = _tracker.Tick(axis, _stats.Range, deltaTime, out Vector3 direction);
                ApplyAim();
                if (aligned && _cadence.TryFire(_stats.FireInterval))
                {
                    Fire(axis, direction);
                }
            }

            if (_visual != null)
            {
                _visual.Apply(_tracker.Yaw, _isPowered, deltaTime);
            }
        }

        private void Fire(Vector3 axis, Vector3 direction)
        {
            Vector3 muzzle = _muzzle.position;

            // Işın taretin ekseninden (namlu yüksekliğinde) başlar: tarete yapışık düşman da vurulur. Menzil eksenden ölçülür.
            var request = new HitscanRequest(
                axis, direction, 0f, 1, _stats.Range, _config.HitMask, _stats.Damage, gameObject, _config.WeaponId, tracerOrigin: muzzle);
            EventBus<WeaponFiredEvent>.Raise(new WeaponFiredEvent(gameObject, _config.WeaponId, muzzle, direction, 1));
            IReadOnlyList<ShotResult> results = _shooter.Fire(request);

            // Şok Taret (GDD §13): isabet eden vuruş yavaşlatır.
            BuildingPerkConfig perks = _config.Perks;
            if (perks != null && _modifiers != null && _modifiers.HasFlag(PerkFlag.ShockTurret))
            {
                TurretShock.Apply(results, perks.ShockSlowFraction, perks.ShockSlowDuration);
            }
        }

        // Seviye × run çarpanları (perk, market) her kare; ucuz (en fazla birkaç stat). Seviye değişince loglar.
        private void RefreshStats()
        {
            _stats = TurretStats.From(_structure.CurrentLevel, _modifiers);
            int level = _structure.Level;
            if (level != _loggedLevel)
            {
                _loggedLevel = level;
                Debug.Log(FormattableString.Invariant(
                    $"{LogPrefix}Taret {level}/{_structure.MaxLevel}: hasar {_stats.Damage:0.##}, {_stats.FireRate:0.##} atış/sn, menzil {_stats.Range:0.##} m."),
                    this);
            }
        }

        // Taretin dönüş ekseni, namlu yüksekliğinde (tarama merkezi ve ışın başlangıcı).
        private Vector3 GetAxisPoint()
        {
            Vector3 pivot = _aimPivot.position;
            return new Vector3(pivot.x, _muzzle.position.y, pivot.z);
        }

        private void ApplyAim()
        {
            _aimPivot.rotation = Quaternion.Euler(0f, _tracker.Yaw, 0f);
        }

        private void OnPowerChanged(PowerChangedEvent changed)
        {
            _isPowered = changed.IsPowered;
            if (!_isPowered)
            {
                _tracker?.ClearTarget();
            }
        }
    }
}
