using System;
using UnityEngine;

namespace Project.Gameplay
{
    public sealed class Health : MonoBehaviour, IDamageable
    {
        float _invulnerableUntil;

        public event Action<Health> Changed;
        public event Action<Health, DamageInfo> Damaged;
        public event Action<Health> Died;

        public Team Team { get; private set; }
        public float Current { get; private set; }
        public float Max { get; private set; }
        public bool IsAlive => Current > 0f;
        public float Fraction => Max > 0f ? Current / Max : 0f;

        public void Init(Team team, float max)
        {
            Team = team;
            Max = max;
            Current = max;
            _invulnerableUntil = 0f;
            Changed?.Invoke(this);
        }

        // Raising max also heals by the same amount so upgrades feel immediate.
        public void SetMax(float max)
        {
            float delta = max - Max;
            Max = max;
            Current = Mathf.Clamp(Current + Mathf.Max(0f, delta), 0f, Max);
            Changed?.Invoke(this);
        }

        public void Heal(float amount)
        {
            if (!IsAlive || amount <= 0f) return;
            Current = Mathf.Min(Max, Current + amount);
            Changed?.Invoke(this);
        }

        public void GrantInvulnerability(float seconds)
        {
            _invulnerableUntil = Mathf.Max(_invulnerableUntil, Time.time + seconds);
        }

        public void TakeDamage(in DamageInfo info)
        {
            if (!IsAlive || Time.time < _invulnerableUntil) return;

            Current = Mathf.Max(0f, Current - info.Amount);
            Damaged?.Invoke(this, info);
            Changed?.Invoke(this);
            if (Current <= 0f) Died?.Invoke(this);
        }
    }
}
