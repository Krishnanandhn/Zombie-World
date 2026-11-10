using Project.Core;
using UnityEngine;

namespace Project.Gameplay
{
    public sealed class PickupManager
    {
        const float HealAmount = 20f;

        readonly GameplayAssets _assets;
        readonly IPoolService _pool;

        public PickupManager(GameplayAssets assets, IPoolService pool)
        {
            _assets = assets;
            _pool = pool;
        }

        public void TrySpawnHealth(Vector3 position, float chance)
        {
            if (chance <= 0f || Random.value > chance) return;

            GameObject go = _pool.Spawn(_assets.HealthPickup, new Vector3(position.x, 0f, position.z), Quaternion.identity);
            if (go.TryGetComponent(out HealthPickup pickup)) pickup.Setup(HealAmount);
        }

        public void Clear() => HealthPickup.DespawnAll(_pool);
    }
}
