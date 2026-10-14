using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>
    /// [Systems]/RunModifiers: run başında boş bir <see cref="RunModifiers"/> oluşturur ve <see cref="IRunModifiers"/> olarak kaydeder.
    /// Sahne yeniden yüklenince (yeni run) her şey sıfırlanır.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RunModifiersHost : MonoBehaviour
    {
        private RunModifiers _modifiers;

        /// <summary>Debug ve testler için; oyun kodu servisi ServiceLocator'dan alır.</summary>
        public RunModifiers Modifiers => _modifiers;

        private void Awake()
        {
            _modifiers = new RunModifiers();
            ServiceLocator.Register<IRunModifiers>(_modifiers);
        }

        private void OnDestroy()
        {
            if (_modifiers != null)
            {
                ServiceLocator.Unregister<IRunModifiers>(_modifiers);
            }
        }
    }
}
