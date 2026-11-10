using UnityEngine;

namespace Project.Core
{
    public interface IPoolService
    {
        GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation);
        void Despawn(GameObject instance);
        void Prewarm(GameObject prefab, int count);
    }
}
