using UnityEngine;

namespace ZombieWorld
{
    public enum GameState { Playing, GameOver }

    public sealed class GameManager : MonoBehaviour
    {
        const float RestartDelay = 0.8f;

        float _gameOverTime;

        public PlayerController Player { get; private set; }
        public ZombieManager Zombies { get; private set; }
        public BulletManager Bullets { get; private set; }
        public FxManager Fx { get; private set; }
        public WaveSpawner Spawner { get; private set; }
        public InputReader Controls { get; } = new InputReader();

        public GameState State { get; private set; }
        public int Score { get; private set; }
        public int Kills { get; private set; }
        public int Best { get; private set; }

        public void Init(PlayerController player, ZombieManager zombies, BulletManager bullets, FxManager fx, WaveSpawner spawner)
        {
            Player = player;
            Zombies = zombies;
            Bullets = bullets;
            Fx = fx;
            Spawner = spawner;

            Player.Init(this);
            Zombies.Init(this);
            Bullets.Init(this);
            Fx.Init();
            Spawner.Init(this);

            StartGame();
        }

        public void AddKill(int points)
        {
            Kills++;
            Score += points;
        }

        public void OnPlayerDied()
        {
            State = GameState.GameOver;
            _gameOverTime = Time.time;
            if (Score > Best) Best = Score;
            Player.Die();
        }

        void StartGame()
        {
            Zombies.ClearAll();
            Bullets.ClearAll();
            Player.ResetPlayer();
            Spawner.ResetWaves();
            Score = 0;
            Kills = 0;
            State = GameState.Playing;
        }

        void Update()
        {
            if (State == GameState.GameOver && Time.time - _gameOverTime > RestartDelay && Controls.AnyPressed)
            {
                StartGame();
            }
        }
    }
}
