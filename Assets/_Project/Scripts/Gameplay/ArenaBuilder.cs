using System.Collections.Generic;
using Project.Data;
using UnityEngine;

namespace Project.Gameplay
{
    // Procedural graveyard arena: ground, perimeter wall, cover props and street lamps.
    public static class ArenaBuilder
    {
        const float WallHeight = 3.6f;
        const float WallThickness = 1.6f;
        const float SafeRadius = 7f;
        const int PropCount = 30;
        const int LampCount = 4;

        static readonly Color Concrete = new Color(0.28f, 0.29f, 0.3f);
        static readonly Color Wood = new Color(0.36f, 0.24f, 0.14f);
        static readonly Color Stone = new Color(0.4f, 0.42f, 0.44f);
        static readonly Color Rust = new Color(0.45f, 0.2f, 0.12f);
        static readonly Color BarkColor = new Color(0.18f, 0.13f, 0.1f);

        public static Transform Build(LevelDefinition level, int seed)
        {
            var rng = new System.Random(seed);
            var root = new GameObject("Arena").transform;
            float half = level.ArenaHalfSize;

            BuildGround(root, half);
            BuildWalls(root, half);
            BuildProps(root, half, rng);
            BuildLamps(root, half);
            return root;
        }

        static void BuildGround(Transform root, float half)
        {
            float size = (half + 40f) * 2f;
            Material mat = MaterialLibrary.LitTextured(new Color(0.55f, 0.58f, 0.52f), ProceduralTextures.Ground, new Vector2(size / 8f, size / 8f), 0.05f);
            Transform ground = Solid("Ground", PrimitiveMeshes.Cube, root, new Vector3(0f, -0.5f, 0f), new Vector3(size, 1f, size), mat);
            ground.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        static void BuildWalls(Transform root, float half)
        {
            Material wall = MaterialLibrary.Lit(Concrete, 0.1f);
            Material cap = MaterialLibrary.Lit(new Color(0.18f, 0.19f, 0.2f), 0.1f);
            Material warning = MaterialLibrary.Emissive(new Color(1f, 0.15f, 0.08f), 2.2f);
            float outer = half + WallThickness * 0.5f;
            float length = half * 2f + WallThickness * 2f;

            for (int side = 0; side < 4; side++)
            {
                bool alongX = side < 2;
                float sign = side % 2 == 0 ? 1f : -1f;
                Vector3 pos = alongX ? new Vector3(0f, WallHeight * 0.5f, sign * outer) : new Vector3(sign * outer, WallHeight * 0.5f, 0f);
                Vector3 scale = alongX ? new Vector3(length, WallHeight, WallThickness) : new Vector3(WallThickness, WallHeight, length);
                Solid("Wall" + side, PrimitiveMeshes.Cube, root, pos, scale, wall);

                Vector3 capPos = pos + Vector3.up * (WallHeight * 0.5f + 0.15f);
                Vector3 capScale = alongX ? new Vector3(length, 0.3f, WallThickness + 0.3f) : new Vector3(WallThickness + 0.3f, 0.3f, length);
                ModelFactory.Part("WallCap" + side, PrimitiveMeshes.Cube, root, capPos, capScale, cap);

                for (float t = -half + 5f; t <= half - 5f; t += 10f)
                {
                    Vector3 lightPos = alongX
                        ? new Vector3(t, 2.6f, sign * (half - 0.02f))
                        : new Vector3(sign * (half - 0.02f), 2.6f, t);
                    Vector3 lightScale = alongX ? new Vector3(1.2f, 0.18f, 0.1f) : new Vector3(0.1f, 0.18f, 1.2f);
                    ModelFactory.Part("WarningLight", PrimitiveMeshes.Cube, root, lightPos, lightScale, warning, false);
                }
            }
        }

        static void BuildProps(Transform root, float half, System.Random rng)
        {
            var placed = new List<Vector3>(PropCount);
            float limit = half - 4f;
            int attempts = 0;

            while (placed.Count < PropCount && attempts++ < 600)
            {
                var p = new Vector3(Range(rng, -limit, limit), 0f, Range(rng, -limit, limit));
                if (p.magnitude < SafeRadius) continue;
                if (TooClose(placed, p, 5f)) continue;

                placed.Add(p);
                float yaw = Range(rng, 0f, 360f);
                switch (rng.Next(6))
                {
                    case 0: Crate(root, p, yaw, rng); break;
                    case 1: Barrels(root, p, yaw, rng); break;
                    case 2: Graves(root, p, yaw, rng); break;
                    case 3: Barrier(root, p, yaw); break;
                    case 4: DeadTree(root, p, yaw, rng); break;
                    default: WreckedCar(root, p, yaw); break;
                }
            }
        }

        static bool TooClose(List<Vector3> placed, Vector3 p, float distance)
        {
            for (int i = 0; i < placed.Count; i++)
            {
                if ((placed[i] - p).sqrMagnitude < distance * distance) return true;
            }

            return false;
        }

        static float Range(System.Random rng, float min, float max) => min + (float)rng.NextDouble() * (max - min);

        static void Crate(Transform root, Vector3 p, float yaw, System.Random rng)
        {
            Material wood = MaterialLibrary.Lit(Wood, 0.05f);
            Transform big = Solid("Crate", PrimitiveMeshes.Cube, root, p + Vector3.up * 0.8f, new Vector3(1.6f, 1.6f, 1.6f), wood);
            big.rotation = Quaternion.Euler(0f, yaw, 0f);
            if (rng.NextDouble() < 0.5)
            {
                Transform small = Solid("Crate", PrimitiveMeshes.Cube, root, p + new Vector3(1.5f, 0.5f, 0.4f), new Vector3(1f, 1f, 1f), wood);
                small.rotation = Quaternion.Euler(0f, yaw + 20f, 0f);
            }
        }

        static void Barrels(Transform root, Vector3 p, float yaw, System.Random rng)
        {
            Material rust = MaterialLibrary.Lit(Rust, 0.3f);
            Material toxic = MaterialLibrary.Emissive(new Color(0.4f, 1f, 0.2f), 1.2f);
            int count = 2 + rng.Next(2);
            for (int i = 0; i < count; i++)
            {
                Vector3 offset = Quaternion.Euler(0f, yaw + i * 120f, 0f) * Vector3.forward * (i == 0 ? 0f : 0.95f);
                Material m = rng.NextDouble() < 0.3 ? toxic : rust;
                Solid("Barrel", PrimitiveMeshes.Cylinder, root, p + offset + Vector3.up * 0.55f, new Vector3(0.9f, 0.55f, 0.9f), m);
            }
        }

        static void Graves(Transform root, Vector3 p, float yaw, System.Random rng)
        {
            Material stone = MaterialLibrary.Lit(Stone, 0.1f);
            int count = 3 + rng.Next(2);
            for (int i = 0; i < count; i++)
            {
                Vector3 offset = new Vector3(i * 1.3f - count * 0.65f, 0f, Range(rng, -0.4f, 0.4f));
                Transform t = Solid("Tombstone", PrimitiveMeshes.Cube, root, p + offset + Vector3.up * 0.65f, new Vector3(0.9f, 1.3f, 0.3f), stone);
                t.rotation = Quaternion.Euler(Range(rng, -6f, 6f), yaw + Range(rng, -12f, 12f), Range(rng, -8f, 8f));
            }
        }

        static void Barrier(Transform root, Vector3 p, float yaw)
        {
            Material mat = MaterialLibrary.Lit(new Color(0.5f, 0.5f, 0.48f), 0.05f);
            Transform b = Solid("Barrier", PrimitiveMeshes.Cube, root, p + Vector3.up * 0.55f, new Vector3(3.4f, 1.1f, 0.8f), mat);
            b.rotation = Quaternion.Euler(0f, yaw, 0f);
        }

        static void DeadTree(Transform root, Vector3 p, float yaw, System.Random rng)
        {
            Material bark = MaterialLibrary.Lit(BarkColor, 0.05f);
            Solid("Trunk", PrimitiveMeshes.Cylinder, root, p + Vector3.up * 2.2f, new Vector3(0.7f, 2.2f, 0.7f), bark);
            for (int i = 0; i < 4; i++)
            {
                float a = yaw + i * 90f + Range(rng, -20f, 20f);
                Transform branch = ModelFactory.Part("Branch", PrimitiveMeshes.Cube, root, p + Vector3.up * (3.4f + i * 0.35f), new Vector3(0.18f, 0.18f, 1.9f), bark);
                branch.rotation = Quaternion.Euler(-35f, a, 0f);
                branch.position += branch.forward * 0.9f;
            }
        }

        static void WreckedCar(Transform root, Vector3 p, float yaw)
        {
            Material body = MaterialLibrary.Lit(new Color(0.32f, 0.18f, 0.16f), 0.2f);
            Material glass = MaterialLibrary.Lit(new Color(0.08f, 0.1f, 0.12f), 0.6f);
            Material tire = MaterialLibrary.Lit(new Color(0.05f, 0.05f, 0.05f), 0.1f);

            var car = new GameObject("WreckedCar").transform;
            car.SetParent(root, false);
            car.position = p;
            car.rotation = Quaternion.Euler(0f, yaw, 0f);

            Solid("Body", PrimitiveMeshes.Cube, car, new Vector3(0f, 0.7f, 0f), new Vector3(2.1f, 0.8f, 4.3f), body, true);
            ModelFactory.Part("Cabin", PrimitiveMeshes.Cube, car, new Vector3(0f, 1.3f, -0.2f), new Vector3(1.8f, 0.6f, 2.1f), glass);
            for (int i = 0; i < 4; i++)
            {
                float x = i % 2 == 0 ? -1.05f : 1.05f;
                float z = i < 2 ? 1.4f : -1.4f;
                var wheel = ModelFactory.Part("Wheel", PrimitiveMeshes.Cylinder, car, new Vector3(x, 0.35f, z), new Vector3(0.7f, 0.15f, 0.7f), tire);
                wheel.localRotation = Quaternion.Euler(0f, 0f, 90f);
            }
        }

        static void BuildLamps(Transform root, float half)
        {
            Material pole = MaterialLibrary.Lit(new Color(0.12f, 0.12f, 0.13f), 0.3f);
            Material bulb = MaterialLibrary.Emissive(new Color(1f, 0.8f, 0.5f), 3f);
            float d = half * 0.55f;
            var spots = new[] { new Vector3(d, 0f, d), new Vector3(-d, 0f, d), new Vector3(d, 0f, -d), new Vector3(-d, 0f, -d) };

            for (int i = 0; i < LampCount; i++)
            {
                Vector3 p = spots[i];
                Solid("LampPole", PrimitiveMeshes.Cylinder, root, p + Vector3.up * 2.5f, new Vector3(0.22f, 2.5f, 0.22f), pole);
                ModelFactory.Part("LampBulb", PrimitiveMeshes.Sphere, root, p + Vector3.up * 5.1f, Vector3.one * 0.55f, bulb, false);

                var lightGo = new GameObject("LampLight");
                lightGo.transform.SetParent(root, false);
                lightGo.transform.position = p + Vector3.up * 4.8f;
                var light = lightGo.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = new Color(1f, 0.78f, 0.5f);
                light.range = 17f;
                light.intensity = 5f;
                light.shadows = LightShadows.None;
            }
        }

        // Visible mesh plus a matching collider so it blocks bullets and enemies.
        static Transform Solid(string name, Mesh mesh, Transform parent, Vector3 pos, Vector3 scale, Material mat, bool castShadows = true)
        {
            Transform t = ModelFactory.Part(name, mesh, parent, pos, scale, mat, castShadows);
            if (mesh == PrimitiveMeshes.Cylinder) t.gameObject.AddComponent<CapsuleCollider>();
            else t.gameObject.AddComponent<BoxCollider>();
            return t;
        }
    }
}
