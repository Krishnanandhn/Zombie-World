using UnityEngine;

namespace ZombieWorld
{
    public sealed class PlayerController : MonoBehaviour
    {
        GameManager _gm;
        Transform _tr;
        float _fireTimer;
        float _hurtTimer;
        float _yaw;

        public int Health { get; private set; }
        public Vector3 Position => _tr.position;

        public static PlayerController Create(Transform parent)
        {
            var go = new GameObject("Player");
            go.transform.SetParent(parent, false);
            LowPolyAssets.AddRenderer(go, LowPolyAssets.PlayerMesh, true);
            return go.AddComponent<PlayerController>();
        }

        public void Init(GameManager gm)
        {
            _gm = gm;
            _tr = transform;
            ResetPlayer();
        }

        public void ResetPlayer()
        {
            Health = GameConfig.PlayerMaxHealth;
            _fireTimer = 0f;
            _hurtTimer = 0f;
            _yaw = 0f;
            _tr.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            _tr.localScale = Vector3.one;
        }

        public void Heal(int amount)
        {
            Health = Mathf.Min(GameConfig.PlayerMaxHealth, Health + amount);
        }

        public void TakeDamage(int amount)
        {
            if (_gm.State != GameState.Playing) return;

            Health -= amount;
            _hurtTimer = 0.2f;
            if (Health <= 0)
            {
                Health = 0;
                _gm.OnPlayerDied();
            }
        }

        public void Die()
        {
            Vector3 p = _tr.position;
            _tr.SetPositionAndRotation(new Vector3(p.x, 0.25f, p.z), Quaternion.Euler(80f, _yaw, 0f));
            _tr.localScale = Vector3.one;
        }

        void Update()
        {
            if (_gm.State != GameState.Playing) return;

            float dt = Time.deltaTime;
            Vector2 input = _gm.Controls.Move;
            Vector3 dir = new Vector3(input.x, 0f, input.y);

            Vector3 pos = _tr.position + dir * (GameConfig.PlayerSpeed * dt);
            float limit = GameConfig.PlayAreaHalfSize;
            pos.x = Mathf.Clamp(pos.x, -limit, limit);
            pos.z = Mathf.Clamp(pos.z, -limit, limit);
            pos.y = input.sqrMagnitude > 0f ? Mathf.Abs(Mathf.Sin(Time.time * 12f)) * 0.08f : 0f;

            Zombie target = _gm.Zombies.FindNearest(pos, GameConfig.FireRange);
            Vector3 face = target != null ? target.Pos - pos : dir;
            face.y = 0f;
            if (face.sqrMagnitude > 0.001f)
            {
                float targetYaw = Mathf.Atan2(face.x, face.z) * Mathf.Rad2Deg;
                _yaw = Mathf.MoveTowardsAngle(_yaw, targetYaw, 900f * dt);
            }

            _tr.SetPositionAndRotation(pos, Quaternion.Euler(0f, _yaw, 0f));

            if (_hurtTimer > 0f) _hurtTimer -= dt;
            float h = Mathf.Max(0f, _hurtTimer) * 0.6f;
            _tr.localScale = new Vector3(1f + h, 1f - h, 1f + h);

            _fireTimer -= dt;
            if (target != null && _fireTimer <= 0f)
            {
                Fire(pos, target.Pos);
                _fireTimer = GameConfig.FireInterval;
            }
        }

        void Fire(Vector3 pos, Vector3 targetPos)
        {
            Vector3 aim = targetPos - pos;
            aim.y = 0f;
            if (aim.sqrMagnitude < 0.0001f) return;

            aim = Quaternion.Euler(0f, Random.Range(-GameConfig.BulletSpreadDegrees, GameConfig.BulletSpreadDegrees), 0f) * aim.normalized;
            Vector3 muzzle = pos + Quaternion.Euler(0f, _yaw, 0f) * new Vector3(0.18f, 0.95f, 0.8f);
            _gm.Bullets.Spawn(muzzle, aim);
        }
    }
}
