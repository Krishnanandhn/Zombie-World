using System;
using System.Collections.Generic;
using Project.Data;

namespace Project.Gameplay
{
    // Run-scoped player stats: base values from PlayerDefinition plus stacked upgrade modifiers.
    public sealed class PlayerStats
    {
        readonly StatSheet _sheet = new StatSheet();
        readonly PlayerDefinition _definition;
        readonly Dictionary<string, int> _stacks = new Dictionary<string, int>();

        public event Action Changed;

        public PlayerStats(PlayerDefinition definition)
        {
            _definition = definition;
            _definition.ApplyBase(_sheet);
        }

        public float Get(StatType stat) => _sheet.Get(stat);

        public void Reset()
        {
            _sheet.ClearModifiers();
            _stacks.Clear();
            _definition.ApplyBase(_sheet);
            Changed?.Invoke();
        }

        public int StacksOf(UpgradeDefinition upgrade)
        {
            return _stacks.TryGetValue(upgrade.Id, out int n) ? n : 0;
        }

        public bool CanTake(UpgradeDefinition upgrade) => StacksOf(upgrade) < upgrade.MaxStacks;

        public void Apply(UpgradeDefinition upgrade)
        {
            for (int i = 0; i < upgrade.Modifiers.Length; i++) _sheet.Add(upgrade.Modifiers[i]);
            _stacks[upgrade.Id] = StacksOf(upgrade) + 1;
            Changed?.Invoke();
        }
    }
}
