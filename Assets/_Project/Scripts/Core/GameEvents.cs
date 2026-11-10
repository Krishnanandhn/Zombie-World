using UnityEngine;

namespace Project.Core
{
    // Struct events published on the EventBus. UI, audio and FX react to these; gameplay never calls them directly.

    public struct GameStateChanged
    {
        public GameState Previous;
        public GameState Current;
    }

    public struct RunStarted
    {
        public int TotalWaves;
    }

    public struct RunEnded
    {
        public bool Victory;
        public int Score;
        public int Kills;
        public int Wave;
        public float Duration;
        public bool NewBest;
    }

    public struct WaveStarted
    {
        public int Wave;
        public int TotalWaves;
        public bool IsBoss;
    }

    public struct WaveCleared
    {
        public int Wave;
        public int TotalWaves;
    }

    public struct WaveCountdown
    {
        public int Wave;
        public float SecondsLeft;
    }

    public struct EnemiesRemainingChanged
    {
        public int Remaining;
        public int Total;
    }

    public struct EnemyDamaged
    {
        public Vector3 Position;
        public float Amount;
        public bool IsCrit;
        public bool IsBoss;
        public Color BloodColor;
    }

    public struct EnemyKilled
    {
        public Vector3 Position;
        public int Score;
        public bool IsBoss;
        public float Scale;
        public Color BloodColor;
    }

    public struct ScoreChanged
    {
        public int Score;
        public int Kills;
    }

    public struct PlayerHealthChanged
    {
        public float Current;
        public float Max;
    }

    public struct PlayerDamaged
    {
        public Vector3 Position;
        public float Amount;
    }

    public struct PlayerDied
    {
        public Vector3 Position;
    }

    public struct PlayerDashed
    {
        public Vector3 Position;
        public float Cooldown;
    }

    public struct WeaponFired
    {
        public Vector3 Muzzle;
        public Vector3 Direction;
        public SfxId Sfx;
        public float ShakeStrength;
        public Color Tint;
    }

    public struct WeaponAmmoChanged
    {
        public string WeaponName;
        public int Ammo;
        public int Magazine;
        public int Slot;
        public int SlotCount;
    }

    public struct ReloadStarted
    {
        public float Duration;
    }

    public struct ReloadFinished
    {
    }

    public struct WeaponEmptyClick
    {
    }

    public struct ProjectileImpact
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Color Tint;
    }

    public struct BossSpawned
    {
        public string Name;
        public float MaxHealth;
    }

    public struct BossHealthChanged
    {
        public float Current;
        public float Max;
    }

    public struct BossDefeated
    {
        public Vector3 Position;
    }

    public struct BossSlam
    {
        public Vector3 Position;
        public float Radius;
    }

    public struct BossCharging
    {
        public Vector3 Position;
    }

    public struct BossRoar
    {
        public Vector3 Position;
    }

    public struct EnemySpawned
    {
        public Vector3 Position;
        public bool IsBoss;
    }

    public struct EnemyProjectileFired
    {
        public Vector3 Position;
    }

    public struct PickupCollected
    {
        public Vector3 Position;
        public float Amount;
    }

    public struct CameraShakeRequested
    {
        public float Strength;
        public float Duration;
    }

    public struct HitStopRequested
    {
        public float Duration;
    }

    public struct UpgradeChosen
    {
        public string Id;
        public string DisplayName;
    }
}
