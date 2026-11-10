using System.Collections.Generic;
using UnityEngine;

namespace Project.Core
{
    // Prefab-keyed pools; instances remember their origin so callers only need Despawn(instance).
    public sealed class PoolService : IPoolService
    {
        readonly Dictionary<GameObject, Stack<GameObject>> _pools = new Dictionary<GameObject, Stack<GameObject>>();
        readonly List<IPoolable> _scratch = new List<IPoolable>(8);
        readonly Transform _root;

        public PoolService(Transform root)
        {
            _root = root;
        }

        public void Prewarm(GameObject prefab, int count)
        {
            Stack<GameObject> stack = GetStack(prefab);
            for (int i = 0; i < count; i++)
            {
                GameObject go = Create(prefab);
                go.SetActive(false);
                stack.Push(go);
            }
        }

        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            Stack<GameObject> stack = GetStack(prefab);
            GameObject go = null;
            while (stack.Count > 0 && go == null) go = stack.Pop();
            if (go == null) go = Create(prefab);

            go.transform.SetPositionAndRotation(position, rotation);
            go.GetComponent<PooledObject>().Active = true;
            go.SetActive(true);
            go.GetComponents(_scratch);
            for (int i = 0; i < _scratch.Count; i++) _scratch[i].OnSpawn();
            return go;
        }

        public void Despawn(GameObject instance)
        {
            if (instance == null) return;
            if (!instance.TryGetComponent(out PooledObject tag))
            {
                Object.Destroy(instance);
                return;
            }

            if (!tag.Active) return;
            tag.Active = false;

            instance.GetComponents(_scratch);
            for (int i = 0; i < _scratch.Count; i++) _scratch[i].OnDespawn();
            instance.SetActive(false);
            instance.transform.SetParent(_root, false);
            GetStack(tag.Prefab).Push(instance);
        }

        Stack<GameObject> GetStack(GameObject prefab)
        {
            if (!_pools.TryGetValue(prefab, out Stack<GameObject> stack))
            {
                stack = new Stack<GameObject>(16);
                _pools[prefab] = stack;
            }

            return stack;
        }

        GameObject Create(GameObject prefab)
        {
            GameObject go = Object.Instantiate(prefab, _root);
            go.name = prefab.name;
            go.AddComponent<PooledObject>().Prefab = prefab;
            return go;
        }
    }
}
