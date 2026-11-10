using System.Collections.Generic;
using UnityEngine;

namespace Project.Data
{
    // Root content asset. Everything tunable hangs off this so a new enemy/weapon/upgrade is just a new asset.
    [CreateAssetMenu(menuName = "Zombie Survival/Game Content", fileName = "GameContent")]
    public sealed class GameContent : ScriptableObject
    {
        public const string ResourcePath = "ZombieSurvival/GameContent";

        [field: SerializeField] public PlayerDefinition Player { get; internal set; }
        [field: SerializeField] public LevelDefinition Level { get; internal set; }
        [field: SerializeField] public List<WeaponDefinition> Weapons { get; internal set; } = new List<WeaponDefinition>();
        [field: SerializeField] public List<EnemyDefinition> Enemies { get; internal set; } = new List<EnemyDefinition>();
        [field: SerializeField] public List<UpgradeDefinition> Upgrades { get; internal set; } = new List<UpgradeDefinition>();

        [field: Header("Game feel")]
        [field: SerializeField] public float KillHitStop { get; internal set; } = 0.025f;
        [field: SerializeField] public float BossKillHitStop { get; internal set; } = 0.35f;
    }
}
