using LastHaven.Core;

namespace LastHaven.DebugTools
{
    /// <summary>
    /// Debug paneli açıkken oyuncu girdisi: <see cref="Capture"/> girdiyi <see cref="InputMode.UI"/>'a alır (panel düğmesine tıklamak ateş
    /// etmez, WASD yürütmez) ve önceki modu hatırlar; <see cref="Release"/> mod hâlâ UI ise önceki moda döndürür. Açılırken zaten UI
    /// modundaysa (market, kartlar) hiç dokunmaz: o menü kapanınca modu kendisi döndürür. Girdi servisi yoksa veya yok edildiyse etkisiz.
    /// </summary>
    public sealed class DebugPanelInputScope
    {
        private IInputReader _input;
        private InputMode _returnMode;
        private bool _captured;

        /// <summary>Girdi servisi (Start'ta alınır; null olabilir).</summary>
        public void SetInput(IInputReader input)
        {
            _input = input;
        }

        public void Capture()
        {
            _captured = IsAlive && _input.Mode != InputMode.UI;
            if (_captured)
            {
                _returnMode = _input.Mode;
                _input.SetMode(InputMode.UI);
            }
        }

        public void Release()
        {
            if (_captured && IsAlive && _input.Mode == InputMode.UI)
            {
                _input.SetMode(_returnMode);
            }

            _captured = false;
        }

        private bool IsAlive => _input is UnityEngine.Object unityObject ? unityObject != null : _input != null;
    }
}
