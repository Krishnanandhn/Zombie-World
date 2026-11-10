using System.Collections.Generic;
using Project.Core;
using UnityEngine;

namespace Project.Data
{
    // Built-in balance. The editor writes these out as assets; at runtime this is the fallback when none exist.
    public static class DefaultContent
    {
        public static GameContent Create()
        {
            var content = ScriptableObject.CreateInstance<GameContent>();
            content.name = "GameContent";

            WeaponDefinition pistol = Weapon("pistol", "M9 Sidearm", 24f, 4f, 12, 1.1f, 1, 1.5f, 42f, 30f, 0, 3f, SfxId.Pistol, 0.05f, new Color(1f, 0.85f, 0.4f));
            WeaponDefinition smg = Weapon("smg", "Vector SMG", 11f, 13f, 32, 1.5f, 1, 4.5f, 40f, 26f, 0, 1.5f, SfxId.Smg, 0.04f, new Color(1f, 0.75f, 0.25f));
            WeaponDefinition shotgun = Weapon("shotgun", "Pump Shotgun", 13f, 1.25f, 6, 2f, 8, 16f, 36f, 15f, 0, 6f, SfxId.Shotgun, 0.2f, new Color(1f, 0.55f, 0.2f));
            WeaponDefinition rifle = Weapon("rifle", "Marksman Rifle", 75f, 1.8f, 8, 1.8f, 1, 0.4f, 75f, 50f, 2, 8f, SfxId.Rifle, 0.14f, new Color(0.45f, 0.9f, 1f));
            rifle.ProjectileScale = 1.4f;
            content.Weapons.AddRange(new[] { pistol, smg, shotgun, rifle });

            var player = ScriptableObject.CreateInstance<PlayerDefinition>();
            player.name = "Player";
            player.StartingWeapon = pistol;
            content.Player = player;

            EnemyDefinition walker = Enemy("walker", "Walker", EnemyBehaviour.Melee, 40f, 2.2f, 10f, 1.5f, 1f, 10, 0.06f, 1f,
                new Color(0.28f, 0.36f, 0.26f), new Color(0.52f, 0.64f, 0.45f));
            EnemyDefinition runner = Enemy("runner", "Runner", EnemyBehaviour.Melee, 22f, 4.6f, 7f, 1.3f, 0.8f, 15, 0.05f, 0.9f,
                new Color(0.5f, 0.28f, 0.22f), new Color(0.7f, 0.5f, 0.4f));
            EnemyDefinition brute = Enemy("brute", "Brute", EnemyBehaviour.Melee, 240f, 1.7f, 26f, 2.1f, 1.4f, 40, 0.3f, 1.5f,
                new Color(0.32f, 0.24f, 0.4f), new Color(0.5f, 0.42f, 0.55f));
            brute.KnockbackResistance = 0.8f;
            EnemyDefinition spitter = Enemy("spitter", "Spitter", EnemyBehaviour.Ranged, 32f, 2.1f, 12f, 12f, 2.2f, 25, 0.12f, 1.05f,
                new Color(0.3f, 0.45f, 0.12f), new Color(0.6f, 0.78f, 0.25f));
            spitter.PreferredRange = 11f;
            spitter.ProjectileSpeed = 12f;
            spitter.BloodColor = new Color(0.4f, 0.7f, 0.1f);

            EnemyDefinition boss = Enemy("abomination", "The Abomination", EnemyBehaviour.Boss, 2600f, 2.5f, 32f, 3.4f, 1.6f, 1000, 1f, 2.7f,
                new Color(0.35f, 0.08f, 0.1f), new Color(0.6f, 0.35f, 0.3f));
            boss.KnockbackResistance = 1f;
            boss.BloodColor = new Color(0.6f, 0.05f, 0.05f);
            boss.Boss.Minion = runner;
            content.Enemies.AddRange(new[] { walker, runner, brute, spitter, boss });

            LevelDefinition level = ScriptableObject.CreateInstance<LevelDefinition>();
            level.name = "Level_DeadYard";
            level.Waves = new List<WaveDefinition>
            {
                Wave("", false, new SpawnGroup(walker, 8, 0f, 0.9f)),
                Wave("", false, new SpawnGroup(walker, 10, 0f, 0.8f), new SpawnGroup(runner, 4, 4f, 1f)),
                Wave("", false, new SpawnGroup(walker, 10, 0f, 0.7f), new SpawnGroup(runner, 8, 3f, 0.8f), new SpawnGroup(spitter, 2, 6f, 3f)),
                Wave("", false, new SpawnGroup(walker, 12, 0f, 0.7f), new SpawnGroup(brute, 2, 5f, 6f), new SpawnGroup(runner, 6, 8f, 0.8f)),
                Wave("", false, new SpawnGroup(runner, 14, 0f, 0.6f), new SpawnGroup(spitter, 5, 2f, 2.2f), new SpawnGroup(brute, 2, 8f, 5f)),
                Wave("", false, new SpawnGroup(walker, 16, 0f, 0.55f), new SpawnGroup(brute, 4, 4f, 4f), new SpawnGroup(spitter, 4, 6f, 2.5f)),
                Wave("", false, new SpawnGroup(walker, 14, 0f, 0.5f), new SpawnGroup(runner, 12, 2f, 0.6f), new SpawnGroup(brute, 5, 5f, 3.5f), new SpawnGroup(spitter, 6, 6f, 2f)),
                Wave("THE ABOMINATION", true, new SpawnGroup(boss, 1, 1.5f, 1f), new SpawnGroup(walker, 10, 6f, 1.4f))
            };
            content.Level = level;

            content.Upgrades.AddRange(new[]
            {
                Upgrade("hollow_points", "Hollow Points", "+15% weapon damage.", UpgradeRarity.Common, 5, Mod(StatType.Damage, ModifierOp.Percent, 0.15f)),
                Upgrade("hair_trigger", "Hair Trigger", "+12% fire rate.", UpgradeRarity.Common, 5, Mod(StatType.FireRate, ModifierOp.Percent, 0.12f)),
                Upgrade("extended_mag", "Extended Mags", "+25% magazine size.", UpgradeRarity.Common, 3, Mod(StatType.MagazineSize, ModifierOp.Percent, 0.25f)),
                Upgrade("speed_loader", "Speed Loader", "+25% reload speed.", UpgradeRarity.Common, 3, Mod(StatType.ReloadSpeed, ModifierOp.Percent, 0.25f)),
                Upgrade("running_shoes", "Running Shoes", "+10% movement speed.", UpgradeRarity.Common, 3, Mod(StatType.MoveSpeed, ModifierOp.Percent, 0.1f)),
                Upgrade("kevlar", "Kevlar Vest", "+25 max health and heal 25.", UpgradeRarity.Common, 4, Mod(StatType.MaxHealth, ModifierOp.Flat, 25f)),
                Upgrade("magnet", "Scavenger", "+2 pickup radius.", UpgradeRarity.Common, 3, Mod(StatType.PickupRadius, ModifierOp.Flat, 2f)),
                Upgrade("sharpshooter", "Sharpshooter", "+10% critical chance.", UpgradeRarity.Rare, 4, Mod(StatType.CritChance, ModifierOp.Flat, 0.1f)),
                Upgrade("brutal_crits", "Brutal Crits", "+50% critical damage.", UpgradeRarity.Rare, 3, Mod(StatType.CritDamage, ModifierOp.Flat, 0.5f)),
                Upgrade("piercing", "Piercing Rounds", "Bullets pass through +1 enemy.", UpgradeRarity.Rare, 3, Mod(StatType.Pierce, ModifierOp.Flat, 1f)),
                Upgrade("adrenaline", "Adrenaline", "Dash cooldown -20%.", UpgradeRarity.Rare, 3, Mod(StatType.DashCooldown, ModifierOp.Percent, -0.2f)),
                Upgrade("vampiric", "Vampiric Rounds", "Heal 2 HP per kill.", UpgradeRarity.Rare, 3, Mod(StatType.HealOnKill, ModifierOp.Flat, 2f)),
                Upgrade("field_medic", "Field Medic", "Regenerate 1.5 HP per second.", UpgradeRarity.Rare, 3, Mod(StatType.Regen, ModifierOp.Flat, 1.5f)),
                Upgrade("medkit", "Medkit", "Instantly heal 50% of max health.", UpgradeRarity.Rare, 99, null, null, 0.5f),
                Upgrade("double_tap", "Double Tap", "+1 projectile per shot.", UpgradeRarity.Epic, 2, Mod(StatType.ExtraProjectiles, ModifierOp.Flat, 1f)),
                Upgrade("titan_heart", "Titan Heart", "+50% max health and +10% damage.", UpgradeRarity.Epic, 2,
                    Mod(StatType.MaxHealth, ModifierOp.Percent, 0.5f, StatType.Damage, ModifierOp.Percent, 0.1f)),
                Upgrade("unlock_smg", "Vector SMG", "Unlock a rapid-fire submachine gun.", UpgradeRarity.Epic, 1, null, smg),
                Upgrade("unlock_shotgun", "Pump Shotgun", "Unlock a devastating close-range shotgun.", UpgradeRarity.Epic, 1, null, shotgun),
                Upgrade("unlock_rifle", "Marksman Rifle", "Unlock a piercing high-damage rifle.", UpgradeRarity.Epic, 1, null, rifle)
            });

            return content;
        }

