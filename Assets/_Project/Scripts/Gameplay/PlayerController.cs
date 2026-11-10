using Project.Core;
using Project.Data;
using UnityEngine;

namespace Project.Gameplay
{
    [RequireComponent(typeof(Rigidbody), typeof(Health))]
    public sealed class PlayerController : MonoBehaviour, IPlayerTarget
    {
        const float TurnSpeed = 18f;
        const float KnockbackDecay = 10f;
        const float RegenTick = 1f;
        const float HitShake = 0.35f;

        Rigidbody _body;
        Health _health;
        ProceduralAnimator _anim;
        Transform _facing;
        PlayerDefinition _definition;
        IInputService _input;
        Camera _camera;
        Plane _aimPlane;

        Vector3 _moveInput;
        Vector3 _aimDirection = Vector3.forward;
        Vector3 _knockback;
        Vector3 _dashDirection;
        float _dashEnd;
        float _dashReadyAt;
        float _regenBank;
        bool _dashing;

        public PlayerStats Stats { get; private set; }
        public WeaponController Weapons { get; private set; }
        public bool Active { get; set; }
        public Vector3 AimDirection => _aimDirection;
        public float DashCooldownRemaining => Mathf.Max(0f, _dashReadyAt - Time.time);

        public Transform Transform => transform;
        public Vector3 Position => transform.position;
        public bool IsAlive => _health.IsAlive;
        public Health Health => _health;
        public float PickupRadius => Stats.Get(StatType.PickupRadius);

        public void Heal(float amount) => _health.Heal(amount);

        public static PlayerController Create(PlayerDefinition definition, IInputService input, IPoolService pool, GameplayAssets assets, Camera camera)
        {
            var go = new GameObject("Player");
            go.SetActive(false);

            var body = go.AddComponent<Rigidbody>();
            body.useGravity = false;
            body.mass = 6f;
            body.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.linearDamping = 0f;

            var capsule = go.AddComponent<CapsuleCollider>();
            capsule.radius = 0.38f;
            capsule.height = 1.85f;
            capsule.center = new Vector3(0f, 0.93f, 0f);

            go.AddComponent<Health>();
            ModelFactory.BuildHumanoid(go.transform, new HumanoidStyle
            {
                Kind = HumanoidKind.Player,
                Body = new Color(0.22f, 0.3f, 0.24f),
                Skin = new Color(0.85f, 0.68f, 0.55f),
                Pants = new Color(0.16f, 0.17f, 0.2f),
                Eye = new Color(0.1f, 0.1f, 0.1f),
                Scale = 1f,
                Bulk = 1f
            });
            go.AddComponent<ProceduralAnimator>().Configure(true, 1f);
            go.AddComponent<WeaponController>();
            var player = go.AddComponent<PlayerController>();
            go.SetActive(true);

            player.Init(definition, input, pool, assets, camera);
            return player;
        }

        void Awake()
        {
            _body = GetComponent<Rigidbody>();
            _health = GetComponent<Health>();
            _anim = GetComponent<ProceduralAnimator>();
            Weapons = GetComponent<WeaponController>();
            _facing = transform.Find("Facing");
            _health.Damaged += OnDamaged;
            _health.Changed += OnHealthChanged;
            _health.Died += OnDied;
        }

        void OnEnable() => EventBus<EnemyKilled>.Subscribe(OnEnemyKilled);

        void OnDisable() => EventBus<EnemyKilled>.Unsubscribe(OnEnemyKilled);

        void Init(PlayerDefinition definition, IInputService input, IPoolService pool, GameplayAssets assets, Camera camera)
        {
            _definition = definition;
            _input = input;
            _camera = camera;
            _aimPlane = new Plane(Vector3.up, new Vector3(0f, ModelFactory.PlayerMuzzleHeight, 0f));

            Stats = new PlayerStats(definition);
            Stats.Changed += OnStatsChanged;
            Weapons.Init(Stats, input, pool, assets, _anim.Muzzle, () => _aimDirection);
        }

        public void ResetForRun(Vector3 position)
        {
            Stats.Reset();
            _health.Init(Team.Player, Stats.Get(StatType.MaxHealth));
            Weapons.ResetLoadout(_definition.StartingWeapon);

            transform.position = position;
            _body.position = position;
            _body.linearVelocity = Vector3.zero;
            _knockback = Vector3.zero;
            _dashing = false;
            _dashReadyAt = 0f;
            _regenBank = 0f;
            _aimDirection = Vector3.forward;
            _facing.rotation = Quaternion.identity;
            _anim.ResetState();
        }

