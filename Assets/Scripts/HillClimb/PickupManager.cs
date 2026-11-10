using System.Collections.Generic;
using UnityEngine;
using PickupPool = ZombieWorld.ObjectPool<PeakRover.Pickup>;

namespace PeakRover
{
    public sealed class Pickup : MonoBehaviour
    {
        public Vector2 Pos;
        public float Phase;
        public bool IsFuel;
        public Transform Tr { get; private set; }

        void Awake()
        {
            Tr = transform;
        }
    }

    // Spawns coin rows and fuel cans ahead of the camera from pools and recycles them behind it.
    public sealed class PickupManager : MonoBehaviour
    {
        HillGame _game;
        Transform _cam;
        PickupPool _coins;
        PickupPool _fuel;
        readonly List<Pickup> _active = new List<Pickup>(64);
        float _nextCoinX;
        float _nextFuelX;

        public void Init(HillGame game, Transform cam)
        {
            _game = game;
            _cam = cam;

            var root = new GameObject("PickupPool").transform;
            root.SetParent(transform, false);
            _coins = new PickupPool(CreateTemplate("Coin", HillAssets.CoinMesh, false, root), root, HillConfig.CoinPoolPrewarm);
            _fuel = new PickupPool(CreateTemplate("FuelCan", HillAssets.FuelMesh, true, root), root, HillConfig.FuelPoolPrewarm);
        }

        public void ResetAll()
        {
            for (int i = _active.Count - 1; i >= 0; i--) Release(_active[i]);
            _active.Clear();
            _nextCoinX = 14f;
            _nextFuelX = 50f;
        }

        void Update()
        {
            float camX = _cam.position.x;
            while (_nextCoinX < camX + HillConfig.PickupAhead)
            {
                SpawnCoinRow(_nextCoinX);
                _nextCoinX += Random.Range(16f, 32f);
            }
            while (_nextFuelX < camX + HillConfig.PickupAhead)
            {
                SpawnFuel(_nextFuelX);
                _nextFuelX += Random.Range(90f, 140f);
            }

            bool playing = _game.State == HillState.Playing;
            Vector2 car = _game.Car.Position;
            float t = Time.time;

            for (int i = _active.Count - 1; i >= 0; i--)
            {
                Pickup p = _active[i];
                if (p.Pos.x < camX - HillConfig.PickupBehind)
                {
                    RemoveAt(i);
                    continue;
                }

                float radius = p.IsFuel ? HillConfig.FuelPickupRadius : HillConfig.CoinPickupRadius;
                if (playing && (p.Pos - car).sqrMagnitude <= radius * radius)
                {
                    if (p.IsFuel) _game.Car.AddFuel(HillConfig.FuelCanAmount);
                    else _game.AddCoin();
                    RemoveAt(i);
                    continue;
                }

                float bob = Mathf.Sin(t * 3f + p.Phase) * 0.12f;
                p.Tr.SetPositionAndRotation(
                    new Vector3(p.Pos.x, p.Pos.y + bob, 0f),
                    Quaternion.Euler(0f, p.IsFuel ? Mathf.Sin(t * 2f + p.Phase) * 25f : t * 180f + p.Phase * 57f, 0f));
            }
        }

        void SpawnCoinRow(float x)
        {
            for (int i = 0; i < 5; i++)
            {
                float cx = x + i * 1.7f;
                float arc = Mathf.Sin(i / 4f * Mathf.PI) * 0.8f;
                Place(_coins, false, new Vector2(cx, TerrainGen.Height(cx) + 1.3f + arc));
            }
        }

        void SpawnFuel(float x)
        {
            Place(_fuel, true, new Vector2(x, TerrainGen.Height(x) + 1.1f));
        }

        void Place(PickupPool pool, bool isFuel, Vector2 pos)
        {
            Pickup p = pool.Get();
            p.IsFuel = isFuel;
            p.Pos = pos;
            p.Phase = Random.value * 6.28f;
            p.Tr.position = new Vector3(pos.x, pos.y, 0f);
            _active.Add(p);
        }

        void RemoveAt(int index)
        {
            Pickup p = _active[index];
            int last = _active.Count - 1;
            _active[index] = _active[last];
            _active.RemoveAt(last);
            Release(p);
        }

        void Release(Pickup p)
        {
            (p.IsFuel ? _fuel : _coins).Release(p);
        }

        static Pickup CreateTemplate(string name, Mesh mesh, bool castShadows, Transform parent)
        {
            var go = new GameObject(name);
            go.SetActive(false);
            go.transform.SetParent(parent, false);
            HillAssets.AddRenderer(go, mesh, castShadows);
            return go.AddComponent<Pickup>();
        }
    }
}
