using System.Collections.Generic;
using UnityEngine;

namespace ZombieWorld
{
    public sealed class Bullet : MonoBehaviour
    {
        public Vector3 Pos;
        public Vector3 Dir;
        public float Life;
        public Transform Tr { get; private set; }

        void Awake()
        {
            Tr = transform;
        }
    }

    public sealed class BulletManager : MonoBehaviour
    {
        GameManager _gm;
        ObjectPool<Bullet> _pool;
        readonly List<Bullet> _active = new List<Bullet>(GameConfig.BulletPoolPrewarm);

        public int ActiveCount => _active.Count;
        public int PooledTotal => _pool.TotalCreated;

        public void Init(GameManager gm)
        {
            _gm = gm;
            var poolRoot = new GameObject("BulletPool").transform;
            poolRoot.SetParent(transform, false);

            var template = new GameObject("Bullet");
            template.SetActive(false);
            template.transform.SetParent(poolRoot, false);
            LowPolyAssets.AddRenderer(template, LowPolyAssets.BulletMesh, false);
            var bullet = template.AddComponent<Bullet>();
            _pool = new ObjectPool<Bullet>(bullet, poolRoot, GameConfig.BulletPoolPrewarm);
        }

        public void Spawn(Vector3 position, Vector3 direction)
        {
            Bullet b = _pool.Get();
            b.Pos = position;
            b.Dir = direction;
            b.Life = GameConfig.BulletLife;
            b.Tr.SetPositionAndRotation(position, Quaternion.LookRotation(direction));
            _active.Add(b);
        }

        public void ClearAll()
        {
            for (int i = _active.Count - 1; i >= 0; i--) _pool.Release(_active[i]);
            _active.Clear();
        }

        void Remove(int index)
        {
            Bullet b = _active[index];
            int last = _active.Count - 1;
            _active[index] = _active[last];
            _active.RemoveAt(last);
            _pool.Release(b);
        }

        void Update()
        {
            if (_gm.State != GameState.Playing) return;

            float dt = Time.deltaTime;
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                Bullet b = _active[i];
                b.Pos += b.Dir * (GameConfig.BulletSpeed * dt);
                b.Life -= dt;

                Zombie hit = _gm.Zombies.FindHit(b.Pos, GameConfig.BulletRadius);
                if (hit != null)
                {
                    _gm.Fx.Burst(b.Pos, 3, 0.6f);
                    _gm.Zombies.Damage(hit, GameConfig.BulletDamage, b.Dir);
                    Remove(i);
                    continue;
                }

                if (b.Life <= 0f)
                {
                    Remove(i);
                    continue;
                }

                b.Tr.position = b.Pos;
            }
        }
    }
}
