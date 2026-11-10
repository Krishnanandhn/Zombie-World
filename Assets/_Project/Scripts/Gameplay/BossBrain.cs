using Project.Core;
using Project.Data;
using UnityEngine;

namespace Project.Gameplay
{
    // Abomination: chases with melee, charges down the player, slams the ground and summons minions as it is hurt.
    public sealed class BossBrain : IEnemyBrain, System.IDisposable
    {
        const float ChargeLineLength = 24f;
        const float ChargeHitDistance = 1.9f;
        const float MinChargeDistance = 6f;
        const float SummonRoarTime = 1.2f;
        const float WallMargin = 2.5f;

        static readonly IState<BossBrain> ChaseState = new Chase();
        static readonly IState<BossBrain> ChargeWindupState = new ChargeWindup();
        static readonly IState<BossBrain> ChargeState = new Charge();
        static readonly IState<BossBrain> SlamWindupState = new SlamWindup();
        static readonly IState<BossBrain> SummonState = new Summon();
        static readonly IState<BossBrain> RecoverState = new Recover();

        readonly MeleeBrain _melee = new MeleeBrain();
        readonly StateMachine<BossBrain> _machine;

        Enemy _enemy;
        BossSettings _settings;
        Telegraph _telegraph;
        ArenaInfo _arena;
        float _baseSpeedMultiplier;
        float _nextCharge;
        float _nextSlam;
        float _stateEnd;
        Vector3 _lockedDirection;
        bool _chargeHit;
        bool _enraged;
        int _summonIndex;

        public BossBrain()
        {
            _machine = new StateMachine<BossBrain>(this);
        }

        public void Begin(Enemy enemy)
        {
            _enemy = enemy;
            _settings = enemy.Definition.Boss;
            _arena = Services.Get<ArenaInfo>();
            _telegraph ??= new Telegraph();
            _baseSpeedMultiplier = enemy.SpeedMultiplier;
            _enraged = false;
            _summonIndex = 0;
            _nextCharge = Time.time + 4f;
            _nextSlam = Time.time + 3f;
            _melee.Begin(enemy);
            _machine.Set(null);
            _machine.Set(ChaseState);
        }

        public void Tick(Enemy enemy, float deltaTime)
        {
            if (!_enraged && enemy.Health.Fraction <= _settings.EnrageFraction)
            {
                _enraged = true;
                enemy.SpeedMultiplier = _baseSpeedMultiplier * _settings.EnrageSpeedMultiplier;
            }

            _machine.Tick(deltaTime);
        }

        public void End(Enemy enemy)
        {
            _telegraph?.Hide();
            _machine.Set(null);
        }

        public void Dispose()
        {
            _telegraph?.Destroy();
            _telegraph = null;
        }

        Vector3 DirectionToPlayer(out float distance)
        {
            Vector3 to = EnemySteering.Flat(_enemy.Player.Position - _enemy.Position);
            distance = to.magnitude;
            return distance > 0.001f ? to / distance : _enemy.transform.forward;
        }

        void Shake(float strength, float duration)
        {
            EventBus<CameraShakeRequested>.Publish(new CameraShakeRequested { Strength = strength, Duration = duration });
        }

        void Recovery(float seconds)
        {
            _stateEnd = Time.time + seconds;
            _machine.Set(RecoverState);
        }

        void SpawnMinions()
        {
            var spawner = Services.Get<IEnemySpawner>();
            if (spawner == null || _settings.Minion == null) return;

            int count = _settings.SummonCount;
            for (int i = 0; i < count; i++)
            {
                float angle = i * Mathf.PI * 2f / count;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * 4f;
                Vector3 pos = _enemy.Position + offset;
                if (_arena != null) pos = _arena.Clamp(pos, 1.5f);
                spawner.Spawn(_settings.Minion, pos, 1f, 1.1f);
            }
        }

        sealed class Chase : IState<BossBrain>
        {
            public void Enter(BossBrain b)
            {
            }

            public void Exit(BossBrain b)
            {
            }

            public void Tick(BossBrain b, float dt)
            {
                if (b._enemy.Player == null || !b._enemy.Player.IsAlive) return;

                BossSettings s = b._settings;
                if (s.Minion != null && b._summonIndex < s.SummonThresholds.Length
                    && b._enemy.Health.Fraction <= s.SummonThresholds[b._summonIndex])
                {
                    b._machine.Set(SummonState);
                    return;
                }

                b.DirectionToPlayer(out float dist);
                if (Time.time >= b._nextCharge && dist > MinChargeDistance)
                {
                    b._machine.Set(ChargeWindupState);
                    return;
                }

                if (Time.time >= b._nextSlam && dist < s.SlamRadius * 0.85f)
                {
                    b._machine.Set(SlamWindupState);
                    return;
                }

                b._melee.Tick(b._enemy, dt);
            }
        }

