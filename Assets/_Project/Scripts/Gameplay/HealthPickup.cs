using System.Collections.Generic;
using Project.Core;
using UnityEngine;

namespace Project.Gameplay
{
    public sealed class HealthPickup : MonoBehaviour, IPoolable
    {
        const float Lifetime = 18f;
        const float MagnetSpeed = 16f;
        const float CollectDistance = 0.9f;
        const float HoverHeight = 0.7f;

        static readonly List<HealthPickup> ActivePickups = new List<HealthPickup>(16);

        Transform _visual;
        IPlayerTarget _player;
        IPoolService _pool;
        float _spawnTime;
        float _healAmount;

        void Awake()
        {
            _visual = transform.Find("Visual");
        }

        public void Setup(float healAmount) => _healAmount = healAmount;

        public void OnSpawn()
        {
            _player = Services.Get<IPlayerTarget>();
            _pool = Services.Get<IPoolService>();
            _spawnTime = Time.time;
            ActivePickups.Add(this);
        }

        public void OnDespawn()
        {
            ActivePickups.Remove(this);
        }

        public static void DespawnAll(IPoolService pool)
        {
            for (int i = ActivePickups.Count - 1; i >= 0; i--) pool.Despawn(ActivePickups[i].gameObject);
            ActivePickups.Clear();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetOnPlay() => ActivePickups.Clear();

        void Update()
        {
            float age = Time.time - _spawnTime;
            if (age >= Lifetime)
            {
                _pool.Despawn(gameObject);
                return;
            }

            _visual.localPosition = new Vector3(0f, HoverHeight + Mathf.Sin(Time.time * 3f) * 0.1f, 0f);
            _visual.Rotate(0f, 120f * Time.deltaTime, 0f, Space.World);

            if (_player == null || !_player.IsAlive || _player.Health.Current >= _player.Health.Max) return;

            Vector3 to = _player.Position - transform.position;
            to.y = 0f;
            float dist = to.magnitude;
            if (dist <= CollectDistance)
            {
                _player.Heal(_healAmount);
                EventBus<PickupCollected>.Publish(new PickupCollected { Position = transform.position, Amount = _healAmount });
                _pool.Despawn(gameObject);
                return;
            }

            if (dist <= _player.PickupRadius)
            {
                float speed = MagnetSpeed * (1f - dist / Mathf.Max(0.01f, _player.PickupRadius) * 0.5f);
                transform.position += to / dist * (speed * Time.deltaTime);
            }
        }
    }
}
