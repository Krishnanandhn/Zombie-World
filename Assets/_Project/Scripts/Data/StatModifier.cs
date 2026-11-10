using System;
using UnityEngine;

namespace Project.Data
{
    [Serializable]
    public struct StatModifier
    {
        public StatType Stat;
        public ModifierOp Op;
        public float Value;

        public StatModifier(StatType stat, ModifierOp op, float value)
        {
            Stat = stat;
            Op = op;
            Value = value;
        }
    }
}
