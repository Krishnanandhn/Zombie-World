using Project.Core;
using Project.Data;
using UnityEngine;

namespace Project.Gameplay
{
    public sealed class EnemySpawner : IEnemySpawner
    {
        const int PlacementAttempts = 16;
        const float EdgeMargin = 2.5f;
        const float ClearanceRadius = 0.9f;

        readonly GameplayAssets _assets;
        readonly IPoolService _pool;
        readonly LevelDefinition _level;

        public EnemySpawner(GameplayAssets assets, IPoolService pool, LevelDefinition level)
        {
            _assets = assets;
            _pool = pool;
            _level = level;
        }

        public Enemy Spawn(EnemyDefinition definition, Vector3 position, float healthMultiplier, float speedMultiplier)
        {
            GameObject template = _assets.GetEnemyTemplate(definition);
            GameObject go = _pool.Spawn(template, new Vector3(position.x, 0f, position.z), Quaternion.identity);
            var enemy = go.GetComponent<Enemy>();
            enemy.Init(definition, healthMultiplier, speedMultiplier);
            return enemy;
        }

        public bool TryGetSpawnPoint(out Vector3 point)
        {
            float limit = _level.ArenaHalfSize - EdgeMargin;
            IPlayerTarget player = Services.Get<IPlayerTarget>();
            point = Vector3.zero;

            for (int i = 0; i < PlacementAttempts; i++)
            {
                var candidate = new Vector3(Random.Range(-limit, limit), 0f, Random.Range(-limit, limit));
                point = candidate;

                if (player != null)
                {
                    Vector3 d = candidate - player.Position;
                    d.y = 0f;
                    if (d.magnitude < _level.MinSpawnDistance) continue;
                }

                if (Physics.CheckSphere(candidate + Vector3.up, ClearanceRadius, ~0, QueryTriggerInteraction.Ignore)) continue;
                return true;
            }

            return false;
        }
    }
}
