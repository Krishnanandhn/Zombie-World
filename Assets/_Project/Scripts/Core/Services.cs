using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Core
{
    // Lightweight service locator. Gameplay depends on interfaces so implementations stay swappable.
    public static class Services
    {
        static readonly Dictionary<Type, object> Map = new Dictionary<Type, object>();

        public static void Register<T>(T service) where T : class
        {
            Map[typeof(T)] = service;
        }

        public static T Get<T>() where T : class
        {
            return Map.TryGetValue(typeof(T), out object s) ? (T)s : null;
        }

        public static bool TryGet<T>(out T service) where T : class
        {
            service = Get<T>();
            return service != null;
        }

        public static void Clear() => Map.Clear();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetOnPlay() => Clear();
    }
}
