using UnityEngine;
using UnityEngine.SceneManagement;

namespace ZombieWorld
{
    // Builds the entire game at runtime so no scene wiring is required.
    public static class GameBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Run()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            if (sceneName == "HillClimb" || sceneName == "ZombieSurvival") return;
            if (GameObject.Find("ZombieWorld") != null) return;

            Application.targetFrameRate = 60;
            LowPolyAssets.Build();
            Camera cam = WorldBuilder.Build();

            var root = new GameObject("ZombieWorld");
            var gm = root.AddComponent<GameManager>();
            var player = PlayerController.Create(root.transform);
            var zombies = root.AddComponent<ZombieManager>();
            var bullets = root.AddComponent<BulletManager>();
            var fx = root.AddComponent<FxManager>();
            var spawner = root.AddComponent<WaveSpawner>();
            var hud = root.AddComponent<HUD>();

            var follow = cam.GetComponent<CameraFollow>();
            if (follow == null) follow = cam.gameObject.AddComponent<CameraFollow>();

            gm.Init(player, zombies, bullets, fx, spawner);
            follow.Init(player.transform);
            hud.Init(gm);
        }
    }
}
