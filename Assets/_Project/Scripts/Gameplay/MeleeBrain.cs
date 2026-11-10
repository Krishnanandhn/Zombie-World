using UnityEngine;

namespace Project.Gameplay
{
    // Chase the player and hit them with a short wind-up.
    public sealed class MeleeBrain : IEnemyBrain
    {
        const float WindupTime = 0.3f;

        float _nextAttack;
        float _windupEnd;
        bool _winding;

        public void Begin(Enemy enemy)
        {
            _winding = false;
            _nextAttack = Time.time + Random.Range(0.3f, 0.9f);
        }

        public void End(Enemy enemy)
        {
            _winding = false;
        }

        public void Tick(Enemy enemy, float deltaTime)
        {
            IPlayerTarget player = enemy.Player;
            if (player == null || !player.IsAlive) return;

            Vector3 to = EnemySteering.Flat(player.Position - enemy.Position);
            float dist = to.magnitude;
            Vector3 dir = dist > 0.001f ? to / dist : Vector3.forward;
            float range = enemy.Definition.AttackRange;
            enemy.Face(dir);

            if (_winding)
            {
                if (Time.time < _windupEnd) return;
                _winding = false;
                _nextAttack = Time.time + enemy.Definition.AttackCooldown;
                if (dist <= range * 1.3f) Strike(enemy, player, dir);
                return;
            }

            if (dist <= range && Time.time >= _nextAttack)
            {
                _winding = true;
                _windupEnd = Time.time + WindupTime;
                enemy.Anim.Attack(WindupTime);
                return;
            }

            if (dist > range * 0.8f)
            {
                Vector3 steer = EnemySteering.AvoidObstacles(enemy.Position, dir, 1.4f);
                enemy.SetVelocity(steer * (enemy.Definition.MoveSpeed * enemy.SpeedMultiplier));
            }
        }

        static void Strike(Enemy enemy, IPlayerTarget player, Vector3 dir)
        {
            var info = new DamageInfo
            {
                Amount = enemy.Definition.AttackDamage,
                Point = player.Position + Vector3.up,
                Direction = dir,
                Knockback = 5f,
                SourceTeam = Team.Enemy
            };
            player.Health.TakeDamage(info);
        }
    }
}
