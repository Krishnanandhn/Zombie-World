using UnityEngine;

namespace Project.Data
{
    [CreateAssetMenu(menuName = "Zombie Survival/Upgrade", fileName = "Upgrade")]
    public sealed class UpgradeDefinition : ScriptableObject
    {
        [field: SerializeField] public string Id { get; internal set; } = "upgrade";
        [field: SerializeField] public string DisplayName { get; internal set; } = "Upgrade";
        [field: SerializeField, TextArea] public string Description { get; internal set; } = "";
        [field: SerializeField] public UpgradeRarity Rarity { get; internal set; } = UpgradeRarity.Common;
        [field: SerializeField, Min(1)] public int MaxStacks { get; internal set; } = 3;
        [field: SerializeField] public StatModifier[] Modifiers { get; internal set; } = new StatModifier[0];
        [field: SerializeField] public WeaponDefinition UnlockWeapon { get; internal set; }
        [field: SerializeField, Range(0f, 1f), Tooltip("Instant heal as a fraction of max health.")]
        public float HealFraction { get; internal set; }
    }
}
