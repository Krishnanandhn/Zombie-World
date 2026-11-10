using UnityEngine;
using UnityEngine.Rendering;

namespace ZombieWorld
{
    public static class WorldBuilder
    {
        static readonly Color FogColor = new Color(0.106f, 0.145f, 0.212f);

        public static Camera Build()
        {
            var ground = new GameObject("Ground");
            LowPolyAssets.AddRenderer(ground, LowPolyAssets.GroundMesh, false);

            var props = new GameObject("Props");
            LowPolyAssets.AddRenderer(props, LowPolyAssets.PropsMesh, true);
            props.isStatic = true;

            SetupLighting();
            return SetupCamera();
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

            sun.color = new Color(1f, 0.85f, 0.65f);
            sun.intensity = 1.2f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.85f;
            sun.transform.rotation = Quaternion.Euler(50f, -35f, 0f);

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.36f, 0.42f, 0.55f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = FogColor;
            RenderSettings.fogStartDistance = 28f;
            RenderSettings.fogEndDistance = 60f;
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
            cam.backgroundColor = FogColor;
            cam.fieldOfView = GameConfig.CameraFov;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 80f;
            cam.transform.rotation = Quaternion.LookRotation(-GameConfig.CameraOffset);
            return cam;
        }
    }
}
