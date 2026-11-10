using Project.Core;
using Project.Data;
using UnityEngine;

namespace Project.Gameplay
{
    // Runs one wave at a time: countdown, timed spawn groups, then reports when the arena is clear.
    public sealed class WaveDirector : MonoBehaviour
    {
        const float MaxSpeedScaling = 0.3f;

        enum Phase
        {
            Idle,
            Countdown,
            Active
        }

        LevelDefinition _level;
        IEnemySpawner _spawner;
        EnemyRegistry _registry;
        Phase _phase;
        int[] _spawned;
        int _totalToSpawn;
        int _lastRemaining = -1;
        int _lastCountdown = -1;
        float _countdownEnd;
        float _waveStart;

        public int WaveIndex { get; private set; } = -1;
        public int WaveCount => _level != null ? _level.Waves.Count : 0;

        public void Init(LevelDefinition level, IEnemySpawner spawner, EnemyRegistry registry)
        {
            _level = level;
            _spawner = spawner;
            _registry = registry;
        }

        public void Stop()
        {
            _phase = Phase.Idle;
            WaveIndex = -1;
        }

        public void BeginWave(int index, float delay)
        {
            WaveIndex = index;
            _phase = Phase.Countdown;
            _countdownEnd = Time.time + delay;
            _lastCountdown = -1;
        }

        void Update()
        {
            switch (_phase)
            {
                case Phase.Countdown: TickCountdown(); break;
                case Phase.Active: TickActive(); break;
            }
        }

        void TickCountdown()
        {
            float left = _countdownEnd - Time.time;
            int shown = Mathf.CeilToInt(Mathf.Max(0f, left));
            if (shown != _lastCountdown)
            {
                _lastCountdown = shown;
                EventBus<WaveCountdown>.Publish(new WaveCountdown { Wave = WaveIndex + 1, SecondsLeft = left });
            }

            if (left <= 0f) StartWave();
        }

        void StartWave()
        {
            WaveDefinition wave = _level.Waves[WaveIndex];
            _phase = Phase.Active;
            _waveStart = Time.time;
            _spawned = new int[wave.Groups.Count];
            _totalToSpawn = wave.TotalEnemies;
            _lastRemaining = -1;

            EventBus<WaveStarted>.Publish(new WaveStarted { Wave = WaveIndex + 1, TotalWaves = WaveCount, IsBoss = wave.IsBossWave });
        }

        void TickActive()
        {
            WaveDefinition wave = _level.Waves[WaveIndex];
            float elapsed = Time.time - _waveStart;
            int spawnedTotal = 0;

            for (int g = 0; g < wave.Groups.Count; g++)
            {
                SpawnGroup group = wave.Groups[g];
                if (_spawned[g] < group.Count
                    && elapsed >= group.StartDelay + _spawned[g] * group.Interval
                    && _registry.Count < _level.MaxAlive)
                {
                    SpawnOne(group.Enemy);
                    _spawned[g]++;
                }

                spawnedTotal += _spawned[g];
            }

            int remaining = _totalToSpawn - spawnedTotal + _registry.Count;
            if (remaining != _lastRemaining)
            {
                _lastRemaining = remaining;
                EventBus<EnemiesRemainingChanged>.Publish(new EnemiesRemainingChanged { Remaining = remaining, Total = _totalToSpawn });
            }

            if (spawnedTotal >= _totalToSpawn && _registry.Count == 0)
            {
                _phase = Phase.Idle;
                EventBus<WaveCleared>.Publish(new WaveCleared { Wave = WaveIndex + 1, TotalWaves = WaveCount });
            }
        }

        void SpawnOne(EnemyDefinition definition)
        {
            if (!_spawner.TryGetSpawnPoint(out Vector3 point)) point = FallbackPoint();

            bool boss = definition.Behaviour == EnemyBehaviour.Boss;
            float healthScale = boss ? 1f : 1f + _level.HealthScalingPerWave * WaveIndex;
            float speedScale = boss ? 1f : 1f + Mathf.Min(MaxSpeedScaling, _level.SpeedScalingPerWave * WaveIndex);
            _spawner.Spawn(definition, point, healthScale, speedScale);
        }

        Vector3 FallbackPoint()
        {
            float limit = _level.ArenaHalfSize - 3f;
            return new Vector3(Random.Range(-limit, limit), 0f, limit * (Random.value < 0.5f ? -1f : 1f));
        }
    }
}
