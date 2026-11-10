using UnityEngine;

namespace Project.Gameplay
{
    public static class ProceduralTextures
    {
        static Texture2D _softCircle;
        static Texture2D _ring;
        static Texture2D _ground;

        public static Texture2D SoftCircle => _softCircle != null ? _softCircle : _softCircle = Build(64, (u, v) =>
        {
            float r = Mathf.Clamp01(Mathf.Sqrt(u * u + v * v));
            float a = (1f - r) * (1f - r);
            return new Color(1f, 1f, 1f, a);
        }, false);

        public static Texture2D Ring => _ring != null ? _ring : _ring = Build(128, (u, v) =>
        {
            float r = Mathf.Sqrt(u * u + v * v);
            float edge = Mathf.Clamp01(1f - Mathf.Abs(r - 0.9f) / 0.08f);
            float fill = r < 0.9f ? 0.12f * (r / 0.9f) : 0f;
            return new Color(1f, 1f, 1f, Mathf.Max(edge, fill));
        }, false);

        public static Texture2D Ground => _ground != null ? _ground : _ground = BuildGround();

        // u and v are in [-1, 1] across the texture.
        static Texture2D Build(int size, System.Func<float, float, Color> pixel, bool mipmaps)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, mipmaps) { wrapMode = TextureWrapMode.Clamp };
            var colors = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float u = (x + 0.5f) / size * 2f - 1f;
                    float v = (y + 0.5f) / size * 2f - 1f;
                    colors[y * size + x] = pixel(u, v);
                }
            }

            tex.SetPixels32(colors);
            tex.Apply(mipmaps);
            return tex;
        }

        static Texture2D BuildGround()
        {
            const int size = 128;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, true)
            {
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear
            };

            var colors = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float n = Mathf.PerlinNoise(x * 0.09f, y * 0.09f) * 0.6f + Mathf.PerlinNoise(x * 0.4f, y * 0.4f) * 0.4f;
                    float shade = 0.55f + n * 0.45f;
                    bool seam = x % 64 < 1 || y % 64 < 1;
                    if (seam) shade *= 0.7f;
                    colors[y * size + x] = new Color(shade, shade * 0.98f, shade * 0.95f, 1f);
                }
            }

            tex.SetPixels32(colors);
            tex.Apply(true);
            return tex;
        }
    }
}
