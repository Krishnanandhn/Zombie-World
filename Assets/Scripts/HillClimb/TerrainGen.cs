using UnityEngine;

namespace PeakRover
{
    // Deterministic height function so chunk meshes, colliders and pickups always agree.
    public static class TerrainGen
    {
        public static float Seed;

        public static float Height(float x)
        {
            float t = Mathf.Clamp01(x / 700f);
            float amp = Mathf.Lerp(1.6f, 5.5f, t);
            float s = Seed;

            float h = Mathf.Sin(x * 0.045f + s) * amp
                    + Mathf.Sin(x * 0.11f + s * 1.7f) * amp * 0.35f
                    + Mathf.Sin(x * 0.021f + s * 0.6f) * amp * 1.1f
                    + (Mathf.PerlinNoise(x * 0.35f, s) - 0.5f) * 0.7f
                    + x * 0.035f;

            float flatBlend = Mathf.SmoothStep(0f, 1f, (x - 18f) / 30f);
            return h * flatBlend;
        }

        public static float Hash01(int n)
        {
            unchecked
            {
                n = (n << 13) ^ n;
                int m = (n * (n * n * 15731 + 789221) + 1376312589) & 0x7fffffff;
                return m / 2147483647f;
            }
        }
    }
}
