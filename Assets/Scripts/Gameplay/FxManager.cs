using System.Collections.Generic;
using UnityEngine;

namespace ZombieWorld
{
    public sealed class Chunk : MonoBehaviour
    {
        public Vector3 Pos;
        public Vector3 Vel;
        public float Life;
        public float Size;
        public Transform Tr { get; private set; }

        void Awake()
        {
            Tr = transform;
        }
    }

    // Pooled blood chunks for hit/death bursts.
    public sealed class FxManager : MonoBehaviour
    {
        ObjectPool<Chunk> _pool;
        readonly List<Chunk> _active = new List<Chunk>(GameConfig.MaxChunks);

        public void Init()
        {
            var poolRoot = new GameObject("ChunkPool").transform;
            poolRoot.SetParent(transform, false);

            var template = new GameObject("Chunk");
            template.SetActive(false);
            template.transform.SetParent(poolRoot, false);
            LowPolyAssets.AddRenderer(template, LowPolyAssets.ChunkMesh, false);
            var chunk = template.AddComponent<Chunk>();
            _pool = new ObjectPool<Chunk>(chunk, poolRoot, GameConfig.ChunkPoolPrewarm);
        }

        public void Burst(Vector3 position, int count, float size)
        {
            for (int i = 0; i < count; i++)
            {
                if (_active.Count >= GameConfig.MaxChunks) return;

                Chunk c = _pool.Get();
                c.Pos = position;
                c.Vel = new Vector3(Random.Range(-3f, 3f), Random.Range(3f, 6f), Random.Range(-3f, 3f));
                c.Life = Random.Range(0.5f, 0.8f);
                c.Size = size * Random.Range(0.6f, 1.2f);
                c.Tr.SetPositionAndRotation(position, Random.rotation);
                _active.Add(c);
            }
        }

        void Update()
        {
            float dt = Time.deltaTime;
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                Chunk c = _active[i];
                c.Life -= dt;
                if (c.Life <= 0f)
                {
                    int last = _active.Count - 1;
                    _active[i] = _active[last];
                    _active.RemoveAt(last);
                    _pool.Release(c);
                    continue;
                }

                c.Vel.y -= 18f * dt;
                c.Pos += c.Vel * dt;
                if (c.Pos.y < 0.05f)
                {
                    c.Pos.y = 0.05f;
                    c.Vel.x = 0f;
                    c.Vel.z = 0f;
                    c.Vel.y = 0f;
                }

                float s = c.Size * Mathf.Clamp01(c.Life / 0.3f);
                c.Tr.position = c.Pos;
                c.Tr.localScale = new Vector3(s, s, s);
            }
        }
    }
}
