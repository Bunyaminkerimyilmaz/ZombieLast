using System;
using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>
    /// Cihazdan bağımsız oyuncu girdisi (GDD §4). Sürekli değerler property, tek seferlik basışlar C# event'idir.
    /// Sadece etkin <see cref="Mode"/>'un girdileri gelir; diğer modlardaki değerler sıfır/false döner.
    /// </summary>
    public interface IInputReader
    {
        /// <summary>Etkin girdi modu. Başlangıçta <see cref="InputMode.Gameplay"/>.</summary>
        InputMode Mode { get; }

        /// <summary>Ham hareket girdisi (-1..1, çapraz normalize). Kameraya göre çevirmek tüketicinin işi. UI modunda sıfır.</summary>
        Vector2 MoveInput { get; }

        /// <summary>Farenin son bilinen ekran pozisyonu (piksel). Mod değişse de son değer korunur.</summary>
        Vector2 AimScreenPosition { get; }

        /// <summary>Sağ çubuk (-1..1); ölü bölgedeyken sıfır. UI modunda sıfır.</summary>
        Vector2 AimStick { get; }

        /// <summary>Son kullanılan cihaz gamepad mi. Nişan yöntemi ve ekrandaki tuş ipuçları buna göre seçilir.</summary>
        bool IsUsingGamepad { get; }

        /// <summary>Ateş tuşu basılı mı (sadece Gameplay modunda).</summary>
        bool IsFireHeld { get; }

        /// <summary>Etkileşim tuşu basılı mı (sadece Gameplay modunda).</summary>
        bool IsInteractHeld { get; }

        event Action ReloadPressed;

        event Action DashPressed;

        /// <summary>Silah yuvası seçildi. Yuva indeksi 0 tabanlıdır (1 tuşu → 0).</summary>
        event Action<int> WeaponSlotPressed;

        /// <summary>Silah sırayla değişti: +1 sıradaki, -1 önceki.</summary>
        event Action<int> WeaponCyclePressed;

        /// <summary>İnşa modu aç/kapa isteği (Gameplay ve BuildMode modlarında).</summary>
        event Action BuildModePressed;

        event Action RotatePressed;

        event Action UpgradePressed;

        event Action SellPressed;

        /// <summary>Yapı seçildi (BuildMode). Yuva indeksi 0 tabanlıdır (1 tuşu → 0).</summary>
        event Action<int> BuildSlotPressed;

        /// <summary>Yapı seçimi sırayla değişti (BuildMode, gamepad D-pad sol/sağ): +1 sıradaki, -1 önceki.</summary>
        event Action<int> BuildCyclePressed;

        /// <summary>İptal: inşa önizlemesi (BuildMode) veya açık menü (UI).</summary>
        event Action CancelPressed;

        event Action ShopPressed;

        /// <summary>"Hazırım": gündüzü erken bitir.</summary>
        event Action ReadyPressed;

        event Action PausePressed;

        /// <summary>Onay: yapıyı yerleştir (BuildMode) veya seçimi onayla (UI).</summary>
        event Action ConfirmPressed;

        /// <summary>Sadece <paramref name="mode"/>'un action haritasını açar.</summary>
        void SetMode(InputMode mode);
    }
}
