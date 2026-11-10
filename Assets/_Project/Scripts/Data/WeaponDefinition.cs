using Project.Core;
using UnityEngine;

namespace Project.Data
{
    [CreateAssetMenu(menuName = "Zombie Survival/Weapon", fileName = "Weapon")]
    public sealed class WeaponDefinition : ScriptableObject
    {
        [field: SerializeField] public string Id { get; internal set; } = "weapon";
        [field: SerializeField] public string DisplayName { get; internal set; } = "Weapon";
        [field: SerializeField, Min(0.1f)] public float Damage { get; internal set; } = 20f;
        [field: SerializeField, Tooltip("Shots per second.")] public float FireRate { get; internal set; } = 4f;
        [field: SerializeField, Min(1)] public int MagazineSize { get; internal set; } = 12;
        [field: SerializeField, Min(0.1f)] public float ReloadTime { get; internal set; } = 1.2f;
        [field: SerializeField, Min(1)] public int Pellets { get; internal set; } = 1;
        [field: SerializeField, Tooltip("Total cone angle in degrees.")] public float SpreadDegrees { get; internal set; } = 2f;
        [field: SerializeField] public float ProjectileSpeed { get; internal set; } = 40f;
        [field: SerializeField] public float Range { get; internal set; } = 28f;
        [field: SerializeField, Min(0)] public int Pierce { get; internal set; }
        [field: SerializeField] public float Knockback { get; internal set; } = 3f;
        [field: SerializeField] public float ProjectileScale { get; internal set; } = 1f;
        [field: SerializeField] public Color Tint { get; internal set; } = new Color(1f, 0.85f, 0.35f);
        [field: SerializeField] public SfxId Sfx { get; internal set; } = SfxId.Pistol;
        [field: SerializeField] public float ShakeStrength { get; internal set; } = 0.06f;
    }
}
