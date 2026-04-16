using System;
using System.Collections.Generic;

namespace GamePlay.Battle.Event
{
    public static class EventBus
    {
        private static readonly Dictionary<Type, Delegate> _handlers = new();

        public static void Subscribe<T>(Action<T> handler)
        {
            var type = typeof(T);

            if (_handlers.TryGetValue(type, out var existing))
            {
                _handlers[type] = Delegate.Combine(existing, handler);
            }
            else
            {
                _handlers[type] = handler;
            }
        }

        public static void Unsubscribe<T>(Action<T> handler)
        {
            var type = typeof(T);

            if (!_handlers.TryGetValue(type, out var existing))
                return;

            var current = Delegate.Remove(existing, handler);

            if (current == null)
            {
                _handlers.Remove(type);
            }
            else
            {
                _handlers[type] = current;
            }
        }

        public static void Publish<T>(T evt)
        {
            var type = typeof(T);

            if (_handlers.TryGetValue(type, out var del) && del is Action<T> callback)
            {
                callback.Invoke(evt);
            }
        }
    }
}