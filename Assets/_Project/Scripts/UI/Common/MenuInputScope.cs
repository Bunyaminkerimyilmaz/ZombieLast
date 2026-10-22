using LastHaven.Core;
using UnityEngine;

namespace LastHaven.UI
{
    /// <summary>
    /// Açık bir menünün girdisi (seviye kartları, duraklatma menüsü, run sonu, ana menü): <see cref="Enter"/> girdiyi
    /// <see cref="InputMode.UI"/>'a alır (oyuncu durur, Enter / Esc / 1–3 menüye gider) ve önceki modu hatırlar; <see cref="Exit"/> hâlâ
    /// UI'daysa önceki moda döndürür (inşa modunda duraklatılınca inşa moduna; arada başka bir şey modu değiştirdiyse dokunmaz). Enter
    /// zaten UI modundayken çağrılırsa dönüş modu <see cref="InputMode.Gameplay"/>'dir. Girdi servisi yoksa veya yok edildiyse hiçbir
    /// şey yapmaz.
    /// </summary>
    public sealed class MenuInputScope
    {
        private readonly IInputReader _input;
        private bool _entered;
        private InputMode _returnMode = InputMode.Gameplay;

        /// <param name="input">Null olabilir.</param>
        public MenuInputScope(IInputReader input)
        {
            _input = input;
        }

        /// <summary>Etkin girdi modu; servis yoksa <see cref="InputMode.Gameplay"/>.</summary>
        public InputMode Mode => IsAlive ? _input.Mode : InputMode.Gameplay;

        /// <summary>Son kullanılan cihaz gamepad mi (tuş ipuçları, odak).</summary>
        public bool IsUsingGamepad => IsAlive && _input.IsUsingGamepad;

        /// <summary><see cref="Exit"/>'in döneceği mod.</summary>
        public InputMode ReturnMode => _returnMode;

        private bool IsAlive => _input is Object unityObject ? unityObject != null : _input != null;

        /// <summary>Girdiyi UI moduna alır.</summary>
        public void Enter()
        {
            _entered = IsAlive;
            if (_entered)
            {
                _returnMode = _input.Mode == InputMode.UI ? InputMode.Gameplay : _input.Mode;
                _input.SetMode(InputMode.UI);
            }
        }

        /// <summary><see cref="Enter"/> çağrıldıysa ve mod hâlâ UI ise önceki moda döner.</summary>
        public void Exit()
        {
            if (_entered && IsAlive && _input.Mode == InputMode.UI)
            {
                _input.SetMode(_returnMode);
            }

            _entered = false;
        }
    }
}
