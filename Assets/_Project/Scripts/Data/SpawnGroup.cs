using System;
using UnityEngine;

namespace Project.Data
{
    [Serializable]
    public sealed class SpawnGroup
    {
        [field: SerializeField] public EnemyDefinition Enemy { get; internal set; }
        [field: SerializeField, Min(1)] public int Count { get; internal set; } = 5;
        [field: SerializeField, Tooltip("Seconds after wave start before this group begins.")]
        public float StartDelay { get; internal set; }
        [field: SerializeField, Tooltip("Seconds between each spawn in this group.")]
        public float Interval { get; internal set; } = 0.8f;

        public SpawnGroup()
        {
        }

        public SpawnGroup(EnemyDefinition enemy, int count, float startDelay = 0f, float interval = 0.8f)
        {
            Enemy = enemy;
            Count = count;
            StartDelay = startDelay;
            Interval = interval;
        }
    }
}
