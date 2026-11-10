using System.Collections.Generic;
using UnityEngine;

namespace ZombieWorld
{
    // Owns every zombie: pooling, chase, separation, contact damage and pose in single loops (no per-zombie Update).
    public sealed class ZombieManager : MonoBehaviour
    {
        GameManager _gm;
        ObjectPool<Zombie>[] _pools;
        readonly List<Zombie> _active = new List<Zombie>(GameConfig.MaxAliveZombies + 4);

        public int ActiveCount => _active.Count;

        public int PooledTotal
        {
            get
            {
                int total = 0;
                for (int i = 0; i < _pools.Length; i++) total += _pools[i].TotalCreated;
                return total;
            }
        }

        public void Init(GameManager gm)
        {
            _gm = gm;
            var poolRoot = new GameObject("ZombiePool").transform;
            poolRoot.SetParent(transform, false);

            int kinds = System.Enum.GetValues(typeof(ZombieKind)).Length;
            _pools = new ObjectPool<Zombie>[kinds];
            for (int i = 0; i < kinds; i++)
            {
                var kind = (ZombieKind)i;
                var template = new GameObject("Zombie_" + kind);
                template.SetActive(false);
                template.transform.SetParent(poolRoot, false);
                LowPolyAssets.AddRenderer(template, LowPolyAssets.GetZombieMesh(kind), true);
                var zombie = template.AddComponent<Zombie>();
                _pools[i] = new ObjectPool<Zombie>(zombie, poolRoot, GameConfig.ZombiePoolPrewarm);
            }
        }

        public bool TrySpawn(ZombieKind kind, Vector3 position)
        {
            if (_active.Count >= GameConfig.MaxAliveZombies) return false;
            Zombie z = _pools[(int)kind].Get();
            z.Activate(kind, position, Time.time);
            _active.Add(z);
            return true;
        }

        public Zombie FindNearest(Vector3 from, float range)
        {
            float now = Time.time;
            float best = range * range;
            Zombie result = null;
            for (int i = 0; i < _active.Count; i++)
            {
                Zombie z = _active[i];
                if (!z.IsTargetable(now)) continue;
                float dx = z.Pos.x - from.x;
                float dz = z.Pos.z - from.z;
                float d = dx * dx + dz * dz;
                if (d < best)
                {
                    best = d;
                    result = z;
                }
            }
            return result;
        }

        public Zombie FindHit(Vector3 point, float radius)
        {
            float now = Time.time;
            for (int i = 0; i < _active.Count; i++)
            {
                Zombie z = _active[i];
                if (!z.IsTargetable(now)) continue;
                float dx = z.Pos.x - point.x;
                float dz = z.Pos.z - point.z;
                float r = z.Stats.Radius + radius;
                if (dx * dx + dz * dz <= r * r) return z;
            }
            return null;
        }

        public void Damage(Zombie z, int amount, Vector3 direction)
        {
            z.Health -= amount;
            z.HitTimer = 0.12f;
            float push = z.Kind == ZombieKind.Brute ? 1.5f : 5f;
            z.Knock = new Vector3(direction.x, 0f, direction.z).normalized * push;

            if (z.Health <= 0) Kill(z);
        }

        public void ClearAll()
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                _pools[(int)_active[i].Kind].Release(_active[i]);
            }
            _active.Clear();
        }

        void Kill(Zombie z)
        {
            int index = _active.IndexOf(z);
            if (index >= 0)
            {
                int last = _active.Count - 1;
                _active[index] = _active[last];
                _active.RemoveAt(last);
            }

            _gm.Fx.Burst(z.Pos + Vector3.up * 0.8f, z.Kind == ZombieKind.Brute ? 14 : 8, 1f);
            _gm.AddKill(z.Stats.Score);
            _pools[(int)z.Kind].Release(z);
        }

        void Update()
        {
            if (_gm.State != GameState.Playing) return;

            float dt = Time.deltaTime;
            float now = Time.time;
            Vector3 target = _gm.Player.Position;
            int count = _active.Count;

            for (int i = 0; i < count; i++)
            {
                Zombie z = _active[i];
                float dx = target.x - z.Pos.x;
                float dz = target.z - z.Pos.z;
                float dist = Mathf.Sqrt(dx * dx + dz * dz);
                float inv = dist > 0.0001f ? 1f / dist : 0f;
                float dirX = dx * inv;
                float dirZ = dz * inv;

                z.Yaw = Mathf.MoveTowardsAngle(z.Yaw, Mathf.Atan2(dirX, dirZ) * Mathf.Rad2Deg, 540f * dt);

                bool risen = now - z.SpawnTime >= GameConfig.ZombieRiseTime;
                float reach = z.Stats.Radius + GameConfig.PlayerRadius;
                if (risen && dist > reach * 0.9f)
                {
                    float step = z.Stats.Speed * dt;
                    z.Pos.x += dirX * step;
                    z.Pos.z += dirZ * step;
                }

                z.Pos += z.Knock * dt;
                z.Knock = Vector3.MoveTowards(z.Knock, Vector3.zero, 25f * dt);

                if (z.AttackTimer > 0f)
                {
                    z.AttackTimer -= dt;
                }
                else if (risen && dist < reach + 0.15f)
                {
                    _gm.Player.TakeDamage(z.Stats.Damage);
                    z.AttackTimer = GameConfig.ZombieAttackCooldown;
                }

                if (z.HitTimer > 0f) z.HitTimer -= dt;
            }

            for (int i = 0; i < count; i++)
            {
                Zombie a = _active[i];
                for (int j = i + 1; j < count; j++)
                {
                    Zombie b = _active[j];
                    float dx = a.Pos.x - b.Pos.x;
                    float dz = a.Pos.z - b.Pos.z;
                    float min = a.Stats.Radius + b.Stats.Radius;
                    float d2 = dx * dx + dz * dz;
                    if (d2 >= min * min || d2 < 0.0001f) continue;

                    float d = Mathf.Sqrt(d2);
                    float push = (min - d) * 0.5f / d;
                    a.Pos.x += dx * push;
                    a.Pos.z += dz * push;
                    b.Pos.x -= dx * push;
                    b.Pos.z -= dz * push;
                }
            }

            for (int i = 0; i < count; i++)
            {
                Zombie z = _active[i];
                float rise = Mathf.Clamp01((now - z.SpawnTime) / GameConfig.ZombieRiseTime);
                float t = now * z.Stats.Speed * 2.4f + z.Phase;
                float s = z.Stats.Scale * rise;
                float squash = 1f + z.HitTimer * 2f;

                z.Tr.SetPositionAndRotation(
                    new Vector3(z.Pos.x, Mathf.Abs(Mathf.Sin(t)) * 0.1f * z.Stats.Scale, z.Pos.z),
                    Quaternion.Euler(0f, z.Yaw, Mathf.Sin(t) * 7f));
                z.Tr.localScale = new Vector3(s * squash, s / squash, s * squash);
            }
        }
    }
}
