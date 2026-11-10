using System;
using System.Collections.Generic;

namespace Project.Data
{
    // Weighted pick without replacement, honouring availability (stack caps, already-owned weapons).
    public static class UpgradeRoller
    {
        public static int WeightOf(UpgradeRarity rarity)
        {
            switch (rarity)
            {
                case UpgradeRarity.Epic: return 10;
                case UpgradeRarity.Rare: return 30;
                default: return 60;
            }
        }

        public static void Roll(
            IReadOnlyList<UpgradeDefinition> pool,
            Func<UpgradeDefinition, bool> isAvailable,
            int count,
            Random rng,
            List<UpgradeDefinition> results)
        {
            results.Clear();
            var candidates = new List<UpgradeDefinition>(pool.Count);
            for (int i = 0; i < pool.Count; i++)
            {
                if (pool[i] != null && isAvailable(pool[i])) candidates.Add(pool[i]);
            }

            while (results.Count < count && candidates.Count > 0)
            {
                int total = 0;
                for (int i = 0; i < candidates.Count; i++) total += WeightOf(candidates[i].Rarity);

                int roll = rng.Next(total);
                for (int i = 0; i < candidates.Count; i++)
                {
                    roll -= WeightOf(candidates[i].Rarity);
                    if (roll >= 0) continue;
                    results.Add(candidates[i]);
                    candidates.RemoveAt(i);
                    break;
                }
            }
        }
    }
}
