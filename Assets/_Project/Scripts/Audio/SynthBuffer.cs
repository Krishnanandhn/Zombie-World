using System;
using UnityEngine;

namespace Project.Audio
{
    public enum SynthWave
    {
        Sine,
        Saw,
        Square,
        Triangle
    }

    // Tiny offline synth: layer tones and filtered noise into a mono buffer, then bake to an AudioClip.
    public sealed class SynthBuffer
    {
        readonly float[] _data;
        readonly int _sampleRate;
        readonly System.Random _rng;

        public SynthBuffer(float seconds, int sampleRate, int seed)
        {
            _sampleRate = sampleRate;
            _data = new float[Mathf.Max(1, Mathf.CeilToInt(seconds * sampleRate))];
            _rng = new System.Random(seed);
        }

        public float Length => _data.Length / (float)_sampleRate;

        // lowpass is a one-pole coefficient in (0, 1]; 1 leaves the noise unfiltered.
        public SynthBuffer Noise(float start, float duration, float decay, float lowpass, float amp, float attack = 0.002f)
        {
            int i0 = Index(start);
            int n = Mathf.Min(Mathf.CeilToInt(duration * _sampleRate), _data.Length - i0);
            float y = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)_sampleRate;
                float noise = (float)(_rng.NextDouble() * 2.0 - 1.0);
                y += lowpass * (noise - y);
                _data[i0 + i] += y * Envelope(t, duration, decay, attack) * amp;
            }

            return this;
        }

        public SynthBuffer Tone(float start, float duration, float freqStart, float freqEnd, float decay, float amp,
            SynthWave wave = SynthWave.Sine, float vibratoHz = 0f, float vibratoDepth = 0f, float lowpass = 1f, float attack = 0.004f)
        {
            int i0 = Index(start);
            int n = Mathf.Min(Mathf.CeilToInt(duration * _sampleRate), _data.Length - i0);
            double phase = 0.0;
            float y = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)_sampleRate;
                float k = duration > 0f ? t / duration : 0f;
                float freq = Mathf.Lerp(freqStart, freqEnd, k);
                if (vibratoHz > 0f) freq *= 1f + Mathf.Sin(2f * Mathf.PI * vibratoHz * t) * vibratoDepth;

                phase += freq / _sampleRate;
                phase -= Math.Floor(phase);
                float s = Wave(wave, (float)phase);
                y += lowpass * (s - y);
                _data[i0 + i] += y * Envelope(t, duration, decay, attack) * amp;
            }

            return this;
        }

        public AudioClip ToClip(string name, float gain = 0.9f)
        {
            float peak = 0.0001f;
            for (int i = 0; i < _data.Length; i++) peak = Mathf.Max(peak, Mathf.Abs(_data[i]));

            var samples = new float[_data.Length];
            float scale = gain / Mathf.Max(1f, peak);
            for (int i = 0; i < samples.Length; i++) samples[i] = (float)Math.Tanh(_data[i] * scale * 1.2f);

            AudioClip clip = AudioClip.Create(name, samples.Length, 1, _sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        int Index(float seconds) => Mathf.Clamp(Mathf.RoundToInt(seconds * _sampleRate), 0, _data.Length - 1);

        static float Envelope(float t, float duration, float decay, float attack)
        {
            float a = attack > 0f ? Mathf.Min(1f, t / attack) : 1f;
            float release = Mathf.Clamp01((duration - t) / 0.01f);
            return a * release * Mathf.Exp(-decay * t);
        }

        static float Wave(SynthWave wave, float phase)
        {
            switch (wave)
            {
                case SynthWave.Saw: return phase * 2f - 1f;
                case SynthWave.Square: return phase < 0.5f ? 1f : -1f;
                case SynthWave.Triangle: return 1f - 4f * Mathf.Abs(phase - 0.5f);
                default: return Mathf.Sin(phase * 2f * Mathf.PI);
            }
        }
    }
}
