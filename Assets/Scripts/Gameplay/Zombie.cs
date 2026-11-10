using UnityEngine;

namespace ZombieWorld
{
    public sealed class Zombie : MonoBehaviour
    {
        public ZombieKind Kind;
        public ZombieStats Stats;
        public int Health;
        public Vector3 Pos;
        public Vector3 Knock;
        public float Yaw;
        public float Phase;
        public float AttackTimer;
        public float HitTimer;
        public float SpawnTime;

        Transform _tr;

        public Transform Tr => _tr;

        void Awake()
        {
            _tr = transform;
        }

        public bool IsTargetable(float now)
        {
            return now - SpawnTime >= GameConfig.ZombieRiseTime * 0.6f;
        }

        public void Activate(ZombieKind kind, Vector3 position, float now)
        {
            Kind = kind;
            Stats = GameConfig.Zombies[(int)kind];
            Health = Stats.Health;
            Pos = new Vector3(position.x, 0f, position.z);
            Knock = Vector3.zero;
            Yaw = Random.Range(0f, 360f);
            Phase = Random.Range(0f, 6.28f);
            AttackTimer = 0.4f;
            HitTimer = 0f;
            SpawnTime = now;
            _tr.SetPositionAndRotation(Pos, Quaternion.Euler(0f, Yaw, 0f));
            _tr.localScale = Vector3.zero;
        }
    }
}
