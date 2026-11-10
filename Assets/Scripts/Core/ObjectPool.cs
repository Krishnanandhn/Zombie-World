using System.Collections.Generic;
using UnityEngine;

namespace ZombieWorld
{
    // Stack-based pool. The prefab must be an inactive template so clones start inactive.
    public sealed class ObjectPool<T> where T : Component
    {
        readonly T _prefab;
        readonly Transform _parent;
        readonly Stack<T> _free = new Stack<T>();

        public int TotalCreated { get; private set; }
        public int ActiveCount => TotalCreated - _free.Count;

        public ObjectPool(T prefab, Transform parent, int prewarm)
        {
            _prefab = prefab;
            _parent = parent;
            for (int i = 0; i < prewarm; i++)
            {
                _free.Push(Create());
            }
        }

        public T Get()
        {
            T item = _free.Count > 0 ? _free.Pop() : Create();
            item.gameObject.SetActive(true);
            return item;
        }

        public void Release(T item)
        {
            if (!item.gameObject.activeSelf) return;
            item.gameObject.SetActive(false);
            _free.Push(item);
        }

        T Create()
        {
            T instance = Object.Instantiate(_prefab, _parent);
            TotalCreated++;
            return instance;
        }
    }
}
