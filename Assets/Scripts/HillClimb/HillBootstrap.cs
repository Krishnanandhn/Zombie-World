using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace PeakRover
{
    // Builds the whole game at runtime when the HillClimb scene loads, so the scene itself stays empty.
    public static class HillBootstrap
    {
        static readonly Color SkyColor = new Color(0.60f, 0.82f, 0.96f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Run()
        {
            if (SceneManager.GetActiveScene().name != HillConfig.SceneName) return;
            if (GameObject.Find("PeakRover") != null) return;

            Application.targetFrameRate = 60;
            if (Application.isMobilePlatform) Screen.orientation = ScreenOrientation.LandscapeLeft;

            HillAssets.Build();
            SetupLighting();
            Camera cam = SetupCamera();

            var root = new GameObject("PeakRover");
            var game = root.AddComponent<HillGame>();
            var terrain = root.AddComponent<TerrainManager>();
            var pickups = root.AddComponent<PickupManager>();
            var backdrop = root.AddComponent<Backdrop>();
            var hud = root.AddComponent<HillHud>();
            CarController car = CarController.Create(root.transform);

            var follow = cam.GetComponent<HillCamera>();
            if (follow == null) follow = cam.gameObject.AddComponent<HillCamera>();

            // Scene objects are wired first so the first Refresh has a camera position to work from.
            TerrainGen.Seed = Random.Range(0f, 100f);
            car.transform.position = new Vector3(0f, TerrainGen.Height(0f) + 1.1f, 0f);
            follow.Init(car);
            terrain.Init(cam.transform);
            pickups.Init(game, cam.transform);
            backdrop.Init(cam.transform);
            game.RunStarted += follow.Snap;
            game.Init(car, terrain, pickups);
            hud.Init(game);
        }

        static void SetupLighting()
        {
            Light sun = null;
            foreach (Light l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if (l.type == LightType.Directional)
                {
                    sun = l;
                    break;
                }
            }

            if (sun == null)
            {
                sun = new GameObject("Sun").AddComponent<Light>();
                sun.type = LightType.Directional;
            }

            sun.color = new Color(1f, 0.95f, 0.85f);
            sun.intensity = 1.3f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.8f;
            sun.transform.rotation = Quaternion.Euler(40f, -28f, 0f);

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.55f, 0.62f, 0.75f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = SkyColor;
            RenderSettings.fogStartDistance = 60f;
            RenderSettings.fogEndDistance = 220f;
        }

        static Camera SetupCamera()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                var go = new GameObject("Main Camera") { tag = "MainCamera" };
                cam = go.AddComponent<Camera>();
                go.AddComponent<AudioListener>();
            }

            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = SkyColor;
            cam.fieldOfView = HillConfig.CameraFov;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 260f;
            return cam;
        }
    }
}
