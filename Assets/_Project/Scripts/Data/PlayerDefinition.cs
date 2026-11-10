using UnityEngine;

namespace Project.Data
{
    [CreateAssetMenu(menuName = "Zombie Survival/Player", fileName = "Player")]
    public sealed class PlayerDefinition : ScriptableObject
    {
        [field: SerializeField] public float MaxHealth { get; internal set; } = 100f;
        [field: SerializeField] public float MoveSpeed { get; internal set; } = 6.5f;
        [field: SerializeField] public float PickupRadius { get; internal set; } = 3f;
        [field: SerializeField, Range(0f, 1f)] public float CritChance { get; internal set; } = 0.05f;
        [field: SerializeField] public float CritDamage { get; internal set; } = 2f;

        [field: Header("Dash")]
        [field: SerializeField] public float DashSpeed { get; internal set; } = 19f;
        [field: SerializeField] public float DashDuration { get; internal set; } = 0.17f;
        [field: SerializeField] public float DashCooldown { get; internal set; } = 1.4f;

        [field: Header("Damage taken")]
        [field: SerializeField] public float HitInvulnerability { get; internal set; } = 0.45f;

        [field: SerializeField] public WeaponDefinition StartingWeapon { get; internal set; }

        public void ApplyBase(StatSheet sheet)
        {
            sheet.SetBase(StatType.Damage, 1f);
            sheet.SetBase(StatType.FireRate, 1f);
            sheet.SetBase(StatType.ReloadSpeed, 1f);
            sheet.SetBase(StatType.MagazineSize, 1f);
            sheet.SetBase(StatType.MaxHealth, MaxHealth);
            sheet.SetBase(StatType.MoveSpeed, MoveSpeed);
            sheet.SetBase(StatType.CritChance, CritChance);
            sheet.SetBase(StatType.CritDamage, CritDamage);
            sheet.SetBase(StatType.Pierce, 0f);
            sheet.SetBase(StatType.ExtraProjectiles, 0f);
            sheet.SetBase(StatType.PickupRadius, PickupRadius);
            sheet.SetBase(StatType.DashCooldown, DashCooldown);
            sheet.SetBase(StatType.HealOnKill, 0f);
            sheet.SetBase(StatType.Regen, 0f);
        }
    }
}