        sealed class ChargeWindup : IState<BossBrain>
        {
            public void Enter(BossBrain b)
            {
                b._stateEnd = Time.time + b._settings.ChargeWindup;
                b._lockedDirection = b.DirectionToPlayer(out _);
                b._enemy.Anim.Attack(b._settings.ChargeWindup);
                EventBus<BossCharging>.Publish(new BossCharging { Position = b._enemy.Position });
            }

            public void Exit(BossBrain b) => b._telegraph.Hide();

            public void Tick(BossBrain b, float dt)
            {
                // Track the player until just before the charge so the final line is a fair warning.
                if (Time.time < b._stateEnd - 0.3f) b._lockedDirection = b.DirectionToPlayer(out _);

                b._enemy.Face(b._lockedDirection);
                b._telegraph.ShowLine(b._enemy.Position, b._lockedDirection, ChargeLineLength, 2.4f);
                if (Time.time >= b._stateEnd) b._machine.Set(ChargeState);
            }
        }

        sealed class Charge : IState<BossBrain>
        {
            public void Enter(BossBrain b)
            {
                b._stateEnd = Time.time + b._settings.ChargeDuration;
                b._chargeHit = false;
            }

            public void Exit(BossBrain b)
            {
                b._nextCharge = Time.time + b._settings.ChargeCooldown;
            }

            public void Tick(BossBrain b, float dt)
            {
                Enemy e = b._enemy;
                float speedMult = b._enraged ? b._settings.EnrageSpeedMultiplier : 1f;
                e.Face(b._lockedDirection);
                e.SetVelocity(b._lockedDirection * (b._settings.ChargeSpeed * speedMult));

                IPlayerTarget player = e.Player;
                if (!b._chargeHit && player.IsAlive)
                {
                    float dist = EnemySteering.Flat(player.Position - e.Position).magnitude;
                    if (dist < ChargeHitDistance)
                    {
                        b._chargeHit = true;
                        player.Health.TakeDamage(new DamageInfo
                        {
                            Amount = b._settings.ChargeDamage,
                            Point = player.Position + Vector3.up,
                            Direction = b._lockedDirection,
                            Knockback = 16f,
                            SourceTeam = Team.Enemy
                        });
                        b.Shake(0.5f, 0.35f);
                    }
                }

                Vector3 p = e.Position;
                bool hitWall = b._arena != null && Mathf.Max(Mathf.Abs(p.x), Mathf.Abs(p.z)) > b._arena.HalfSize - WallMargin;
                if (hitWall || Time.time >= b._stateEnd)
                {
                    if (hitWall) b.Shake(0.6f, 0.4f);
                    b.Recovery(1.1f);
                }
            }
        }

        sealed class SlamWindup : IState<BossBrain>
        {
            public void Enter(BossBrain b)
            {
                b._stateEnd = Time.time + b._settings.SlamWindup;
                b._enemy.Anim.Attack(b._settings.SlamWindup);
            }

            public void Exit(BossBrain b)
            {
                b._telegraph.Hide();
                b._nextSlam = Time.time + b._settings.SlamCooldown;
            }

            public void Tick(BossBrain b, float dt)
            {
                Enemy e = b._enemy;
                b._telegraph.ShowDisc(e.Position, b._settings.SlamRadius);
                if (Time.time < b._stateEnd) return;

                IPlayerTarget player = e.Player;
                Vector3 to = EnemySteering.Flat(player.Position - e.Position);
                if (player.IsAlive && to.magnitude <= b._settings.SlamRadius)
                {
                    player.Health.TakeDamage(new DamageInfo
                    {
                        Amount = b._settings.SlamDamage,
                        Point = player.Position + Vector3.up,
                        Direction = to.normalized,
                        Knockback = 12f,
                        SourceTeam = Team.Enemy
                    });
                }

                EventBus<BossSlam>.Publish(new BossSlam { Position = e.Position, Radius = b._settings.SlamRadius });
                b.Shake(0.7f, 0.45f);
                b.Recovery(0.7f);
            }
        }

        sealed class Summon : IState<BossBrain>
        {
            public void Enter(BossBrain b)
            {
                b._stateEnd = Time.time + SummonRoarTime;
                b._enemy.Anim.Attack(SummonRoarTime);
                EventBus<BossRoar>.Publish(new BossRoar { Position = b._enemy.Position });
                b.Shake(0.35f, SummonRoarTime);
            }

            public void Exit(BossBrain b)
            {
            }

            public void Tick(BossBrain b, float dt)
            {
                if (Time.time < b._stateEnd) return;
                b._summonIndex++;
                b.SpawnMinions();
                b._machine.Set(ChaseState);
            }
        }

        sealed class Recover : IState<BossBrain>
        {
            public void Enter(BossBrain b)
            {
            }

            public void Exit(BossBrain b)
            {
            }

            public void Tick(BossBrain b, float dt)
            {
                if (Time.time >= b._stateEnd) b._machine.Set(ChaseState);
            }
        }
    }
}
