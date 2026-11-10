using System;

namespace Project.Data
{
    // Pure stat math: (base + flat) * (1 + percent). No Unity dependencies so it is trivially testable.
    public sealed class StatSheet
    {
        static readonly int StatCount = Enum.GetValues(typeof(StatType)).Length;

        readonly float[] _base = new float[StatCount];
        readonly float[] _flat = new float[StatCount];
        readonly float[] _percent = new float[StatCount];

        public void SetBase(StatType stat, float value) => _base[(int)stat] = value;

        public void Add(StatModifier modifier)
        {
            int i = (int)modifier.Stat;
            if (modifier.Op == ModifierOp.Flat) _flat[i] += modifier.Value;
            else _percent[i] += modifier.Value;
        }

        public float Get(StatType stat)
        {
            int i = (int)stat;
            return Math.Max(0f, (_base[i] + _flat[i]) * (1f + _percent[i]));
        }

        public void ClearModifiers()
        {
            Array.Clear(_flat, 0, _flat.Length);
            Array.Clear(_percent, 0, _percent.Length);
        }
    }
}
