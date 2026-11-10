using System.Collections.Generic;
using UnityEngine;

namespace Project.Data
{
    [CreateAssetMenu(menuName = "Zombie Survival/Level", fileName = "Level")]
    public sealed class LevelDefinition : ScriptableObject
    {
        [field: SerializeField] public string DisplayName { get; internal set; } = "Dead Yard";
        [field: SerializeField, Min(10f)] public float ArenaHalfSize { get; internal set; } = 30f;
        [field: SerializeField] public List<WaveDefinition> Waves { get; internal set; } = new List<WaveDefinition>();

        [field: Header("Pacing")]
        [field: SerializeField] public float FirstWaveDelay { get; internal set; } = 3f;
        [field: SerializeField] public float WaveClearDelay { get; internal set; } = 1.5f;
        [field: SerializeField, Min(1)] public int MaxAlive { get; internal set; } = 40;
        [field: SerializeField] public float MinSpawnDistance { get; internal set; } = 16f;

        [field: Header("Scaling per wave after the first")]
        [field: SerializeField] public float HealthScalingPerWave { get; internal set; } = 0.1f;
        [field: SerializeField] public float SpeedScalingPerWave { get; internal set; } = 0.02f;

        [field: Header("Rewards")]
        [field: SerializeField] public int ScorePerWaveCleared { get; internal set; } = 100;
        [field: SerializeField, Range(0f, 1f)] public float HealOnWaveClearFraction { get; internal set; } = 0.25f;
        [field: SerializeField, Min(1)] public int UpgradeChoices { get; internal set; } = 3;
    }
}
