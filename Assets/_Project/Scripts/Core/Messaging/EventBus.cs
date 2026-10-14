using System;
using UnityEngine;

namespace LastHaven.Core
{
    /// <summary>
    /// Olay tipine göre ayrılmış statik olay yolu.
    /// Abone listesi copy-on-write tutulur: <see cref="Raise"/> bellek ayırmaz ve
    /// Raise sırasında abone olmak / abonelikten çıkmak güvenlidir.
    /// </summary>
    /// <remarks>
    /// Raise sırasında abone olan dinleyici bir sonraki Raise'den itibaren çağrılır.
    /// Raise sırasında abonelikten çıkan dinleyici, o Raise'de henüz çağrılmadıysa artık çağrılmaz.
    /// Bir dinleyicinin fırlattığı istisna loglanır, diğer dinleyiciler çalışmaya devam eder.
    /// </remarks>
    public static class EventBus<T> where T : struct, IEvent
    {
        private static Action<T>[] _handlers = Array.Empty<Action<T>>();

        static EventBus()
        {
            EventBusRegistry.Register(Clear);
        }

        /// <summary>Dinleyiciyi ekler. Aynı dinleyici ikinci kez eklenmez.</summary>
        public static void Subscribe(Action<T> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            if (Array.IndexOf(_handlers, handler) >= 0)
            {
                return;
            }

            var next = new Action<T>[_handlers.Length + 1];
            Array.Copy(_handlers, next, _handlers.Length);
            next[_handlers.Length] = handler;
            _handlers = next;
        }

        /// <summary>Dinleyiciyi çıkarır. Kayıtlı değilse bir şey yapmaz.</summary>
        public static void Unsubscribe(Action<T> handler)
        {
            if (handler == null)
            {
                return;
            }

            int index = Array.IndexOf(_handlers, handler);
            if (index < 0)
            {
                return;
            }

            if (_handlers.Length == 1)
            {
                _handlers = Array.Empty<Action<T>>();
                return;
            }

            var next = new Action<T>[_handlers.Length - 1];
            Array.Copy(_handlers, 0, next, 0, index);
            Array.Copy(_handlers, index + 1, next, index, _handlers.Length - index - 1);
            _handlers = next;
        }

        /// <summary>Olayı o anki tüm dinleyicilere gönderir.</summary>
        public static void Raise(T evt)
        {
            Action<T>[] snapshot = _handlers;
            for (int i = 0; i < snapshot.Length; i++)
            {
                Action<T> handler = snapshot[i];

                // Liste bu Raise sırasında değiştiyse, çıkarılmış dinleyiciyi atla.
                if (!ReferenceEquals(snapshot, _handlers) && Array.IndexOf(_handlers, handler) < 0)
                {
                    continue;
                }

                try
                {
                    handler(evt);
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
        }

        internal static int SubscriberCount => _handlers.Length;

        internal static void Clear()
        {
            _handlers = Array.Empty<Action<T>>();
        }
    }
}
