using Project.Audio;
using Project.Core;
using Project.Data;
using Project.Gameplay;
using Project.UI;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Project.Game
{
    // Composition root. Wires services, content, world, audio and UI when the ZombieSurvival scene starts.
    public static class ZombieSurvivalBootstrap
    {
        public const string SceneName = "ZombieSurvival";
        const string RootName = "ZombieSurvivalRoot";
        const int ArenaSeed = 1337;
        const int ProjectilePrewarm = 80;
        const int EnemyPrewarm = 14;
        const float CameraFov = 38f;

        static readonly Color SkyColor = new Color(0.02f, 0.03f, 0.05f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Run()
        {
            if (SceneManager.GetActiveScene().name != SceneName) return;
            if (GameObject.Find(RootName) != null) return;
            Build();
        }

        static void Build()
        {
            Application.targetFrameRate = 60;
            Services.Clear();
            EventBus.ClearAll();

            GameContent content = LoadContent();

            var rootGo = new GameObject(RootName);
            Transform root = rootGo.transform;
            var rootComponent = rootGo.AddComponent<ZombieSurvivalRoot>();

            var save = new JsonSaveService();
            ProfileData profile = save.Load<ProfileData>(ProfileData.SaveKey);
            Services.Register<ISaveService>(save);
            Services.Register(profile);

            var input = new InputService();
            rootComponent.Own(input);
            Services.Register<IInputService>(input);

            Camera cam = SetupCamera(root);
            SetupLighting();
            PostFxSetup.Apply(cam, root);

            var pool = new PoolService(new GameObject("Pool").transform);
            Services.Register<IPoolService>(pool);

            var templateRoot = new GameObject("Templates").transform;
            templateRoot.gameObject.SetActive(false);
            var assets = new GameplayAssets(templateRoot);
            Services.Register(assets);

            LevelDefinition level = content.Level;
            ArenaBuilder.Build(level, ArenaSeed);
            var arena = new ArenaInfo(level.ArenaHalfSize);
            Services.Register(arena);

            var registry = new EnemyRegistry();
            Services.Register(registry);
            var spawner = new EnemySpawner(assets, pool, level);
            Services.Register<IEnemySpawner>(spawner);
            var pickups = new PickupManager(assets, pool);
            Services.Register(pickups);

            Prewarm(content, assets, pool);

            PlayerController player = PlayerController.Create(content.Player, input, pool, assets, cam);
            Services.Register<IPlayerTarget>(player);

            var audio = new GameObject("Audio").AddComponent<AudioService>();
            audio.transform.SetParent(root, false);
            audio.Init(profile);
            Services.Register<IAudioService>(audio);
            audio.gameObject.AddComponent<AudioEventBinder>().Init(audio);

            AddChild<FxController>(root, "Fx");
            var waves = AddChild<WaveDirector>(root, "Waves");
            waves.Init(level, spawner, registry);

            var rig = cam.gameObject.AddComponent<CameraRig>();
            rig.Init(player, arena);

            // UI must exist before the director publishes its first state change.
            AddChild<UIManager>(root, "UI").Init(audio, profile, input);

            AddChild<GameDirector>(root, "Director").Init(content, player, waves, registry, pool, pickups, save, profile, input, rig);
        }

        static T AddChild<T>(Transform parent, string name) where T : Component
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.AddComponent<T>();
        }

        static GameContent LoadContent()
        {
            var content = Resources.Load<GameContent>(GameContent.ResourcePath);
            if (content != null && IsValid(content)) return content;

            if (content != null) Debug.LogWarning("[ZombieSurvival] GameContent asset is incomplete; using built-in defaults.");
            return DefaultContent.Create();
        }

        static bool IsValid(GameContent c)
        {
            return c.Player != null && c.Player.StartingWeapon != null && c.Level != null && c.Level.Waves.Count > 0
                   && c.Upgrades.Count > 0;
        }

        static void Prewarm(GameContent content, GameplayAssets assets, IPoolService pool)
        {
            pool.Prewarm(assets.PlayerProjectile, ProjectilePrewarm);
            pool.Prewarm(assets.EnemyProjectile, 16);
            pool.Prewarm(assets.HealthPickup, 6);

            foreach (EnemyDefinition def in content.Enemies)
            {
                if (def == null) continue;
                int count = def.Behaviour == EnemyBehaviour.Boss ? 1 : EnemyPrewarm;
                pool.Prewarm(assets.GetEnemyTemplate(def), count);
            }
        }

        static Camera SetupCamera(Transform root)
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                var go = new GameObject("Main Camera") { tag = "MainCamera" };
                cam = go.AddComponent<Camera>();
            }

            if (cam.GetComponent<AudioListener>() == null) cam.gameObject.AddComponent<AudioListener>();

            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = SkyColor;
            cam.fieldOfView = CameraFov;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 160f;
            return cam;
        }

        static void SetupLighting()
        {
            Light sun = null;
            foreach (Light l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if (l.type != LightType.Directional) continue;
                sun = l;
                break;
            }

            if (sun == null)
            {
                sun = new GameObject("Moonlight").AddComponent<Light>();
                sun.type = LightType.Directional;
            }

            sun.color = new Color(0.55f, 0.65f, 1f);
            sun.intensity = 1.1f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.9f;
            sun.transform.rotation = Quaternion.Euler(52f, -35f, 0f);

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.2f, 0.23f, 0.34f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.011f;
            RenderSettings.fogColor = new Color(0.04f, 0.06f, 0.09f);
        }
    }
}