        public void ApplyUpgrade(UpgradeDefinition upgrade)
        {
            Stats.Apply(upgrade);
            if (upgrade.UnlockWeapon != null) Weapons.Unlock(upgrade.UnlockWeapon);
            if (upgrade.HealFraction > 0f) _health.Heal(_health.Max * upgrade.HealFraction);
        }

        void Update()
        {
            Weapons.Enabled = Active && _health.IsAlive;
            if (!Active || !_health.IsAlive)
            {
                _moveInput = Vector3.zero;
                return;
            }

            Vector2 move = _input.Move;
            _moveInput = new Vector3(move.x, 0f, move.y);

            UpdateAim();
            RotateToAim();

            if (_input.DashPressed && !_dashing && Time.time >= _dashReadyAt) StartDash();

            float regen = Stats.Get(StatType.Regen);
            if (regen > 0f)
            {
                _regenBank += regen * Time.deltaTime;
                if (_regenBank >= RegenTick)
                {
                    _health.Heal(_regenBank);
                    _regenBank = 0f;
                }
            }
        }

        void FixedUpdate()
        {
            Vector3 velocity = _moveInput * Stats.Get(StatType.MoveSpeed);
            if (_dashing)
            {
                velocity = _dashDirection * _definition.DashSpeed;
                if (Time.time >= _dashEnd) _dashing = false;
            }

            _knockback = Vector3.Lerp(_knockback, Vector3.zero, 1f - Mathf.Exp(-KnockbackDecay * Time.fixedDeltaTime));
            _body.linearVelocity = _health.IsAlive ? velocity + _knockback : _knockback;
        }

        void UpdateAim()
        {
            if (_input.UsingGamepad)
            {
                Vector2 stick = _input.AimStick;
                if (stick.sqrMagnitude > 0.09f) _aimDirection = new Vector3(stick.x, 0f, stick.y).normalized;
                else if (_moveInput.sqrMagnitude > 0.25f) _aimDirection = _moveInput.normalized;
                return;
            }

            if (_camera == null) return;
            Ray ray = _camera.ScreenPointToRay(_input.PointerPosition);
            if (!_aimPlane.Raycast(ray, out float t)) return;

            Vector3 to = ray.GetPoint(t) - transform.position;
            to.y = 0f;
            if (to.sqrMagnitude > 0.04f) _aimDirection = to.normalized;
        }

        void RotateToAim()
        {
            Quaternion target = Quaternion.LookRotation(_aimDirection);
            _facing.rotation = Quaternion.Slerp(_facing.rotation, target, 1f - Mathf.Exp(-TurnSpeed * Time.deltaTime));
        }

        void StartDash()
        {
            _dashDirection = _moveInput.sqrMagnitude > 0.04f ? _moveInput.normalized : _aimDirection;
            _dashing = true;
            _dashEnd = Time.time + _definition.DashDuration;
            _dashReadyAt = Time.time + Stats.Get(StatType.DashCooldown);
            _health.GrantInvulnerability(_definition.DashDuration + 0.08f);
            EventBus<PlayerDashed>.Publish(new PlayerDashed { Position = transform.position, Cooldown = Stats.Get(StatType.DashCooldown) });
        }

        void OnDamaged(Health health, DamageInfo info)
        {
            Vector3 dir = info.Direction;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f) _knockback += dir.normalized * info.Knockback;

            health.GrantInvulnerability(_definition.HitInvulnerability);
            _anim.Punch(1f);
            EventBus<PlayerDamaged>.Publish(new PlayerDamaged { Position = transform.position + Vector3.up, Amount = info.Amount });
            EventBus<CameraShakeRequested>.Publish(new CameraShakeRequested { Strength = HitShake, Duration = 0.25f });
        }

        void OnHealthChanged(Health health)
        {
            EventBus<PlayerHealthChanged>.Publish(new PlayerHealthChanged { Current = health.Current, Max = health.Max });
        }

        void OnDied(Health health)
        {
            Active = false;
            _dashing = false;
            _anim.Fall();
            EventBus<PlayerDied>.Publish(new PlayerDied { Position = transform.position });
        }

        void OnStatsChanged()
        {
            float max = Stats.Get(StatType.MaxHealth);
            if (_health != null && _health.Max > 0f && !Mathf.Approximately(max, _health.Max)) _health.SetMax(max);
        }

        void OnEnemyKilled(EnemyKilled e)
        {
            float heal = Stats.Get(StatType.HealOnKill);
            if (heal > 0f && _health.IsAlive) _health.Heal(heal);
        }
    }
}
