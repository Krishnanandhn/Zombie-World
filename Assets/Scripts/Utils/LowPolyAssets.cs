using UnityEngine;
using UnityEngine.Rendering;

namespace ZombieWorld
{
    public static class Pal
    {
        public const int Ground = 0;
        public const int GroundAlt = 1;
        public const int Skin = 2;
        public const int ShirtPurple = 3;
        public const int Pants = 4;
        public const int ShirtRed = 5;
        public const int Orange = 6;
        public const int Player = 7;
        public const int Trunk = 8;
        public const int Foliage = 9;
        public const int Rock = 10;
        public const int BruteRed = 11;
        public const int Human = 12;
        public const int Blood = 13;
        public const int Dark = 14;
        public const int FoliageAlt = 15;
    }

    // Single palette texture + single material shared by every object in the game.
    public static class LowPolyAssets
    {
        static readonly uint[] PaletteHex =
        {
            0x3B4F3A, 0x46603F, 0x8FB36B, 0x6B4A8C,
            0x3A3F5C, 0xD9453D, 0xF2A33A, 0x3FA7D6,
            0x5B4032, 0x2F6B4A, 0x7A7F8A, 0x8C2F2F,
            0xF1D0B0, 0xB3262B, 0x22262E, 0x3C8A5A,
        };

        static Mesh[] _zombieMeshes;

        public static Material Material { get; private set; }
        public static Mesh PlayerMesh { get; private set; }
        public static Mesh BulletMesh { get; private set; }
        public static Mesh ChunkMesh { get; private set; }
        public static Mesh GroundMesh { get; private set; }
        public static Mesh PropsMesh { get; private set; }

        public static Mesh GetZombieMesh(ZombieKind kind) => _zombieMeshes[(int)kind];

        public static void Build()
        {
            Material = CreateMaterial(CreatePaletteTexture());

            _zombieMeshes = new[]
            {
                BuildHumanoid("Walker", Pal.Skin, Pal.ShirtPurple, Pal.Pants, true),
                BuildHumanoid("Runner", Pal.Skin, Pal.ShirtRed, Pal.Dark, true),
                BuildHumanoid("Brute", Pal.Skin, Pal.Dark, Pal.BruteRed, true),
            };
            PlayerMesh = BuildPlayer();

            BulletMesh = new LowPolyMeshBuilder()
                .AddBox(Vector3.zero, new Vector3(0.1f, 0.1f, 0.5f), Pal.Orange)
                .Build("Bullet");
            ChunkMesh = new LowPolyMeshBuilder()
                .AddBox(Vector3.zero, new Vector3(0.18f, 0.18f, 0.18f), Pal.Blood)
                .Build("Chunk");

            GroundMesh = BuildGround();
            PropsMesh = BuildProps();
        }

        public static MeshRenderer AddRenderer(GameObject go, Mesh mesh, bool castShadows)
        {
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = Material;
            mr.shadowCastingMode = castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
            mr.receiveShadows = true;
            mr.lightProbeUsage = LightProbeUsage.Off;
            mr.reflectionProbeUsage = ReflectionProbeUsage.Off;
            return mr;
        }

        static Texture2D CreatePaletteTexture() => CreatePaletteTexture(PaletteHex);

        public static Texture2D CreatePaletteTexture(uint[] hex)
        {
            var tex = new Texture2D(LowPolyMeshBuilder.PaletteSize, 1, TextureFormat.RGBA32, false)
            {
                name = "Palette",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
            };
            var pixels = new Color32[hex.Length];
            for (int i = 0; i < pixels.Length; i++)
            {
                uint h = hex[i];
                pixels[i] = new Color32((byte)(h >> 16), (byte)(h >> 8), (byte)h, 255);
            }
            tex.SetPixels32(pixels);
            tex.Apply(false, true);
            return tex;
        }

        public static Material CreateMaterial(Texture2D palette)
        {
            Shader shader = null;
            RenderPipelineAsset rp = GraphicsSettings.currentRenderPipeline;
            if (rp != null && rp.defaultMaterial != null) shader = rp.defaultMaterial.shader;
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");

            var mat = new Material(shader) { name = "LowPolyPalette" };
            mat.SetTexture("_BaseMap", palette);
            mat.SetTexture("_MainTex", palette);
            mat.SetColor("_BaseColor", Color.white);
            mat.SetFloat("_Smoothness", 0f);
            mat.SetFloat("_Metallic", 0f);
            return mat;
        }

        static Mesh BuildHumanoid(string name, int skin, int shirt, int pants, bool armsForward)
        {
            var b = new LowPolyMeshBuilder();
            b.AddBox(new Vector3(-0.13f, 0.25f, 0f), new Vector3(0.2f, 0.5f, 0.22f), pants);
            b.AddBox(new Vector3(0.13f, 0.25f, 0f), new Vector3(0.2f, 0.5f, 0.22f), pants);
            b.AddBox(new Vector3(0f, 0.8f, 0f), new Vector3(0.52f, 0.6f, 0.3f), shirt);
            b.AddBox(new Vector3(0f, 1.25f, 0.02f), new Vector3(0.34f, 0.34f, 0.34f), skin);
            b.AddBox(new Vector3(-0.08f, 1.28f, 0.2f), new Vector3(0.07f, 0.06f, 0.04f), Pal.ShirtRed);
            b.AddBox(new Vector3(0.08f, 1.28f, 0.2f), new Vector3(0.07f, 0.06f, 0.04f), Pal.ShirtRed);
            if (armsForward)
            {
                b.AddBox(new Vector3(-0.3f, 0.95f, 0.3f), new Vector3(0.13f, 0.13f, 0.6f), skin);
                b.AddBox(new Vector3(0.3f, 0.95f, 0.3f), new Vector3(0.13f, 0.13f, 0.6f), skin);
            }
            return b.Build(name);
        }

