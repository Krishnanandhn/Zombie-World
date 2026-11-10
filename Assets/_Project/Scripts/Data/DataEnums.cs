namespace Project.Data
{
    public enum StatType
    {
        Damage,
        FireRate,
        ReloadSpeed,
        MagazineSize,
        MaxHealth,
        MoveSpeed,
        CritChance,
        CritDamage,
        Pierce,
        ExtraProjectiles,
        PickupRadius,
        DashCooldown,
        HealOnKill,
        Regen
    }

    public enum ModifierOp
    {
        // Added to the base value.
        Flat,
        // Added to a shared multiplier: 0.15 means +15%.
        Percent
    }

    public enum UpgradeRarity
    {
        Common,
        Rare,
        Epic
    }

    public enum EnemyBehaviour
    {
        Melee,
        Ranged,
        Boss
    }
}
