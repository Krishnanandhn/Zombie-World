using Project.Core;
using Project.Data;
using UnityEngine;

namespace Project.Gameplay
{
    public interface IEnemyBrain
    {
        void Begin(Enemy enemy);
        void Tick(Enemy enemy, float deltaTime);
        void End(Enemy enemy);
    }

    [RequireComponent(typeof(Rigidbody), typeof(Health))]
    public sealed class Enemy : MonoBehaviour, IPoolable
    {
        const float SpawnInTime = 0.5f;
        const float TurnSpeed = 10f;
        const float KnockbackDecay = 9f;

        Rigidbody _body;
        Health _health;
        CapsuleCollider _collider;
        ProceduralAnimator _anim;
        Transform _facing;
        IEnemyBrain _brain;
        IPoolService _pool;
        EnemyRegistry _registry;
        Vector3 _velocity;
        Vector3 _knockback;
        Vector3 _faceDirection = Vector3.forward;
        float _spawnProgress = 1f;
        float _deathTimer;
        bool _dying;

        public EnemyDefinition Definition { get; private set; }
        public IPlayerTarget Player { get; private set; }
        public Health Health => _health;
        public ProceduralAnimator Anim => _anim;
        public float SpeedMultiplier { get; set; } = 1f;
        public Vector3 Position => transform.position;
        public bool IsBoss => Definition != null && Definition.Behaviour == EnemyBehaviour.Boss;
        public bool IsDying => _dying;

        void Awake()
        {
            _body = GetComponent<Rigidbody>();
            _health = GetComponent<Health>();
            _collider = GetComponent<CapsuleCollider>();
            _anim = GetComponent<ProceduralAnimator>();
            _facing = transform.Find("Facing");
            _health.Damaged += OnDamaged;
            _health.Died += OnDied;
            _health.Changed += OnHealthChanged;
        }

        void OnDestroy()
        {
            (_brain as System.IDisposable)?.Dispose();
        }

        public void OnSpawn()
        {
            if (_body == null) return;
            _body.linearVelocity = Vector3.zero;
        }

        public void OnDespawn()
        {
            _brain?.End(this);
            _registry?.Remove(this);
            if (_body != null) _body.linearVelocity = Vector3.zero;
        }

        public void Init(EnemyDefinition definition, float healthMultiplier, float speedMultiplier)
        {
            Definition = definition;
            SpeedMultiplier = speedMultiplier;
            Player = Services.Get<IPlayerTarget>();
            _pool = Services.Get<IPoolService>();
            _registry = Services.Get<EnemyRegistry>();

            _dying = false;
            _deathTimer = 0f;
            _velocity = Vector3.zero;
            _knockback = Vector3.zero;
            _collider.enabled = true;
            _spawnProgress = 0f;
            ApplySpawnVisual();

            _health.Init(Team.Enemy, definition.MaxHealth * healthMultiplier);
            _anim.ResetState();

            if (Player != null)
            {
                Vector3 toPlayer = Player.Position - Position;
                toPlayer.y = 0f;
                if (toPlayer.sqrMagnitude > 0.01f) _faceDirection = toPlayer.normalized;
            }

            _facing.rotation = Quaternion.LookRotation(_faceDirection);

            _brain ??= CreateBrain(definition.Behaviour);
            _brain.Begin(this);
            _registry?.Add(this);

            if (IsBoss)
            {
                EventBus<BossSpawned>.Publish(new BossSpawned { Name = definition.DisplayName, MaxHealth = _health.Max });
            }

            EventBus<EnemySpawned>.Publish(new EnemySpawned { Position = Position, IsBoss = IsBoss });
        }

        public void SetVelocity(Vector3 velocity) => _velocity = velocity;

        public void Face(Vector3 direction)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.0001f) _faceDirection = direction.normalized;
        }

        public void AddKnockback(Vector3 impulse) => _knockback += impulse;

        static IEnemyBrain CreateBrain(EnemyBehaviour behaviour)
        {
            switch (behaviour)
            {
                case EnemyBehaviour.Ranged: return new RangedBrain();
                case EnemyBehaviour.Boss: return new BossBrain();
                default: return new MeleeBrain();
            }
        }

        void FixedUpdate()
        {
            if (_dying || _spawnProgress < 1f || !_health.IsAlive)
            {
                _body.linearVelocity = Vector3.zero;
                return;
            }

            float dt = Time.fixedDeltaTime;
            _velocity = Vector3.zero;
            _brain.Tick(this, dt);
            _knockback = Vector3.Lerp(_knockback, Vector3.zero, 1f - Mathf.Exp(-KnockbackDecay * dt));

            Vector3 v = _velocity + _knockback;
            v.y = 0f;
            _body.linearVelocity = v;
        }

        void Update()
        {
            float dt = Time.deltaTime;

            if (_spawnProgress < 1f)
            {
                _spawnProgress = Mathf.Min(1f, _spawnProgress + dt / SpawnInTime);
                ApplySpawnVisual();
            }

            if (!_dying && _faceDirection.sqrMagnitude > 0.0001f)
            {
                Quaternion target = Quaternion.LookRotation(_faceDirection);
                _facing.rotation = Quaternion.Slerp(_facing.rotation, target, 1f - Mathf.Exp(-TurnSpeed * dt));
            }

            if (_dying)
            {
                _deathTimer += dt;
                if (_deathTimer >= (IsBoss ? 1.6f : 0.7f)) _pool.Despawn(gameObject);
            }
        }

        void ApplySpawnVisual()
        {
            float t = _spawnProgress;
            float eased = 1f - (1f - t) * (1f - t);
            _facing.localScale = Vector3.one * Mathf.Max(0.01f, eased);
            _facing.localPosition = new Vector3(0f, -(1f - eased) * 1.2f, 0f);
        }

        void OnDamaged(Health health, DamageInfo info)
        {
            if (_dying) return;

            Vector3 dir = info.Direction;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f)
            {
                AddKnockback(dir.normalized * info.Knockback * (1f - Definition.KnockbackResistance));
            }

            _anim.Punch(1f);
            EventBus<EnemyDamaged>.Publish(new EnemyDamaged
            {
                Position = info.Point,
                Amount = info.Amount,
                IsCrit = info.IsCrit,
                IsBoss = IsBoss,
                BloodColor = Definition.BloodColor
            });
        }

        void OnHealthChanged(Health health)
        {
            if (!IsBoss) return;
            EventBus<BossHealthChanged>.Publish(new BossHealthChanged { Current = health.Current, Max = health.Max });
        }

        void OnDied(Health health)
        {
            if (_dying) return;
            _dying = true;
            _deathTimer = 0f;
            _collider.enabled = false;
            _registry?.Remove(this);
            _anim.Fall();
            _brain?.End(this);

            Vector3 center = Position + Vector3.up;
            EventBus<EnemyKilled>.Publish(new EnemyKilled
            {
                Position = center,
                Score = Definition.ScoreValue,
                IsBoss = IsBoss,
                Scale = Definition.Scale,
                BloodColor = Definition.BloodColor
            });

            Services.Get<PickupManager>()?.TrySpawnHealth(Position, Definition.HealthDropChance);

            if (IsBoss) EventBus<BossDefeated>.Publish(new BossDefeated { Position = center });
        }
    }
}
