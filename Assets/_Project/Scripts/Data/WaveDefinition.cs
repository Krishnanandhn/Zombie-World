using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Data
{
    [Serializable]
    public sealed class WaveDefinition
    {
        [field: SerializeField] public string Title { get; internal set; } = "";
        [field: SerializeField] public bool IsBossWave { get; internal set; }
        [field: SerializeField] public List<SpawnGroup> Groups { get; internal set; } = new List<SpawnGroup>();

        public int TotalEnemies
        {
            get
            {
                int total = 0;
                for (int i = 0; i < Groups.Count; i++) total += Groups[i].Count;
                return total;
            }
        }
    }
}
