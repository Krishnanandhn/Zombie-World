using Project.Core;
using UnityEngine;

namespace Project.Audio
{
    // Procedural loops: a bass pulse, kick and hats over a dark pad. Tempo and density rise with intensity.
    public static class MusicLibrary
    {
        const int Rate = 22050;

        public static AudioClip Build(MusicId id)
        {
            switch (id)
            {
                case MusicId.Menu: return Menu();
                case MusicId.Combat: return Pulse("music_combat", 112f, 0.7f, new[] { 0, 0, -2, -4 });
                case MusicId.Boss: return Pulse("music_boss", 146f, 1f, new[] { 0, 1, 0, -1 });
                default: return null;
            }
        }

        static float Hz(int semitonesFromA1) => 55f * Mathf.Pow(2f, semitonesFromA1 / 12f);

        static AudioClip Menu()
        {
            float bar = 4f * 60f / 64f;
            float length = bar * 4f;
            var b = new SynthBuffer(length, Rate, 101);
            int[] roots = { 0, -4, -2, -5 };
            for (int i = 0; i < roots.Length; i++)
            {
                float start = i * bar;
                b.Tone(start, bar, Hz(roots[i] + 12), Hz(roots[i] + 12), 0.2f, 0.45f, SynthWave.Triangle, 0.3f, 0.01f, 1f, 0.8f);
                b.Tone(start, bar, Hz(roots[i] + 19), Hz(roots[i] + 19), 0.2f, 0.25f, SynthWave.Sine, 0.2f, 0.01f, 1f, 0.8f);
                b.Tone(start, bar, Hz(roots[i] + 24), Hz(roots[i] + 24), 0.3f, 0.12f, SynthWave.Sine, 0.4f, 0.015f, 1f, 1f);
                b.Noise(start, bar, 0.5f, 0.02f, 0.18f, 1.2f);
            }

            return Finish(b, "music_menu", 0.8f);
        }

        static AudioClip Pulse(string name, float bpm, float intensity, int[] barRoots)
        {
            float beat = 60f / bpm;
            float step = beat * 0.5f;
            float bar = beat * 4f;
            var b = new SynthBuffer(bar * barRoots.Length, Rate, 202);

            for (int bi = 0; bi < barRoots.Length; bi++)
            {
                float barStart = bi * bar;
                int root = barRoots[bi];

                b.Tone(barStart, bar, Hz(root + 24), Hz(root + 24), 0.15f, 0.22f * intensity, SynthWave.Saw, 0.25f, 0.004f, 0.06f, 0.6f);
                b.Tone(barStart, bar, Hz(root + 31), Hz(root + 31), 0.15f, 0.14f * intensity, SynthWave.Triangle, 0.3f, 0.006f, 1f, 0.6f);

                for (int s = 0; s < 8; s++)
                {
                    float t = barStart + s * step;
                    int offset = (s == 3 || s == 6) ? 3 : (s == 5 ? 7 : 0);
                    b.Tone(t, step * 0.95f, Hz(root + offset + 12), Hz(root + offset + 12), 7f, 0.55f, SynthWave.Saw, 0f, 0f, 0.09f);

                    if (s % 2 == 1) b.Noise(t, step * 0.4f, 60f, 0.9f, 0.1f * intensity);
                }

                for (int k = 0; k < 4; k++)
                {
                    float t = barStart + k * beat;
                    b.Tone(t, 0.18f, 130f, 38f, 20f, 0.85f);
                    if (intensity >= 1f) b.Tone(t + step, 0.14f, 120f, 40f, 24f, 0.55f);
                }

                b.Noise(barStart + beat, 0.12f, 30f, 0.7f, 0.22f);
                b.Noise(barStart + beat * 3f, 0.12f, 30f, 0.7f, 0.22f);
            }

            return Finish(b, name, 0.85f);
        }

        static AudioClip Finish(SynthBuffer buffer, string name, float gain)
        {
            AudioClip clip = buffer.ToClip(name, gain);
            return clip;
        }
    }
}
