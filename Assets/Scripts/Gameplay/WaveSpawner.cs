using UnityEngine;

namespace ZombieWorld
{
    public sealed class WaveSpawner : MonoBehaviour
    {
        GameManager _gm;
        int _toSpawn;
        float _spawnTimer;
        float _spawnInterval;
        float _breakTimer;

        public int Wave { get; private set; }
        public float WaveStartTime { get; private set; }
        public int Remaining => _toSpawn + _gm.Zombies.ActiveCount;

        public void Init(GameManager gm)
        {
            _gm = gm;
        }

        public void ResetWaves()
        {
            Wave = 0;
            _toSpawn = 0;
            _spawnTimer = 0f;
            _breakTimer = GameConfig.FirstWaveDelay;
            WaveStartTime = -100f;
        }

        void Update()
        {
            if (_gm.State != GameState.Playing) return;

            float dt = Time.deltaTime;
            if (_toSpawn <= 0)
            {
                if (Wave > 0 && _gm.Zombies.ActiveCount > 0) return;

                _breakTimer -= dt;
                if (_breakTimer <= 0f) StartNextWave();
                return;
            }

            _spawnTimer -= dt;
            if (_spawnTimer > 0f) return;

            if (SpawnOne())
            {
                _toSpawn--;
                _spawnTimer = _spawnInterval;
            }
            else
            {
                _spawnTimer = 0.1f;
            }
        }

        void StartNextWave()
        {
            Wave++;
            _toSpawn = 6 + Wave * 4;
            _spawnInterval = Mathf.Max(0.2f, 0.9f - Wave * 0.05f);
            _spawnTimer = 0f;
            _breakTimer = GameConfig.WaveBreak;
            WaveStartTime = Time.time;
            if (Wave > 1) _gm.Player.Heal(GameConfig.WaveHeal);
        }

        bool SpawnOne()
        {
            ZombieKind kind = ZombieKind.Walker;
            float roll = Random.value;
            float bruteChance = Wave >= 3 ? Mathf.Min(0.12f, 0.04f * (Wave - 2)) : 0f;
            float runnerChance = Wave >= 2 ? Mathf.Min(0.35f, 0.1f * (Wave - 1)) : 0f;
            if (roll < bruteChance) kind = ZombieKind.Brute;
            else if (roll < bruteChance + runnerChance) kind = ZombieKind.Runner;

            Vector3 player = _gm.Player.Position;
            float angle = Random.value * Mathf.PI * 2f;
            Vector3 dir = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            float radius = Random.Range(GameConfig.SpawnRadiusMin, GameConfig.SpawnRadiusMax);

            Vector3 pos = ClampToArena(player + dir * radius);
            float minSq = GameConfig.SpawnMinPlayerDistance * GameConfig.SpawnMinPlayerDistance;
            if ((pos - player).sqrMagnitude < minSq) pos = ClampToArena(player - dir * radius);

            return _gm.Zombies.TrySpawn(kind, pos);
        }

        static Vector3 ClampToArena(Vector3 p)
        {
            float limit = GameConfig.PlayAreaHalfSize + 1f;
            return new Vector3(Mathf.Clamp(p.x, -limit, limit), 0f, Mathf.Clamp(p.z, -limit, limit));
        }
    }
}
