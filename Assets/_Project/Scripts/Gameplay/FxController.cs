using Project.Core;
using UnityEngine;

namespace Project.Gameplay
{
    // Reacts to gameplay events with pooled-by-design particle bursts. Five long-lived systems cover every effect.
    public sealed class FxController : MonoBehaviour
    {
        ParticleSystem _blood;
        ParticleSystem _sparks;
        ParticleSystem _flash;
        ParticleSystem _dust;
        ParticleSystem _ring;

        void Awake()
        {
            _blood = CreateSystem("Blood", ProceduralTextures.SoftCircle, false, 3f, 2500, ParticleSystemRenderMode.Billboard, 1f, 0.4f);
            _sparks = CreateSystem("Sparks", ProceduralTextures.SoftCircle, true, 4f, 1200, ParticleSystemRenderMode.Billboard, 1f, 0f);
            _flash = CreateSystem("Flash", ProceduralTextures.SoftCircle, true, 0f, 64, ParticleSystemRenderMode.Billboard, 1f, 0.2f);
            _dust = CreateSystem("Dust", ProceduralTextures.SoftCircle, false, -0.1f, 800, ParticleSystemRenderMode.Billboard, 1f, 1.8f);
            _ring = CreateSystem("Ring", ProceduralTextures.Ring, true, 0f, 16, ParticleSystemRenderMode.HorizontalBillboard, 0.15f, 1f);
        }

        void OnEnable()
        {
            EventBus<EnemyDamaged>.Subscribe(OnEnemyDamaged);
            EventBus<EnemyKilled>.Subscribe(OnEnemyKilled);
            EventBus<WeaponFired>.Subscribe(OnWeaponFired);
            EventBus<ProjectileImpact>.Subscribe(OnImpact);
            EventBus<PlayerDamaged>.Subscribe(OnPlayerDamaged);
            EventBus<PlayerDied>.Subscribe(OnPlayerDied);
            EventBus<PlayerDashed>.Subscribe(OnPlayerDashed);
            EventBus<BossSlam>.Subscribe(OnBossSlam);
            EventBus<PickupCollected>.Subscribe(OnPickup);
        }

        void OnDisable()
        {
            EventBus<EnemyDamaged>.Unsubscribe(OnEnemyDamaged);
            EventBus<EnemyKilled>.Unsubscribe(OnEnemyKilled);
            EventBus<WeaponFired>.Unsubscribe(OnWeaponFired);
            EventBus<ProjectileImpact>.Unsubscribe(OnImpact);
            EventBus<PlayerDamaged>.Unsubscribe(OnPlayerDamaged);
            EventBus<PlayerDied>.Unsubscribe(OnPlayerDied);
            EventBus<PlayerDashed>.Unsubscribe(OnPlayerDashed);
            EventBus<BossSlam>.Unsubscribe(OnBossSlam);
            EventBus<PickupCollected>.Unsubscribe(OnPickup);
        }

