using System.Collections.Generic;
using Project.Data;
using UnityEngine;

namespace Project.Gameplay
{
    // Inactive prefab templates built from primitives; pools clone these.
    public sealed class GameplayAssets
    {
        readonly Transform _root;
        readonly Dictionary<EnemyDefinition, GameObject> _enemyTemplates = new Dictionary<EnemyDefinition, GameObject>();
        readonly Material _trailMaterial;

        public GameObject PlayerProjectile { get; }
        public GameObject EnemyProjectile { get; }
        public GameObject HealthPickup { get; }

        public GameplayAssets(Transform root)
        {
            _root = root;
            _trailMaterial = MaterialLibrary.Particle(Texture2D.whiteTexture, true);
            PlayerProjectile = BuildProjectile("PlayerProjectile", PrimitiveMeshes.Cube, new Vector3(0.07f, 0.07f, 0.6f));
            EnemyProjectile = BuildProjectile("EnemyProjectile", PrimitiveMeshes.Sphere, new Vector3(0.3f, 0.3f, 0.3f));
            HealthPickup = BuildPickup();
        }

        public GameObject GetEnemyTemplate(EnemyDefinition definition)
        {
            if (_enemyTemplates.TryGetValue(definition, out GameObject existing) && existing != null) return existing;

            GameObject template = BuildEnemy(definition);
            _enemyTemplates[definition] = template;
            return template;
        }

        GameObject NewTemplate(string name)
        {
            var go = new GameObject(name);
            go.SetActive(false);
            go.transform.SetParent(_root, false);
            return go;
        }

        GameObject BuildEnemy(EnemyDefinition def)
        {
            GameObject go = NewTemplate("Enemy_" + def.Id);

            var body = go.AddComponent<Rigidbody>();
            body.useGravity = false;
            body.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.mass = Mathf.Max(1f, def.Scale * def.Scale);
            body.linearDamping = 0f;

            var capsule = go.AddComponent<CapsuleCollider>();
            capsule.radius = 0.4f * def.Scale;
            capsule.height = 1.9f * def.Scale;
            capsule.center = new Vector3(0f, 0.95f * def.Scale, 0f);

            go.AddComponent<Health>();
            ModelFactory.BuildHumanoid(go.transform, ModelFactory.StyleFor(def));
            go.AddComponent<ProceduralAnimator>().Configure(true, def.Scale);
            go.AddComponent<Enemy>();
            return go;
        }

        GameObject BuildProjectile(string name, Mesh mesh, Vector3 size)
        {
            GameObject go = NewTemplate(name);
            ModelFactory.Part("Bolt", mesh, go.transform, Vector3.zero, size, MaterialLibrary.Emissive(Color.white), false);

            var trail = go.AddComponent<TrailRenderer>();
            trail.time = 0.14f;
            trail.startWidth = 0.1f;
            trail.endWidth = 0f;
            trail.minVertexDistance = 0.1f;
            trail.sharedMaterial = _trailMaterial;
            trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            trail.receiveShadows = false;

            go.AddComponent<Projectile>();
            return go;
        }

        GameObject BuildPickup()
        {
            GameObject go = NewTemplate("HealthPickup");
            var visual = new GameObject("Visual").transform;
            visual.SetParent(go.transform, false);

            Material white = MaterialLibrary.Lit(new Color(0.92f, 0.92f, 0.92f), 0.4f);
            Material red = MaterialLibrary.Emissive(new Color(1f, 0.1f, 0.12f), 2f);
            ModelFactory.Part("Case", PrimitiveMeshes.Cube, visual, Vector3.zero, new Vector3(0.5f, 0.38f, 0.5f), white);
            ModelFactory.Part("CrossH", PrimitiveMeshes.Cube, visual, new Vector3(0f, 0.2f, 0f), new Vector3(0.34f, 0.04f, 0.1f), red, false);
            ModelFactory.Part("CrossV", PrimitiveMeshes.Cube, visual, new Vector3(0f, 0.2f, 0f), new Vector3(0.1f, 0.04f, 0.34f), red, false);

            go.AddComponent<HealthPickup>();
            return go;
        }
    }
}
