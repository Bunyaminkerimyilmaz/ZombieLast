using LastHaven.Core;
using UnityEngine;

namespace LastHaven.DebugTools
{
    /// <summary>
    /// Geliştirme aracı: <see cref="IInputReader"/> olaylarını ve mod/cihaz/basılı tutma değişimlerini Console'a yazar.
    /// Varsayılan kapalıdır; [Systems]/Input üzerinde bileşeni açınca loglar. Mod değiştirmek için sağ tık menüsü kullanılabilir.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class InputDebugLogger : MonoBehaviour
    {
        private const string Prefix = "[Input] ";

        private IInputReader _input;
        private bool _subscribed;
        private InputMode _lastMode;
        private bool _lastGamepad;
        private bool _lastFire;
        private bool _lastInteract;
        private Vector2Int _lastMove;
        private Vector2Int _lastAimStick;

        private void Start()
        {
            _input = ServiceLocator.Get<IInputReader>();
            _lastMode = _input.Mode;
            _lastGamepad = _input.IsUsingGamepad;
            Subscribe();
        }

        private void OnEnable()
        {
            // İlk açılışta servis henüz alınmadı; Start abone olur. Sonraki açılışlar buradan.
            if (_input != null)
            {
                Subscribe();
            }
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Update()
        {
            if (_input.Mode != _lastMode)
            {
                _lastMode = _input.Mode;
                Debug.Log(Prefix + "Mode → " + _lastMode, this);
            }

            LogIfChanged(ref _lastGamepad, _input.IsUsingGamepad, "Cihaz: Gamepad", "Cihaz: Klavye/Fare");
            LogIfChanged(ref _lastFire, _input.IsFireHeld, "Fire basıldı", "Fire bırakıldı");
            LogIfChanged(ref _lastInteract, _input.IsInteractHeld, "Interact basıldı", "Interact bırakıldı");
            LogIfChanged(ref _lastMove, _input.MoveInput, "Move");
            LogIfChanged(ref _lastAimStick, _input.AimStick, "AimStick");
        }

        private void LogIfChanged(ref bool last, bool current, string onTrue, string onFalse)
        {
            if (current != last)
            {
                last = current;
                Debug.Log(Prefix + (current ? onTrue : onFalse), this);
            }
        }

        /// <summary>Vektörü -1/0/1 yönlerine yuvarlayıp sadece yön değişince loglar (her karede log basmasın diye).</summary>
        private void LogIfChanged(ref Vector2Int last, Vector2 current, string label)
        {
            var rounded = new Vector2Int(Mathf.RoundToInt(current.x), Mathf.RoundToInt(current.y));
            if (rounded != last)
            {
                last = rounded;
                Debug.Log($"{Prefix}{label} {current} (ekran nişanı {_input.AimScreenPosition})", this);
            }
        }

        private void Subscribe()
        {
            if (_subscribed)
            {
                return;
            }

            _subscribed = true;
            _input.ReloadPressed += OnReload;
            _input.DashPressed += OnDash;
            _input.WeaponSlotPressed += OnWeaponSlot;
            _input.WeaponCyclePressed += OnWeaponCycle;
            _input.BuildModePressed += OnBuildMode;
            _input.RotatePressed += OnRotate;
            _input.UpgradePressed += OnUpgrade;
            _input.SellPressed += OnSell;
            _input.BuildSlotPressed += OnBuildSlot;
            _input.BuildCyclePressed += OnBuildCycle;
            _input.CancelPressed += OnCancel;
            _input.ShopPressed += OnShop;
            _input.ReadyPressed += OnReady;
            _input.PausePressed += OnPause;
            _input.ConfirmPressed += OnConfirm;
        }

        private void Unsubscribe()
        {
            if (!_subscribed)
            {
                return;
            }

            _subscribed = false;
            _input.ReloadPressed -= OnReload;
            _input.DashPressed -= OnDash;
            _input.WeaponSlotPressed -= OnWeaponSlot;
            _input.WeaponCyclePressed -= OnWeaponCycle;
            _input.BuildModePressed -= OnBuildMode;
            _input.RotatePressed -= OnRotate;
            _input.UpgradePressed -= OnUpgrade;
            _input.SellPressed -= OnSell;
            _input.BuildSlotPressed -= OnBuildSlot;
            _input.BuildCyclePressed -= OnBuildCycle;
            _input.CancelPressed -= OnCancel;
            _input.ShopPressed -= OnShop;
            _input.ReadyPressed -= OnReady;
            _input.PausePressed -= OnPause;
            _input.ConfirmPressed -= OnConfirm;
        }

        private void Log(string message) => Debug.Log(Prefix + message, this);

        private void OnReload() => Log("ReloadPressed");
        private void OnDash() => Log("DashPressed");
        private void OnWeaponSlot(int slot) => Log("WeaponSlotPressed " + slot);
        private void OnWeaponCycle(int direction) => Log("WeaponCyclePressed " + direction);
        private void OnBuildMode() => Log("BuildModePressed");
        private void OnRotate() => Log("RotatePressed");
        private void OnUpgrade() => Log("UpgradePressed");
        private void OnSell() => Log("SellPressed");
        private void OnBuildSlot(int slot) => Log("BuildSlotPressed " + slot);
        private void OnBuildCycle(int direction) => Log("BuildCyclePressed " + direction);
        private void OnCancel() => Log("CancelPressed");
        private void OnShop() => Log("ShopPressed");
        private void OnReady() => Log("ReadyPressed");
        private void OnPause() => Log("PausePressed");
        private void OnConfirm() => Log("ConfirmPressed");

        [ContextMenu("Mode → Gameplay")]
        private void SetModeGameplay() => _input?.SetMode(InputMode.Gameplay);

        [ContextMenu("Mode → BuildMode")]
        private void SetModeBuildMode() => _input?.SetMode(InputMode.BuildMode);

        [ContextMenu("Mode → UI")]
        private void SetModeUI() => _input?.SetMode(InputMode.UI);
    }
}
