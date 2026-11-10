using UnityEngine;
using ZombieWorld;

namespace PeakRover
{
    public static class HillPal
    {
        public const int GrassA = 0;
        public const int GrassB = 1;
        public const int DirtA = 2;
        public const int DirtB = 3;
        public const int CarBody = 4;
        public const int White = 5;
        public const int Dark = 6;
        public const int Glass = 7;
        public const int Gold = 8;
        public const int FuelRed = 9;
        public const int Rim = 10;
        public const int Trunk = 11;
        public const int FoliageA = 12;
        public const int FoliageB = 13;
        public const int Skin = 14;
        public const int Mountain = 15;
    }

    // Every mesh and texture is generated in code, so the game ships with no third-party assets.
    public static class HillAssets
    {
        static readonly uint[] PaletteHex =
        {
            0x7BC043, 0x6AAE3A, 0xB07A4F, 0x9C6A42,
            0xE8523A, 0xF4F4F4, 0x22262E, 0x7FD1F2,
            0xFFC83D, 0xD62828, 0xCFD8DC, 0x6B4A32,
            0x2E8B57, 0x3FA66B, 0xF1C27D, 0x8EA0BF,
        };

        public static Material Material { get; private set; }
        public static PhysicsMaterial2D GroundMaterial { get; private set; }
        public static PhysicsMaterial2D WheelMaterial { get; private set; }
        public static PhysicsMaterial2D ChassisMaterial { get; private set; }
        public static Mesh CarMesh { get; private set; }
        public static Mesh WheelMesh { get; private set; }
        public static Mesh CoinMesh { get; private set; }
        public static Mesh FuelMesh { get; private set; }
        public static Mesh MountainMesh { get; private set; }
        public static Mesh HillMesh { get; private set; }
        public static Mesh CloudMesh { get; private set; }

        public const float MountainPeriod = 80f;
        public const float HillPeriod = 60f;
        public const float CloudPeriod = 90f;

        public static void Build()
        {
            Material = LowPolyAssets.CreateMaterial(LowPolyAssets.CreatePaletteTexture(PaletteHex));
            Material.name = "PeakRoverPalette";

            GroundMaterial = new PhysicsMaterial2D("Ground") { friction = HillConfig.WheelFriction, bounciness = 0f };
            WheelMaterial = new PhysicsMaterial2D("Wheel") { friction = HillConfig.WheelFriction, bounciness = 0f };
            ChassisMaterial = new PhysicsMaterial2D("Chassis") { friction = 0.3f, bounciness = 0f };

            CarMesh = BuildCar();
            WheelMesh = BuildWheel();
            CoinMesh = new LowPolyMeshBuilder()
                .AddCylinderZ(Vector3.zero, 0.35f, 0.12f, 8, HillPal.Gold)
                .Build("Coin");
            FuelMesh = BuildFuelCan();

            MountainMesh = BuildPeakLayer("Mountains", 6, MountainPeriod, 25f, 45f, 60f, 1.9f,
                HillPal.Mountain, HillPal.Mountain, true, 11);
            HillMesh = BuildPeakLayer("Hills", 6, HillPeriod, 8f, 14f, 30f, 2.4f,
                HillPal.FoliageA, HillPal.FoliageB, false, 23);
            CloudMesh = BuildClouds();
        }

        public static MeshRenderer AddRenderer(GameObject go, Mesh mesh, bool castShadows)
        {
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = Material;
            mr.shadowCastingMode = castShadows
                ? UnityEngine.Rendering.ShadowCastingMode.On
                : UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = true;
            mr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            return mr;
        }

        static Mesh BuildCar()
        {
            var b = new LowPolyMeshBuilder();
            b.AddBox(new Vector3(0f, 0.05f, 0f), new Vector3(2.4f, 0.5f, 1.1f), HillPal.CarBody);
            b.AddBox(new Vector3(0.85f, 0.38f, 0f), new Vector3(0.7f, 0.12f, 1.0f), HillPal.CarBody);
            b.AddBox(new Vector3(0f, 0.12f, 0.56f), new Vector3(1.6f, 0.12f, 0.02f), HillPal.White);
            b.AddBox(new Vector3(0f, 0.12f, -0.56f), new Vector3(1.6f, 0.12f, 0.02f), HillPal.White);

            b.AddBox(new Vector3(1.25f, -0.12f, 0f), new Vector3(0.12f, 0.2f, 1.2f), HillPal.Dark);
            b.AddBox(new Vector3(-1.25f, -0.12f, 0f), new Vector3(0.12f, 0.2f, 1.2f), HillPal.Dark);
            b.AddBox(new Vector3(1.21f, 0.15f, 0.35f), new Vector3(0.06f, 0.14f, 0.2f), HillPal.Gold);
            b.AddBox(new Vector3(1.21f, 0.15f, -0.35f), new Vector3(0.06f, 0.14f, 0.2f), HillPal.Gold);
            b.AddBox(new Vector3(-1.21f, 0.15f, 0.35f), new Vector3(0.06f, 0.12f, 0.2f), HillPal.FuelRed);
            b.AddBox(new Vector3(-1.21f, 0.15f, -0.35f), new Vector3(0.06f, 0.12f, 0.2f), HillPal.FuelRed);

            b.AddBox(new Vector3(0.1f, 0.7f, 0.5f), new Vector3(0.07f, 0.8f, 0.07f), HillPal.Dark);
            b.AddBox(new Vector3(0.1f, 0.7f, -0.5f), new Vector3(0.07f, 0.8f, 0.07f), HillPal.Dark);
            b.AddBox(new Vector3(-0.9f, 0.7f, 0.5f), new Vector3(0.07f, 0.8f, 0.07f), HillPal.Dark);
            b.AddBox(new Vector3(-0.9f, 0.7f, -0.5f), new Vector3(0.07f, 0.8f, 0.07f), HillPal.Dark);
            b.AddBox(new Vector3(-0.4f, 1.1f, 0f), new Vector3(1.2f, 0.07f, 1.1f), HillPal.Dark);
            b.AddBox(new Vector3(0.1f, 0.75f, 0f), new Vector3(0.05f, 0.6f, 0.9f), HillPal.Glass);

            b.AddBox(new Vector3(-0.4f, 0.45f, 0f), new Vector3(0.35f, 0.45f, 0.5f), HillPal.Gold);
            b.AddBox(new Vector3(-0.38f, 0.8f, 0f), new Vector3(0.34f, 0.34f, 0.34f), HillPal.Skin);
            b.AddBox(new Vector3(-0.38f, 0.93f, 0f), new Vector3(0.4f, 0.2f, 0.4f), HillPal.White);
            return b.Build("Car");
        }

