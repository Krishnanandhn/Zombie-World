using UnityEngine;

namespace ZombieWorld
{
    public enum ZombieKind { Walker = 0, Runner = 1, Brute = 2 }

    public readonly struct ZombieStats
    {
        public readonly int Health;
        public readonly float Speed;
        public readonly int Damage;
        public readonly float Scale;
        public readonly float Radius;
        public readonly int Score;

        public ZombieStats(int health, float speed, int damage, float scale, float radius, int score)
        {
            Health = health;
            Speed = speed;
            Damage = damage;
            Scale = scale;
            Radius = radius;
            Score = score;
        }
    }

    // All tuning values live here.
    public static class GameConfig
    {
        // Arena
        public const float ArenaHalfSize = 30f;
        public const float PlayAreaHalfSize = 28f;

        // Player
        public const float PlayerSpeed = 5.5f;
        public const int PlayerMaxHealth = 100;
        public const float PlayerRadius = 0.4f;
        public const float FireInterval = 0.22f;
        public const float FireRange = 12f;
        public const float BulletSpeed = 24f;
        public const float BulletLife = 0.9f;
        public const float BulletRadius = 0.2f;
        public const int BulletDamage = 1;
        public const float BulletSpreadDegrees = 2f;
        public const int BulletPoolPrewarm = 24;

        // Zombies
        public const int MaxAliveZombies = 50;
        public const int ZombiePoolPrewarm = 12;
        public const float ZombieAttackCooldown = 0.9f;
        public const float ZombieRiseTime = 0.35f;

        public static readonly ZombieStats[] Zombies =
        {
            //                 hp  speed dmg scale radius score
            new ZombieStats(3, 2.0f, 8, 1.00f, 0.45f, 10),  // Walker
            new ZombieStats(2, 3.8f, 5, 0.85f, 0.40f, 15),  // Runner
            new ZombieStats(14, 1.4f, 18, 1.45f, 0.70f, 50), // Brute
        };

        // Waves
        public const float FirstWaveDelay = 1.5f;
        public const float WaveBreak = 3f;
        public const int WaveHeal = 15;
        public const float SpawnRadiusMin = 16f;
        public const float SpawnRadiusMax = 20f;
        public const float SpawnMinPlayerDistance = 12f;

        // Camera
        public static readonly Vector3 CameraOffset = new Vector3(0f, 15f, -9.5f);
        public const float CameraFov = 50f;
        public const float CameraFollowSharpness = 8f;

        // Effects
        public const int ChunkPoolPrewarm = 48;
        public const int MaxChunks = 96;
    }
}