        static Mesh BuildPlayer()
        {
            var b = new LowPolyMeshBuilder();
            b.AddBox(new Vector3(-0.13f, 0.25f, 0f), new Vector3(0.2f, 0.5f, 0.22f), Pal.Dark);
            b.AddBox(new Vector3(0.13f, 0.25f, 0f), new Vector3(0.2f, 0.5f, 0.22f), Pal.Dark);
            b.AddBox(new Vector3(0f, 0.8f, 0f), new Vector3(0.52f, 0.6f, 0.3f), Pal.Player);
            b.AddBox(new Vector3(0f, 1.25f, 0.02f), new Vector3(0.34f, 0.34f, 0.34f), Pal.Human);
            b.AddBox(new Vector3(0f, 1.46f, 0.02f), new Vector3(0.38f, 0.1f, 0.38f), Pal.Orange);
            b.AddBox(new Vector3(-0.08f, 1.27f, 0.2f), new Vector3(0.06f, 0.06f, 0.04f), Pal.Dark);
            b.AddBox(new Vector3(0.08f, 1.27f, 0.2f), new Vector3(0.06f, 0.06f, 0.04f), Pal.Dark);
            b.AddBox(new Vector3(-0.3f, 0.9f, 0.22f), new Vector3(0.13f, 0.13f, 0.45f), Pal.Player);
            b.AddBox(new Vector3(0.3f, 0.9f, 0.22f), new Vector3(0.13f, 0.13f, 0.45f), Pal.Player);
            b.AddBox(new Vector3(0.18f, 0.95f, 0.55f), new Vector3(0.09f, 0.12f, 0.55f), Pal.Dark);
            return b.Build("Player");
        }

        static Mesh BuildGround()
        {
            var b = new LowPolyMeshBuilder();
            float size = GameConfig.ArenaHalfSize * 3f;
            b.AddBox(new Vector3(0f, -0.15f, 0f), new Vector3(size, 0.3f, size), Pal.Ground);

            var rng = new System.Random(1234);
            float half = GameConfig.ArenaHalfSize;
            for (int i = 0; i < 50; i++)
            {
                float x = ((float)rng.NextDouble() * 2f - 1f) * half;
                float z = ((float)rng.NextDouble() * 2f - 1f) * half;
                float w = 2f + (float)rng.NextDouble() * 5f;
                float d = 2f + (float)rng.NextDouble() * 5f;
                float yaw = (float)rng.NextDouble() * 90f;
                b.AddBox(new Vector3(x, 0.01f, z), new Vector3(w, 0.02f, d), Pal.GroundAlt, yaw);
            }
            return b.Build("Ground");
        }

        static Mesh BuildProps()
        {
            var b = new LowPolyMeshBuilder();
            var rng = new System.Random(4321);
            float Rand() => (float)rng.NextDouble();
            float border = GameConfig.ArenaHalfSize + 1f;

            int trees = 0;
            int attempts = 0;
            while (trees < 110 && attempts < 1000)
            {
                attempts++;
                float x = (Rand() * 2f - 1f) * (border + 14f);
                float z = (Rand() * 2f - 1f) * (border + 14f);
                if (Mathf.Max(Mathf.Abs(x), Mathf.Abs(z)) < border) continue;

                float s = 0.8f + Rand() * 0.8f;
                float yaw = Rand() * 90f;
                b.AddBox(new Vector3(x, 0.6f * s, z), new Vector3(0.4f * s, 1.2f * s, 0.4f * s), Pal.Trunk, yaw);
                b.AddPyramid(new Vector3(x, 1.0f * s, z), 2.2f * s, 1.8f * s, Pal.Foliage, yaw);
                b.AddPyramid(new Vector3(x, 2.0f * s, z), 1.6f * s, 1.6f * s, Pal.FoliageAlt, yaw);
                trees++;
            }

            for (int i = 0; i < 28; i++)
            {
                float x = (Rand() * 2f - 1f) * (GameConfig.PlayAreaHalfSize - 2f);
                float z = (Rand() * 2f - 1f) * (GameConfig.PlayAreaHalfSize - 2f);
                if (x * x + z * z < 36f) continue;

                float w = 0.6f + Rand() * 1.0f;
                float h = 0.15f + Rand() * 0.2f;
                b.AddBox(new Vector3(x, h * 0.35f, z), new Vector3(w, h, w * (0.7f + Rand() * 0.6f)), Pal.Rock, Rand() * 90f);
            }

            return b.Build("Props");
        }
    }
}