        static StatModifier[] Mod(StatType stat, ModifierOp op, float value) => new[] { new StatModifier(stat, op, value) };

        static StatModifier[] Mod(StatType s1, ModifierOp o1, float v1, StatType s2, ModifierOp o2, float v2)
            => new[] { new StatModifier(s1, o1, v1), new StatModifier(s2, o2, v2) };

        static WaveDefinition Wave(string title, bool boss, params SpawnGroup[] groups)
        {
            return new WaveDefinition { Title = title, IsBossWave = boss, Groups = new List<SpawnGroup>(groups) };
        }

        static UpgradeDefinition Upgrade(string id, string name, string description, UpgradeRarity rarity, int maxStacks,
            StatModifier[] modifiers, WeaponDefinition unlock = null, float heal = 0f)
        {
            var u = ScriptableObject.CreateInstance<UpgradeDefinition>();
            u.name = "Upgrade_" + id;
            u.Id = id;
            u.DisplayName = name;
            u.Description = description;
            u.Rarity = rarity;
            u.MaxStacks = maxStacks;
            u.Modifiers = modifiers ?? new StatModifier[0];
            u.UnlockWeapon = unlock;
            u.HealFraction = heal;
            return u;
        }

        static WeaponDefinition Weapon(string id, string name, float damage, float rate, int mag, float reload, int pellets,
            float spread, float speed, float range, int pierce, float knockback, SfxId sfx, float shake, Color tint)
        {
            var w = ScriptableObject.CreateInstance<WeaponDefinition>();
            w.name = "Weapon_" + id;
            w.Id = id;
            w.DisplayName = name;
            w.Damage = damage;
            w.FireRate = rate;
            w.MagazineSize = mag;
            w.ReloadTime = reload;
            w.Pellets = pellets;
            w.SpreadDegrees = spread;
            w.ProjectileSpeed = speed;
            w.Range = range;
            w.Pierce = pierce;
            w.Knockback = knockback;
            w.Sfx = sfx;
            w.ShakeStrength = shake;
            w.Tint = tint;
            return w;
        }

        static EnemyDefinition Enemy(string id, string name, EnemyBehaviour behaviour, float health, float speed, float damage,
            float range, float cooldown, int score, float dropChance, float scale, Color body, Color skin)
        {
            var e = ScriptableObject.CreateInstance<EnemyDefinition>();
            e.name = "Enemy_" + id;
            e.Id = id;
            e.DisplayName = name;
            e.Behaviour = behaviour;
            e.MaxHealth = health;
            e.MoveSpeed = speed;
            e.AttackDamage = damage;
            e.AttackRange = range;
            e.AttackCooldown = cooldown;
            e.ScoreValue = score;
            e.HealthDropChance = dropChance;
            e.Scale = scale;
            e.BodyColor = body;
            e.SkinColor = skin;
            return e;
        }
    }
}
