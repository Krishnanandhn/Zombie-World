using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Core
{
    // Typed, allocation-free pub/sub. Subscribe in OnEnable, unsubscribe in OnDisable.
    public static class EventBus
    {
        static readonly List<Action> Resetters = new List<Action>();

        internal static void Register(Action reset) => Resetters.Add(reset);

        public static void ClearAll()
        {
            for (int i = 0; i < Resetters.Count; i++) Resetters[i]();
        }

        // Domain reload may be disabled, so static handlers must not leak across play sessions.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetOnPlay() => ClearAll();
    }

    public static class EventBus<T> where T : struct
    {
        static Action<T> _handlers;

        static EventBus()
        {
            EventBus.Register(() => _handlers = null);
        }

        public static void Subscribe(Action<T> handler) => _handlers += handler;

        public static void Unsubscribe(Action<T> handler) => _handlers -= handler;

        public static void Publish(T evt) => _handlers?.Invoke(evt);
    }
}
