using UnityEngine;

namespace Project.Data
{
    [CreateAssetMenu(menuName = "Zombie Survival/Enemy", fileName = "Enemy")]
    public sealed class EnemyDefinition : ScriptableObject
    {
        [field: Header("Identity")]
        [field: SerializeField] public string Id { get; internal set; } = "enemy";
        [field: SerializeField] public string DisplayName { get; internal set; } = "Enemy";
        [field: SerializeField] public EnemyBehaviour Behaviour { get; internal set; } = EnemyBehaviour.Melee;

        [field: Header("Combat")]
        [field: SerializeField, Min(1f)] public float MaxHealth { get; internal set; } = 40f;
        [field: SerializeField] public float MoveSpeed { get; internal set; } = 2.2f;
        [field: SerializeField] public float AttackDamage { get; internal set; } = 10f;
        [field: SerializeField] public float AttackRange { get; internal set; } = 1.5f;
        [field: SerializeField] public float AttackCooldown { get; internal set; } = 1f;
        [field: SerializeField, Range(0f, 1f)] public float KnockbackResistance { get; internal set; }

        [field: Header("Ranged")]
        [field: SerializeField] public float PreferredRange { get; internal set; } = 10f;
        [field: SerializeField] public float ProjectileSpeed { get; internal set; } = 11f;
        [field: SerializeField] public Color ProjectileColor { get; internal set; } = new Color(0.55f, 1f, 0.2f);

        [field: Header("Rewards")]
        [field: SerializeField] public int ScoreValue { get; internal set; } = 10;
        [field: SerializeField, Range(0f, 1f)] public float HealthDropChance { get; internal set; } = 0.05f;

        [field: Header("Look")]
        [field: SerializeField] public float Scale { get; internal set; } = 1f;
        [field: SerializeField] public Color BodyColor { get; internal set; } = new Color(0.35f, 0.45f, 0.3f);
        [field: SerializeField] public Color SkinColor { get; internal set; } = new Color(0.55f, 0.65f, 0.45f);
        [field: SerializeField] public Color EyeColor { get; internal set; } = new Color(1f, 0.2f, 0.1f);
        [field: SerializeField] public Color BloodColor { get; internal set; } = new Color(0.5f, 0.02f, 0.02f);

        [field: Header("Boss (only used when Behaviour is Boss)")]
        [field: SerializeField] public BossSettings Boss { get; internal set; } = new BossSettings();
    }
}
