using Project.Core;
using UnityEngine;

namespace Project.Gameplay
{
    public enum Team
    {
        Player,
        Enemy
    }

    public struct DamageInfo
    {
        public float Amount;
        public Vector3 Point;
        public Vector3 Direction;
        public float Knockback;
        public bool IsCrit;
        public Team SourceTeam;
    }

    public interface IDamageable
    {
        Team Team { get; }
        bool IsAlive { get; }
        void TakeDamage(in DamageInfo info);
    }

    public interface IPlayerTarget
    {
        Transform Transform { get; }
        Vector3 Position { get; }
        bool IsAlive { get; }
        Health Health { get; }
        float PickupRadius { get; }
        void Heal(float amount);
    }

    public interface IEnemySpawner
    {
        Enemy Spawn(Data.EnemyDefinition definition, Vector3 position, float healthMultiplier, float speedMultiplier);
        bool TryGetSpawnPoint(out Vector3 point);
    }

    public sealed class ArenaInfo
    {
        public float HalfSize;

        public ArenaInfo(float halfSize)
        {
            HalfSize = halfSize;
        }

        public Vector3 Clamp(Vector3 p, float margin)
        {
            float limit = HalfSize - margin;
            p.x = Mathf.Clamp(p.x, -limit, limit);
            p.z = Mathf.Clamp(p.z, -limit, limit);
            return p;
        }
    }
}