        // shrink/grow scale the particle size over its life: 1 means "unchanged" at that end.
        ParticleSystem CreateSystem(string systemName, Texture2D texture, bool additive, float gravity, int maxParticles,
            ParticleSystemRenderMode mode, float startScale, float endScale)
        {
            var go = new GameObject(systemName);
            go.transform.SetParent(transform, false);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = ps.main;
            main.loop = true;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = maxParticles;
            main.gravityModifier = gravity;
            main.startLifetime = 0.5f;
            main.startSpeed = 0f;
            main.startSize = 0.2f;

            var emission = ps.emission;
            emission.enabled = false;
            var shape = ps.shape;
            shape.enabled = false;

            var color = ps.colorOverLifetime;
            color.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.7f, 0.4f), new GradientAlphaKey(0f, 1f) });
            color.color = new ParticleSystem.MinMaxGradient(gradient);

            var size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, startScale, 1f, endScale));

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = MaterialLibrary.Particle(texture, additive);
            renderer.renderMode = mode;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            ps.Play();
            return ps;
        }

        static void Emit(ParticleSystem ps, Vector3 position, Vector3 velocity, Color color, float size, float life)
        {
            var p = new ParticleSystem.EmitParams
            {
                position = position,
                velocity = velocity,
                startColor = color,
                startSize = size,
                startLifetime = life
            };
            ps.Emit(p, 1);
        }

        static Vector3 RandomDirection(float upBias)
        {
            Vector3 v = Random.onUnitSphere;
            v.y = Mathf.Abs(v.y) * upBias + upBias * 0.3f;
            return v;
        }

        void Burst(ParticleSystem ps, Vector3 position, Color color, int count, float minSpeed, float maxSpeed, float minSize, float maxSize,
            float minLife, float maxLife, float upBias = 1f)
        {
            for (int i = 0; i < count; i++)
            {
                Vector3 vel = RandomDirection(upBias) * Random.Range(minSpeed, maxSpeed);
                Emit(ps, position, vel, color, Random.Range(minSize, maxSize), Random.Range(minLife, maxLife));
            }
        }

        void OnEnemyDamaged(EnemyDamaged e)
        {
            Burst(_blood, e.Position, e.BloodColor, e.IsCrit ? 9 : 4, 2f, 6f, 0.12f, 0.26f, 0.5f, 0.9f);
        }

        void OnEnemyKilled(EnemyKilled e)
        {
            int count = e.IsBoss ? 140 : Mathf.RoundToInt(16f * e.Scale);
            float spread = e.IsBoss ? 12f : 7f;
            Burst(_blood, e.Position, e.BloodColor, count, 2f, spread, 0.15f * e.Scale, 0.35f * e.Scale, 0.6f, 1.2f);
            Burst(_dust, new Vector3(e.Position.x, 0.2f, e.Position.z), new Color(0.5f, 0.48f, 0.42f, 0.6f), e.IsBoss ? 30 : 5,
                0.5f, 2.5f, 0.6f * e.Scale, 1.2f * e.Scale, 0.6f, 1.1f, 0.4f);
            if (e.IsBoss) Emit(_ring, new Vector3(e.Position.x, 0.1f, e.Position.z), Vector3.zero, new Color(1f, 0.4f, 0.2f, 0.9f), 24f, 0.9f);
        }

        void OnWeaponFired(WeaponFired e)
        {
            Emit(_flash, e.Muzzle, Vector3.zero, new Color(e.Tint.r, e.Tint.g, e.Tint.b, 0.9f), 1f, 0.06f);
            for (int i = 0; i < 3; i++)
            {
                Vector3 vel = (e.Direction + Random.insideUnitSphere * 0.25f) * Random.Range(6f, 14f);
                Emit(_sparks, e.Muzzle, vel, e.Tint, Random.Range(0.05f, 0.1f), Random.Range(0.12f, 0.25f));
            }
        }

        void OnImpact(ProjectileImpact e)
        {
            for (int i = 0; i < 6; i++)
            {
                Vector3 vel = (e.Normal + Random.insideUnitSphere * 0.7f) * Random.Range(2f, 7f);
                Emit(_sparks, e.Position, vel, e.Tint, Random.Range(0.05f, 0.1f), Random.Range(0.15f, 0.35f));
            }
        }

        void OnPlayerDamaged(PlayerDamaged e)
        {
            Burst(_blood, e.Position, new Color(0.7f, 0.05f, 0.05f), 10, 2f, 6f, 0.1f, 0.24f, 0.4f, 0.8f);
        }

        void OnPlayerDied(PlayerDied e)
        {
            Burst(_blood, e.Position + Vector3.up, new Color(0.7f, 0.05f, 0.05f), 50, 2f, 8f, 0.12f, 0.3f, 0.6f, 1.2f);
        }

        void OnPlayerDashed(PlayerDashed e)
        {
            Burst(_dust, new Vector3(e.Position.x, 0.15f, e.Position.z), new Color(0.55f, 0.52f, 0.46f, 0.6f), 10, 0.5f, 2.5f, 0.4f, 0.9f, 0.4f, 0.8f, 0.3f);
        }

        void OnBossSlam(BossSlam e)
        {
            Emit(_ring, new Vector3(e.Position.x, 0.1f, e.Position.z), Vector3.zero, new Color(1f, 0.3f, 0.1f, 0.9f), e.Radius * 2f, 0.55f);
            Burst(_dust, new Vector3(e.Position.x, 0.2f, e.Position.z), new Color(0.5f, 0.45f, 0.4f, 0.7f), 50, 3f, 9f, 0.8f, 1.8f, 0.6f, 1.1f, 0.15f);
        }

        void OnPickup(PickupCollected e)
        {
            Burst(_sparks, e.Position + Vector3.up * 0.7f, new Color(0.4f, 1f, 0.5f), 14, 2f, 6f, 0.08f, 0.14f, 0.3f, 0.6f, 1.5f);
        }
    }
}
