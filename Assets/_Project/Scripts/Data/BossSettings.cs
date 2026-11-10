using System;
using UnityEngine;

namespace Project.Data
{
    [Serializable]
    public sealed class BossSettings
    {
        [field: SerializeField] public float ChargeCooldown { get; internal set; } = 7f;
        [field: SerializeField] public float ChargeWindup { get; internal set; } = 1f;
        [field: SerializeField] public float ChargeSpeed { get; internal set; } = 16f;
        [field: SerializeField] public float ChargeDuration { get; internal set; } = 0.9f;
        [field: SerializeField] public float ChargeDamage { get; internal set; } = 35f;
        [field: SerializeField] public float SlamCooldown { get; internal set; } = 6f;
        [field: SerializeField] public float SlamWindup { get; internal set; } = 0.9f;
        [field: SerializeField] public float SlamRadius { get; internal set; } = 6.5f;
        [field: SerializeField] public float SlamDamage { get; internal set; } = 30f;
        [field: SerializeField] public EnemyDefinition Minion { get; internal set; }
        [field: SerializeField, Tooltip("Health fractions that trigger a minion summon.")]
        public float[] SummonThresholds { get; internal set; } = new[] { 0.66f, 0.33f };
        [field: SerializeField] public int SummonCount { get; internal set; } = 6;
        [field: SerializeField, Tooltip("Below this health fraction the boss moves and acts faster.")]
        public float EnrageFraction { get; internal set; } = 0.3f;
        [field: SerializeField] public float EnrageSpeedMultiplier { get; internal set; } = 1.35f;
    }
}
