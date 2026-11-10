using UnityEngine;

namespace Project.Core
{
    // Marks an instance with the prefab it came from so it can be returned to the right pool.
    public sealed class PooledObject : MonoBehaviour
    {
        public GameObject Prefab;
        public bool Active;
    }
}
