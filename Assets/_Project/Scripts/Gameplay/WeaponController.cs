using System;
using System.Collections.Generic;
using Project.Core;
using Project.Data;
using UnityEngine;

namespace Project.Gameplay
{
    public sealed class WeaponController : MonoBehaviour
    {
        const float MinMultiProjectileSpread = 8f;

        sealed class Slot
        {
            public WeaponDefinition Definition;
            public int Ammo;
        }

        readonly List<Slot> _slots = new List<Slot>(4);

        PlayerStats _stats;
        IInputService _input;
        IPoolService _pool;
        GameplayAssets _assets;
        Transform _muzzle;
        Func<Vector3> _aimDirection;
        int _current;
        float _nextFire;
        float _reloadEnd;
        bool _reloading;

        public bool Enabled { get; set; }
        public WeaponDefinition Current => _slots.Count > 0 ? _slots[_current].Definition : null;

        public void Init(PlayerStats stats, IInputService input, IPoolService pool, GameplayAssets assets, Transform muzzle, Func<Vector3> aimDirection)
        {
            _stats = stats;
            _input = input;
            _pool = pool;
            _assets = assets;
            _muzzle = muzzle;
            _aimDirection = aimDirection;
        }

        public void ResetLoadout(WeaponDefinition starting)
        {
            _slots.Clear();
            _slots.Add(new Slot { Definition = starting, Ammo = MagazineOf(starting) });
            _current = 0;
            _reloading = false;
            _nextFire = 0f;
            PublishAmmo();
        }

        public bool Owns(WeaponDefinition definition)
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].Definition == definition) return true;
            }

            return false;
        }

        public void Unlock(WeaponDefinition definition)
        {
            if (definition == null || Owns(definition)) return;
            _slots.Add(new Slot { Definition = definition, Ammo = MagazineOf(definition) });
            SwitchTo(_slots.Count - 1);
        }

        public void RefillAll()
        {
            for (int i = 0; i < _slots.Count; i++) _slots[i].Ammo = MagazineOf(_slots[i].Definition);
            CancelReload();
            PublishAmmo();
        }

        int MagazineOf(WeaponDefinition definition)
        {
            return Mathf.Max(1, Mathf.RoundToInt(definition.MagazineSize * _stats.Get(StatType.MagazineSize)));
        }

        void Update()
        {
            if (!Enabled || _slots.Count == 0) return;

            int slot = _input.SlotPressed;
            if (slot >= 0 && slot < _slots.Count) SwitchTo(slot);
            else if (_input.NextWeaponPressed && _slots.Count > 1) SwitchTo((_current + 1) % _slots.Count);

            if (_reloading)
            {
                if (Time.time >= _reloadEnd) FinishReload();
                return;
            }

            if (_input.ReloadPressed) StartReload();

            if (_input.FireHeld && Time.time >= _nextFire) Fire();
        }

        void SwitchTo(int index)
        {
            if (index == _current && _slots.Count > 0 && !_reloading) return;
            CancelReload();
            _current = index;
            _nextFire = Time.time + 0.15f;
            PublishAmmo();
        }

        void StartReload()
        {
            Slot slot = _slots[_current];
            if (_reloading || slot.Ammo >= MagazineOf(slot.Definition)) return;

            float duration = slot.Definition.ReloadTime / Mathf.Max(0.1f, _stats.Get(StatType.ReloadSpeed));
            _reloading = true;
            _reloadEnd = Time.time + duration;
            EventBus<ReloadStarted>.Publish(new ReloadStarted { Duration = duration });
        }

        void FinishReload()
        {
            Slot slot = _slots[_current];
            slot.Ammo = MagazineOf(slot.Definition);
            _reloading = false;
            EventBus<ReloadFinished>.Publish(new ReloadFinished());
            PublishAmmo();
        }

        void CancelReload()
        {
            if (!_reloading) return;
            _reloading = false;
            EventBus<ReloadFinished>.Publish(new ReloadFinished());
        }

        void Fire()
        {
            Slot slot = _slots[_current];
            WeaponDefinition w = slot.Definition;

            if (slot.Ammo <= 0)
            {
                _nextFire = Time.time + 0.25f;
                EventBus<WeaponEmptyClick>.Publish(new WeaponEmptyClick());
                StartReload();
                return;
            }

            _nextFire = Time.time + 1f / Mathf.Max(0.1f, w.FireRate * _stats.Get(StatType.FireRate));
            slot.Ammo--;

            Vector3 aim = _aimDirection();
            Vector3 origin = _muzzle.position;
            int pellets = w.Pellets + Mathf.RoundToInt(_stats.Get(StatType.ExtraProjectiles));
            float spread = pellets > 1 && w.Pellets == 1 ? Mathf.Max(w.SpreadDegrees, MinMultiProjectileSpread) : w.SpreadDegrees;
            int pierce = w.Pierce + Mathf.RoundToInt(_stats.Get(StatType.Pierce));
            float baseDamage = w.Damage * _stats.Get(StatType.Damage);
            float critChance = _stats.Get(StatType.CritChance);
            float critMultiplier = _stats.Get(StatType.CritDamage);

            for (int i = 0; i < pellets; i++)
            {
                float angle = pellets > 1
                    ? Mathf.Lerp(-spread * 0.5f, spread * 0.5f, (i + 0.5f) / pellets) + UnityEngine.Random.Range(-spread, spread) * 0.08f
                    : UnityEngine.Random.Range(-spread * 0.5f, spread * 0.5f);
                Vector3 dir = Quaternion.AngleAxis(angle, Vector3.up) * aim;
                bool crit = UnityEngine.Random.value < critChance;

                GameObject go = _pool.Spawn(_assets.PlayerProjectile, origin, Quaternion.LookRotation(dir));
                if (!go.TryGetComponent(out Projectile projectile)) continue;

                projectile.Launch(new ProjectileSettings
                {
                    Team = Team.Player,
                    Direction = dir,
                    Speed = w.ProjectileSpeed,
                    Damage = crit ? baseDamage * critMultiplier : baseDamage,
                    Range = w.Range,
                    Pierce = pierce,
                    Knockback = w.Knockback,
                    IsCrit = crit,
                    Tint = w.Tint,
                    Scale = w.ProjectileScale
                });
            }

            EventBus<WeaponFired>.Publish(new WeaponFired
            {
                Muzzle = origin,
                Direction = aim,
                Sfx = w.Sfx,
                ShakeStrength = w.ShakeStrength,
                Tint = w.Tint
            });
            PublishAmmo();

            if (slot.Ammo <= 0) StartReload();
        }

        void PublishAmmo()
        {
            if (_slots.Count == 0) return;
            Slot slot = _slots[_current];
            EventBus<WeaponAmmoChanged>.Publish(new WeaponAmmoChanged
            {
                WeaponName = slot.Definition.DisplayName,
                Ammo = slot.Ammo,
                Magazine = MagazineOf(slot.Definition),
                Slot = _current,
                SlotCount = _slots.Count
            });
        }
    }
}
