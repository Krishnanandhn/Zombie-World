using System.Collections.Generic;
using Project.Core;
using UnityEngine;

namespace Project.Gameplay
{
    public struct ProjectileSettings
    {
        public Team Team;
        public Vector3 Direction;
        public float Speed;
        public float Damage;
        public float Range;
        public int Pierce;
        public float Knockback;
        public bool IsCrit;
        public Color Tint;
        public float Scale;
    }

    // Swept-sphere bullet. Moves in Update and resolves hits itself, so no rigidbody is needed.
    public sealed class Projectile : MonoBehaviour, IPoolable
    {
        const float Radius = 0.12f;

        static readonly RaycastHit[] Hits = new RaycastHit[24];
        static readonly List<Projectile> ActiveProjectiles = new List<Projectile>(128);

        TrailRenderer _trail;
        MeshRenderer _renderer;
        IPoolService _pool;
        ProjectileSettings _s;
        float _travelled;
        int _pierceLeft;

        void Awake()
        {
            _trail = GetComponentInChildren<TrailRenderer>(true);
            _renderer = GetComponentInChildren<MeshRenderer>(true);
        }

        public void OnSpawn()
        {
            _pool = Services.Get<IPoolService>();
            ActiveProjectiles.Add(this);
            if (_trail != null) _trail.Clear();
        }

        public void OnDespawn()
        {
            ActiveProjectiles.Remove(this);
        }

        public static void DespawnAll(IPoolService pool)
        {
            for (int i = ActiveProjectiles.Count - 1; i >= 0; i--) pool.Despawn(ActiveProjectiles[i].gameObject);
            ActiveProjectiles.Clear();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetOnPlay() => ActiveProjectiles.Clear();

        public void Launch(in ProjectileSettings settings)
        {
            _s = settings;
            _s.Direction = settings.Direction.normalized;
            _pierceLeft = settings.Pierce;
            _travelled = 0f;

            transform.rotation = Quaternion.LookRotation(_s.Direction);
            transform.localScale = Vector3.one * Mathf.Max(0.1f, settings.Scale);

            if (_renderer != null) _renderer.sharedMaterial = MaterialLibrary.Emissive(settings.Tint, 4f);
            if (_trail != null)
            {
                _trail.startColor = settings.Tint;
                _trail.endColor = new Color(settings.Tint.r, settings.Tint.g, settings.Tint.b, 0f);
                _trail.Clear();
            }
        }

        void Update()
        {
            float step = _s.Speed * Time.deltaTime;
            if (step <= 0f) return;

            Vector3 pos = transform.position;
            Vector3 dir = _s.Direction;
            int count = Physics.SphereCastNonAlloc(pos, Radius, dir, Hits, step, ~0, QueryTriggerInteraction.Ignore);
            SortByDistance(count);

            for (int i = 0; i < count; i++)
            {
                RaycastHit hit = Hits[i];
                if (hit.collider.TryGetComponent(out IDamageable target))
                {
                    if (target.Team == _s.Team || !target.IsAlive) continue;

                    target.TakeDamage(new DamageInfo
                    {
                        Amount = _s.Damage,
                        Point = hit.point,
                        Direction = dir,
                        Knockback = _s.Knockback,
                        IsCrit = _s.IsCrit,
                        SourceTeam = _s.Team
                    });

                    if (_pierceLeft > 0)
                    {
                        _pierceLeft--;
                        continue;
                    }

                    _pool.Despawn(gameObject);
                    return;
                }

                EventBus<ProjectileImpact>.Publish(new ProjectileImpact { Position = hit.point, Normal = hit.normal, Tint = _s.Tint });
                _pool.Despawn(gameObject);
                return;
            }

            transform.position = pos + dir * step;
            _travelled += step;
            if (_travelled >= _s.Range) _pool.Despawn(gameObject);
        }

        static void SortByDistance(int count)
        {
            for (int i = 1; i < count; i++)
            {
                RaycastHit key = Hits[i];
                int j = i - 1;
                while (j >= 0 && Hits[j].distance > key.distance)
                {
                    Hits[j + 1] = Hits[j];
                    j--;
                }

                Hits[j + 1] = key;
            }
        }
    }
}
