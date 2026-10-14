using System;
using System.Collections.Generic;
using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>
    /// Kullanılmış tüm <see cref="EventBus{T}"/> tiplerini tutar ve oyun başında temizler.
    /// Generic sınıfta [RuntimeInitializeOnLoadMethod] çalışmadığı için sıfırlama buradan yapılır.
    /// </summary>
    internal static class EventBusRegistry
    {
        // Domain reload kapalıyken de static constructor'lar tekrar çalışmaz; liste bu yüzden hiç silinmez.
        private static readonly List<Action> _clearActions = new List<Action>();

        internal static void Register(Action clear)
        {
            _clearActions.Add(clear);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        internal static void ResetAll()
        {
            for (int i = 0; i < _clearActions.Count; i++)
            {
                _clearActions[i]();
            }
        }
    }
}
