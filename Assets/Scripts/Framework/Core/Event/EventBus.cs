using System;
using System.Collections.Generic;

namespace Framework.Core.Event
{
    public static class EventBus
    {
        private static Dictionary<Type, Delegate> eventTable = new Dictionary<Type, Delegate>();

        public static void Subscribe<T>(Action<T> listener)
        {
            Type eventType = typeof(T);

            if (eventTable.ContainsKey(eventType))
            {
                eventTable[eventType] = Delegate.Combine(eventTable[eventType], listener);
            }
            else
            {
                eventTable.Add(eventType, listener);
            }
        }

        public static void Unsubscribe<T>(Action<T> listener)
        {
            Type eventType = typeof(T);

            if (!eventTable.ContainsKey(eventType))
                return;

            Delegate currentDelegate = Delegate.Remove(eventTable[eventType], listener);

            if (currentDelegate == null)
            {
                eventTable.Remove(eventType);
            }
            else
            {
                eventTable[eventType] = currentDelegate;
            }
        }

        public static void Publish<T>(T eventData)
        {
            Type eventType = typeof(T);

            if (!eventTable.ContainsKey(eventType))
                return;

            Action<T> callback = eventTable[eventType] as Action<T>;
            callback?.Invoke(eventData);
        }

        public static void Clear()
        {
            eventTable.Clear();
        }
    }
}
