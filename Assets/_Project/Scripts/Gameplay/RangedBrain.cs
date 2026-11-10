using Project.Core;
using UnityEngine;

namespace Project.Gameplay
{
    // Keeps its distance and lobs acid at the player.
    public sealed class RangedBrain : IEnemyBrain
    {
        const float RangeTolerance = 2.5f;
        const float ProjectileRange = 40f;

        float _nextShot;
        float _strafeSign = 1f;
        float _nextStrafeFlip;

        public void Begin(Enemy enemy)
        {
            _nextShot = Time.time + Random.Range(0.8f, 1.8f);
            _strafeSign = Random.value < 0.5f ? -1f : 1f;
            _nextStrafeFlip = Time.time + Random.Range(1.5f, 3.5f);
        }

        public void End(Enemy enemy)
        {
        }

        public void Tick(Enemy enemy, float deltaTime)
        {
            IPlayerTarget player = enemy.Player;
            if (player == null || !player.IsAlive) return;

            Vector3 to = EnemySteering.Flat(player.Position - enemy.Position);
            float dist = to.magnitude;
            Vector3 dir = dist > 0.001f ? to / dist : Vector3.forward;
            enemy.Face(dir);

            float preferred = enemy.Definition.PreferredRange;
            float speed = enemy.Definition.MoveSpeed * enemy.SpeedMultiplier;
            Vector3 move;

            if (dist > preferred + RangeTolerance) move = dir;
            else if (dist < preferred - RangeTolerance) move = -dir;
            else
            {
                if (Time.time >= _nextStrafeFlip)
                {
                    _strafeSign = -_strafeSign;
                    _nextStrafeFlip = Time.time + Random.Range(1.5f, 3.5f);
                }

                move = Vector3.Cross(Vector3.up, dir) * (_strafeSign * 0.6f);
            }

            enemy.SetVelocity(EnemySteering.AvoidObstacles(enemy.Position, move, 1.4f) * speed);

            if (Time.time >= _nextShot && dist <= preferred + RangeTolerance * 2f)
            {
                _nextShot = Time.time + enemy.Definition.AttackCooldown;
                Fire(enemy, player);
            }
        }

        static void Fire(Enemy enemy, IPlayerTarget player)
        {
            var pool = Services.Get<IPoolService>();
            var assets = Services.Get<GameplayAssets>();
            if (pool == null || assets == null) return;

            enemy.Anim.Attack(0.25f);
            Vector3 origin = enemy.Position + Vector3.up * 1.2f;
            Vector3 target = player.Position + Vector3.up * 1.1f;
            Vector3 dir = (target - origin).normalized;

            GameObject go = pool.Spawn(assets.EnemyProjectile, origin, Quaternion.LookRotation(dir));
            if (go.TryGetComponent(out Projectile projectile))
            {
                projectile.Launch(new ProjectileSettings
                {
                    Team = Team.Enemy,
                    Direction = dir,
                    Speed = enemy.Definition.ProjectileSpeed,
                    Damage = enemy.Definition.AttackDamage,
                    Range = ProjectileRange,
                    Knockback = 3f,
                    Tint = enemy.Definition.ProjectileColor,
                    Scale = 1.4f
                });
            }

            EventBus<EnemyProjectileFired>.Publish(new EnemyProjectileFired { Position = origin });
        }
    }
}