        static Mesh BuildWheel()
        {
            const int segments = 10;
            float r = HillConfig.WheelRadius;
            var b = new LowPolyMeshBuilder();
            b.AddCylinderZ(Vector3.zero, 0.07f, 1.5f, 6, HillPal.Dark);
            for (int side = -1; side <= 1; side += 2)
            {
                float z = side * 0.62f;
                b.AddCylinderZ(new Vector3(0f, 0f, z), r, 0.3f, segments, HillPal.Dark);
                b.AddCylinderZ(new Vector3(0f, 0f, z), r * 0.58f, 0.32f, segments, HillPal.Rim);
                b.AddBox(new Vector3(0f, 0f, z), new Vector3(0.09f, r * 0.9f, 0.34f), HillPal.White);
            }
            return b.Build("Wheel");
        }

        static Mesh BuildFuelCan()
        {
            var b = new LowPolyMeshBuilder();
            b.AddBox(Vector3.zero, new Vector3(0.5f, 0.7f, 0.3f), HillPal.FuelRed);
            b.AddBox(Vector3.zero, new Vector3(0.52f, 0.16f, 0.32f), HillPal.White);
            b.AddBox(new Vector3(0.12f, 0.42f, 0f), new Vector3(0.18f, 0.14f, 0.14f), HillPal.Dark);
            return b.Build("FuelCan");
        }

        // One period of peaks repeated 'periods' times; the backdrop snaps it by whole periods to loop forever.
        static Mesh BuildPeakLayer(string name, int periods, float period, float minH, float maxH, float baseDepth,
            float widthMul, int colorA, int colorB, bool snow, int seed)
        {
            var b = new LowPolyMeshBuilder();
            var rng = new System.Random(seed);
            const int peaks = 3;
            var xs = new float[peaks];
            var hs = new float[peaks];
            for (int k = 0; k < peaks; k++)
            {
                xs[k] = (k + 0.15f + (float)rng.NextDouble() * 0.7f) * period / peaks;
                hs[k] = Mathf.Lerp(minH, maxH, (float)rng.NextDouble());
            }

            for (int p = 0; p < periods; p++)
            {
                float offset = (p - periods / 2) * period;
                for (int k = 0; k < peaks; k++)
                {
                    float h = hs[k];
                    float width = h * widthMul;
                    int color = (k & 1) == 0 ? colorA : colorB;
                    Vector3 baseCenter = new Vector3(offset + xs[k], -baseDepth, 0f);
                    b.AddPyramid(baseCenter, width, h + baseDepth, color);

                    if (snow)
                    {
                        Vector3 capBase = new Vector3(offset + xs[k], h * 0.7f, 0f);
                        b.AddPyramid(capBase, width * 0.32f, h * 0.3f + 0.1f, HillPal.White);
                    }
                }
            }

            return b.Build(name);
        }

        static Mesh BuildClouds()
        {
            var b = new LowPolyMeshBuilder();
            var rng = new System.Random(77);
            const int periods = 6;
            for (int p = 0; p < periods; p++)
            {
                float offset = (p - periods / 2) * HillAssets.CloudPeriod;
                for (int c = 0; c < 2; c++)
                {
                    float cx = offset + (c + 0.2f + (float)rng.NextDouble() * 0.5f) * CloudPeriod * 0.5f;
                    float cy = 4f + (float)rng.NextDouble() * 8f;
                    float w = 7f + (float)rng.NextDouble() * 5f;
                    b.AddBox(new Vector3(cx, cy, 0f), new Vector3(w, 1.8f, 3f), HillPal.White);
                    b.AddBox(new Vector3(cx - w * 0.2f, cy + 1.2f, 0f), new Vector3(w * 0.55f, 1.6f, 2.6f), HillPal.White);
                    b.AddBox(new Vector3(cx + w * 0.25f, cy + 0.9f, 0f), new Vector3(w * 0.4f, 1.3f, 2.4f), HillPal.White);
                }
            }
            return b.Build("Clouds");
        }
    }
}
