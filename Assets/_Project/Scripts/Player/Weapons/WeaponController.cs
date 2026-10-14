using LastHaven.Combat;
using LastHaven.Core;
using UnityEngine;

namespace LastHaven.Player
{
    /// <summary>
    /// Oyuncunun silah girdisi ve atışı (GDD §6): 1–3 ve tekerlekle değiştirme, basılı tutunca atış hızına göre otomatik atış,
    /// R ile doldurma (şarjör boşalınca kendiliğinden). Atış Muzzle'dan nişan yönüne hitscan'dir.
    /// Silahlar ve yuva seçimi <see cref="WeaponArsenal"/>'da.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(15)] // PlayerAim'den (10) sonra: bu karenin nişan yönünü kullanır.
    [RequireComponent(typeof(PlayerAim), typeof(Health))]
    public sealed class WeaponController : MonoBehaviour
    {
        [SerializeField, Tooltip("Silah yuvaları, yükseltme verisi ve isabet layer'ları.")]
        private WeaponLoadoutData _loadout;

        [SerializeField, Tooltip("Namlu: mermiler buradan çıkar. Visual dışında durur.")]
        private Transform _muzzle;

        private readonly HitscanShooter _shooter = new HitscanShooter();
        private PlayerAim _aim;
        private Health _health;
        private IInputReader _input;
        private IRunModifiers _modifiers;
        private WeaponArsenal _arsenal;
        private bool _started;

        /// <summary>Silahlar; Start'tan önce null.</summary>
        public WeaponArsenal Arsenal => _arsenal;

        /// <summary>Eldeki silah; yoksa null.</summary>
        public WeaponRuntime CurrentWeapon => _arsenal?.Current;

        private void Awake()
        {
            _aim = GetComponent<PlayerAim>();
            _health = GetComponent<Health>();
            if (_loadout == null || _muzzle == null)
            {
                Debug.LogError("WeaponController: WeaponLoadoutData veya Muzzle atanmamış; Tools/Last Haven/Generate Prefabs çalıştır.", this);
                enabled = false;
            }
        }

        private void Start()
        {
            if (!ServiceLocator.TryGet(out _input))
            {
                Debug.LogWarning("WeaponController: IInputReader bulunamadı; oyuncu ateş edemeyecek.", this);
            }

            if (!ServiceLocator.TryGet(out _modifiers))
            {
                Debug.LogWarning("WeaponController: IRunModifiers bulunamadı; perk ve gece etkileri silahlara uygulanmayacak.", this);
            }

            _arsenal = new WeaponArsenal(_loadout.Slots, _loadout.Upgrades, _modifiers);
            if (_arsenal.Current == null)
            {
                Debug.LogWarning("WeaponController: Başta açık silah yok; oyuncu ateş edemeyecek.", this);
            }

            _started = true;
            Subscribe();
            _arsenal.RaiseCurrentState();
        }

        private void OnEnable()
        {
            // Servisler Start'ta alınır; ilk aboneliği Start yapar.
            if (_started)
            {
                Subscribe();
            }
        }

        private void OnDisable()
        {
            if (_input != null)
            {
                _input.ReloadPressed -= OnReloadPressed;
                _input.WeaponSlotPressed -= OnWeaponSlotPressed;
                _input.WeaponCyclePressed -= OnWeaponCyclePressed;
            }

            if (_modifiers != null)
            {
                _modifiers.StatChanged -= OnStatChanged;
            }
        }

        private void Update()
        {
            WeaponRuntime weapon = CurrentWeapon;
            if (weapon == null)
            {
                return;
            }

            weapon.Tick(Time.deltaTime);
            if (_input != null && _input.IsFireHeld && _health.IsAlive && weapon.TryFire())
            {
                Fire(weapon);
            }
        }

        private void Fire(WeaponRuntime weapon)
        {
            Vector3 direction = _aim.AimDirection;
            Vector3 muzzle = _muzzle.position;

            // Işın oyuncunun ekseninden (namlu yüksekliğinde) başlar: oyuncuya yapışık düşman veya namlunun girdiği duvar da vurulur.
            float reach = Mathf.Max(0f, Vector3.Dot(muzzle - transform.position, direction));
            Vector3 castOrigin = muzzle - direction * reach;

            var request = new HitscanRequest(
                castOrigin, direction, weapon.SpreadAngle, weapon.PelletCount, weapon.Range + reach, _loadout.HitMask, weapon.Damage, gameObject,
                weapon.Id, tracerOrigin: muzzle);
            EventBus<WeaponFiredEvent>.Raise(new WeaponFiredEvent(gameObject, weapon.Id, muzzle, direction, weapon.PelletCount));
            _shooter.Fire(request);
        }

        private void Subscribe()
        {
            if (_input != null)
            {
                _input.ReloadPressed += OnReloadPressed;
                _input.WeaponSlotPressed += OnWeaponSlotPressed;
                _input.WeaponCyclePressed += OnWeaponCyclePressed;
            }

            if (_modifiers != null)
            {
                _modifiers.StatChanged += OnStatChanged;
            }
        }

        private void OnWeaponSlotPressed(int slot)
        {
            WeaponRuntime weapon = _arsenal.GetWeapon(slot);
            if (weapon != null && !_arsenal.IsUnlocked(slot))
            {
                Debug.Log($"[Weapon] {slot + 1}. yuva kilitli ({weapon.Data.DisplayName}).", this);
                return;
            }

            _arsenal.Select(slot);
        }

        private void OnWeaponCyclePressed(int direction)
        {
            _arsenal.Cycle(direction);
        }

        private void OnReloadPressed()
        {
            if (_health.IsAlive)
            {
                CurrentWeapon?.TryStartReload();
            }
        }

        private void OnStatChanged(StatType stat)
        {
            if (stat == StatType.PlayerMagSize)
            {
                _arsenal.NotifyMagSizeChanged();
            }
        }

        [ContextMenu("Debug/Tüm silahları aç")]
        private void UnlockAllForDebug()
        {
            if (_arsenal == null)
            {
                Debug.LogWarning("WeaponController: Önce Play Mode'a gir.", this);
                return;
            }

            for (int i = 0; i < _arsenal.SlotCount; i++)
            {
                _arsenal.Unlock(i);
            }

            Debug.Log("[Weapon] Tüm silahlar açıldı (debug).", this);
        }
    }
}
