using System.Collections.Generic;
using Project.Core;
using UnityEngine;

namespace Project.Audio
{
    // Every sound is synthesised at startup, so the game ships without audio assets or licensing concerns.
    public static class SfxLibrary
    {
        const int Rate = 44100;

        public static Dictionary<SfxId, AudioClip> BuildAll()
        {
            var clips = new Dictionary<SfxId, AudioClip>();
            foreach (SfxId id in System.Enum.GetValues(typeof(SfxId)))
            {
                if (id == SfxId.None) continue;
                clips[id] = Build(id);
            }

            return clips;
        }

        static SynthBuffer Buf(float seconds, SfxId id) => new SynthBuffer(seconds, Rate, (int)id * 7919 + 13);

        static AudioClip Build(SfxId id)
        {
            SynthBuffer b;
            switch (id)
            {
                case SfxId.Pistol:
                    b = Buf(0.3f, id).Noise(0f, 0.3f, 24f, 0.55f, 1f).Tone(0f, 0.2f, 190f, 60f, 22f, 0.9f).Tone(0f, 0.05f, 2200f, 900f, 60f, 0.25f, SynthWave.Square, 0f, 0f, 0.4f);
                    break;
                case SfxId.Smg:
                    b = Buf(0.16f, id).Noise(0f, 0.16f, 42f, 0.6f, 0.9f).Tone(0f, 0.1f, 220f, 80f, 40f, 0.7f);
                    break;
                case SfxId.Shotgun:
                    b = Buf(0.6f, id).Noise(0f, 0.6f, 9f, 0.3f, 1f).Tone(0f, 0.4f, 130f, 40f, 12f, 1f).Noise(0f, 0.05f, 60f, 1f, 0.6f);
                    break;
                case SfxId.Rifle:
                    b = Buf(0.55f, id).Noise(0f, 0.5f, 14f, 0.7f, 0.9f).Tone(0f, 0.35f, 150f, 45f, 14f, 0.9f).Tone(0f, 0.12f, 1800f, 300f, 28f, 0.4f, SynthWave.Saw, 0f, 0f, 0.5f);
                    break;
                case SfxId.EmptyClick:
                    b = Buf(0.07f, id).Noise(0f, 0.07f, 90f, 1f, 0.6f).Tone(0f, 0.04f, 1500f, 900f, 60f, 0.3f, SynthWave.Square, 0f, 0f, 0.3f);
                    break;
                case SfxId.Reload:
                    b = Buf(0.35f, id).Noise(0f, 0.05f, 80f, 0.8f, 0.7f).Tone(0f, 0.08f, 700f, 500f, 40f, 0.3f, SynthWave.Square, 0f, 0f, 0.3f)
                        .Noise(0.18f, 0.06f, 80f, 0.7f, 0.8f).Tone(0.18f, 0.1f, 400f, 250f, 35f, 0.4f, SynthWave.Square, 0f, 0f, 0.3f);
                    break;
                case SfxId.ReloadDone:
                    b = Buf(0.15f, id).Noise(0f, 0.06f, 70f, 0.5f, 0.8f).Tone(0f, 0.12f, 260f, 120f, 30f, 0.7f);
                    break;
                case SfxId.WeaponSwitch:
                    b = Buf(0.12f, id).Noise(0f, 0.05f, 80f, 0.9f, 0.6f).Tone(0.03f, 0.08f, 600f, 900f, 30f, 0.25f, SynthWave.Triangle);
                    break;
                case SfxId.ZombieHit:
                    b = Buf(0.14f, id).Noise(0f, 0.12f, 38f, 0.25f, 0.9f).Tone(0f, 0.12f, 210f, 70f, 28f, 0.6f);
                    break;
                case SfxId.ZombieDeath:
                    b = Buf(0.6f, id).Tone(0f, 0.55f, 130f, 45f, 5f, 0.7f, SynthWave.Saw, 7f, 0.08f, 0.12f).Noise(0f, 0.3f, 12f, 0.2f, 0.5f);
                    break;
                case SfxId.ZombieGroan:
                    b = Buf(0.9f, id).Tone(0f, 0.85f, 95f, 70f, 2.5f, 0.8f, SynthWave.Saw, 5f, 0.06f, 0.1f, 0.15f).Noise(0f, 0.8f, 3f, 0.08f, 0.35f, 0.2f);
                    break;
                case SfxId.SpitShot:
                    b = Buf(0.3f, id).Noise(0f, 0.25f, 14f, 0.35f, 0.7f).Tone(0f, 0.25f, 320f, 760f, 12f, 0.5f, SynthWave.Triangle, 18f, 0.15f);
                    break;
                case SfxId.PlayerHurt:
                    b = Buf(0.3f, id).Noise(0f, 0.25f, 18f, 0.3f, 0.8f).Tone(0f, 0.25f, 170f, 80f, 14f, 0.8f, SynthWave.Saw, 0f, 0f, 0.2f);
                    break;
                case SfxId.PlayerDeath:
                    b = Buf(1.2f, id).Tone(0f, 1.1f, 220f, 35f, 3.5f, 0.9f, SynthWave.Saw, 4f, 0.05f, 0.15f).Noise(0f, 0.6f, 6f, 0.2f, 0.6f);
                    break;
                case SfxId.Dash:
                    b = Buf(0.3f, id).Noise(0f, 0.28f, 8f, 0.18f, 0.9f, 0.05f).Tone(0f, 0.25f, 500f, 180f, 10f, 0.15f, SynthWave.Triangle);
                    break;
                case SfxId.Pickup:
                    b = Buf(0.35f, id).Tone(0f, 0.12f, 660f, 660f, 10f, 0.5f, SynthWave.Triangle).Tone(0.07f, 0.12f, 880f, 880f, 10f, 0.5f, SynthWave.Triangle)
                        .Tone(0.14f, 0.2f, 1320f, 1320f, 9f, 0.5f, SynthWave.Triangle);
                    break;
                case SfxId.WaveStart:
                    b = Buf(1.1f, id).Tone(0f, 1f, 110f, 110f, 2.2f, 0.7f, SynthWave.Saw, 0f, 0f, 0.2f, 0.12f)
                        .Tone(0f, 1f, 165f, 165f, 2.2f, 0.5f, SynthWave.Saw, 0f, 0f, 0.2f, 0.12f)
                        .Tone(0.3f, 0.7f, 220f, 220f, 3f, 0.4f, SynthWave.Saw, 0f, 0f, 0.2f, 0.1f);
                    break;
                case SfxId.WaveClear:
                    b = Buf(0.9f, id).Tone(0f, 0.5f, 440f, 440f, 5f, 0.5f, SynthWave.Triangle).Tone(0.12f, 0.5f, 554f, 554f, 5f, 0.5f, SynthWave.Triangle)
                        .Tone(0.24f, 0.6f, 659f, 659f, 4f, 0.5f, SynthWave.Triangle);
                    break;
                case SfxId.BossRoar:
                    b = Buf(1.8f, id).Tone(0f, 1.7f, 62f, 40f, 1.4f, 1f, SynthWave.Saw, 13f, 0.12f, 0.15f, 0.12f).Noise(0f, 1.6f, 1.6f, 0.15f, 0.7f, 0.15f)
                        .Tone(0f, 1.5f, 124f, 80f, 2f, 0.4f, SynthWave.Square, 9f, 0.1f, 0.1f, 0.1f);
                    break;
                case SfxId.BossCharge:
                    b = Buf(0.9f, id).Noise(0f, 0.9f, 0f, 0.15f, 0.7f, 0.5f).Tone(0f, 0.9f, 70f, 220f, 0f, 0.7f, SynthWave.Saw, 0f, 0f, 0.15f, 0.5f);
                    break;
                case SfxId.BossSlam:
                    b = Buf(1f, id).Tone(0f, 0.9f, 80f, 24f, 4.5f, 1f).Noise(0f, 0.7f, 6f, 0.12f, 1f).Noise(0f, 0.05f, 60f, 1f, 0.6f);
                    break;
                case SfxId.BossDeath:
                    b = Buf(2.4f, id).Tone(0f, 2.3f, 90f, 25f, 1.2f, 1f, SynthWave.Saw, 6f, 0.1f, 0.12f, 0.05f).Noise(0f, 1.2f, 2.5f, 0.15f, 0.8f)
                        .Tone(0.1f, 1.5f, 55f, 20f, 2f, 0.7f);
                    break;
                case SfxId.UpgradeOffer:
                    b = Buf(0.6f, id).Tone(0f, 0.5f, 880f, 880f, 5f, 0.35f, SynthWave.Sine, 14f, 0.01f).Tone(0.05f, 0.5f, 1320f, 1320f, 5f, 0.3f, SynthWave.Sine, 12f, 0.01f)
                        .Tone(0.1f, 0.5f, 1760f, 1760f, 6f, 0.2f);
                    break;
                case SfxId.UpgradePick:
                    b = Buf(0.5f, id).Tone(0f, 0.4f, 523f, 523f, 7f, 0.4f, SynthWave.Triangle).Tone(0f, 0.4f, 659f, 659f, 7f, 0.35f, SynthWave.Triangle)
                        .Tone(0f, 0.4f, 784f, 784f, 7f, 0.35f, SynthWave.Triangle).Tone(0.08f, 0.4f, 1047f, 1047f, 8f, 0.3f, SynthWave.Triangle);
                    break;
                case SfxId.UiClick:
                    b = Buf(0.07f, id).Tone(0f, 0.06f, 1100f, 700f, 50f, 0.5f, SynthWave.Triangle).Noise(0f, 0.02f, 100f, 1f, 0.2f);
                    break;
                case SfxId.Victory:
                    b = Buf(2f, id).Tone(0f, 0.5f, 523f, 523f, 4f, 0.5f, SynthWave.Triangle).Tone(0.25f, 0.5f, 659f, 659f, 4f, 0.5f, SynthWave.Triangle)
                        .Tone(0.5f, 0.5f, 784f, 784f, 4f, 0.5f, SynthWave.Triangle).Tone(0.8f, 1.1f, 1047f, 1047f, 2.5f, 0.55f, SynthWave.Triangle)
                        .Tone(0.8f, 1.1f, 784f, 784f, 2.5f, 0.35f, SynthWave.Triangle);
                    break;
                case SfxId.GameOver:
                    b = Buf(1.6f, id).Tone(0f, 0.6f, 330f, 330f, 3f, 0.5f, SynthWave.Saw, 0f, 0f, 0.2f).Tone(0.4f, 0.6f, 262f, 262f, 3f, 0.5f, SynthWave.Saw, 0f, 0f, 0.2f)
                        .Tone(0.8f, 0.8f, 196f, 196f, 2.2f, 0.55f, SynthWave.Saw, 0f, 0f, 0.2f);
                    break;
                default:
                    b = Buf(0.05f, id);
                    break;
            }

            return b.ToClip("sfx_" + id);
        }
    }
}
