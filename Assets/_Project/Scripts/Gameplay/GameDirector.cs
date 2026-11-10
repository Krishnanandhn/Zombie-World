using System.Collections.Generic;
using Project.Core;
using Project.Data;
using UnityEngine;

namespace Project.Gameplay
{
    // Owns run flow: menu, playing, upgrade pick, pause and end states, plus score and time scale.
    public sealed class GameDirector : MonoBehaviour, IGameFlow
    {
        const float PlayerDeathDelay = 1.6f;
        const float VictoryDelay = 2.8f;
        const float NextWaveDelay = 3f;
        const float HitStopScale = 0.04f;
        const float SlowMotionScale = 0.3f;

        static readonly IState<GameDirector> MenuState = new Menu();
        static readonly IState<GameDirector> PlayingState = new Playing();
        static readonly IState<GameDirector> UpgradeState = new UpgradeSelect();
        static readonly IState<GameDirector> PausedState = new Paused();
        static readonly IState<GameDirector> GameOverState = new Finished();
        static readonly IState<GameDirector> VictoryStateInstance = new Finished();

        readonly List<UpgradeDefinition> _offer = new List<UpgradeDefinition>(4);
        readonly System.Random _rng = new System.Random();

        StateMachine<GameDirector> _machine;
        GameContent _content;
        PlayerController _player;
        WaveDirector _waves;
        EnemyRegistry _registry;
        IPoolService _pool;
        PickupManager _pickups;
        ISaveService _save;
        ProfileData _profile;
        IInputService _input;
        CameraRig _camera;

        int _score;
        int _kills;
        float _runTime;
        float _hitStopUntil;
        float _offerAt = -1f;
        float _endAt;
        bool _ending;
        bool _endingVictory;

        public GameState State { get; private set; } = GameState.Boot;

        public void Init(GameContent content, PlayerController player, WaveDirector waves, EnemyRegistry registry, IPoolService pool,
            PickupManager pickups, ISaveService save, ProfileData profile, IInputService input, CameraRig camera)
        {
            _content = content;
            _player = player;
            _waves = waves;
            _registry = registry;
            _pool = pool;
            _pickups = pickups;
            _save = save;
            _profile = profile;
            _input = input;
            _camera = camera;

            _machine = new StateMachine<GameDirector>(this);
            Services.Register<IGameFlow>(this);
            ShowMenu();
        }

        void OnEnable()
        {
            EventBus<EnemyKilled>.Subscribe(OnEnemyKilled);
            EventBus<PlayerDied>.Subscribe(OnPlayerDied);
            EventBus<BossDefeated>.Subscribe(OnBossDefeated);
            EventBus<WaveCleared>.Subscribe(OnWaveCleared);
            EventBus<HitStopRequested>.Subscribe(OnHitStop);
        }

        void OnDisable()
        {
            EventBus<EnemyKilled>.Unsubscribe(OnEnemyKilled);
            EventBus<PlayerDied>.Unsubscribe(OnPlayerDied);
            EventBus<BossDefeated>.Unsubscribe(OnBossDefeated);
            EventBus<WaveCleared>.Unsubscribe(OnWaveCleared);
            EventBus<HitStopRequested>.Unsubscribe(OnHitStop);
        }

        void OnDestroy() => Time.timeScale = 1f;

        void OnApplicationFocus(bool focus)
        {
            if (!focus && State == GameState.Playing && !_ending) Pause();
        }

        void Update()
        {
            _machine?.Tick(Time.unscaledDeltaTime);
            ApplyTimeScale();
        }

        void ApplyTimeScale()
        {
            float scale;
            switch (State)
            {
                case GameState.Playing:
                    scale = Time.unscaledTime < _hitStopUntil ? HitStopScale : _ending ? SlowMotionScale : 1f;
                    break;
                case GameState.Menu:
                    scale = 1f;
                    break;
                default:
                    scale = 0f;
                    break;
            }

            Time.timeScale = scale;
        }

        public void StartRun()
        {
            ResetWorld();
            _score = 0;
            _kills = 0;
            _runTime = 0f;
            _ending = false;
            _offerAt = -1f;
            _player.ResetForRun(Vector3.zero);
            _camera.Snap();

            EventBus<ScoreChanged>.Publish(new ScoreChanged { Score = 0, Kills = 0 });
            EventBus<RunStarted>.Publish(new RunStarted { TotalWaves = _waves.WaveCount });
            Go(GameState.Playing, PlayingState);
            _waves.BeginWave(0, _content.Level.FirstWaveDelay);
        }

        public void Restart() => StartRun();

        public void ReturnToMenu()
        {
            SaveProfile();
            ShowMenu();
        }

        public void Pause()
        {
            if (State == GameState.Playing) Go(GameState.Paused, PausedState);
        }

        public void Resume()
        {
            if (State != GameState.Paused) return;
            SaveProfile();
            Go(GameState.Playing, PlayingState);
        }

        public void ChooseUpgrade(string upgradeId)
        {
            if (State != GameState.UpgradeSelect) return;

            UpgradeDefinition chosen = null;
            for (int i = 0; i < _offer.Count; i++)
            {
                if (_offer[i].Id == upgradeId) chosen = _offer[i];
            }

            if (chosen == null) return;

            _player.ApplyUpgrade(chosen);
            EventBus<UpgradeChosen>.Publish(new UpgradeChosen { Id = chosen.Id, DisplayName = chosen.DisplayName });
            Go(GameState.Playing, PlayingState);
            _waves.BeginWave(_waves.WaveIndex + 1, NextWaveDelay);
        }

        public void Quit()
        {
            SaveProfile();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        void ShowMenu()
        {
            ResetWorld();
            _ending = false;
            _offerAt = -1f;
            _player.ResetForRun(Vector3.zero);
            _camera.Snap();
            Go(GameState.Menu, MenuState);
        }

        void ResetWorld()
        {
            _waves.Stop();
            _registry.DespawnAll(_pool);
            Projectile.DespawnAll(_pool);
            _pickups.Clear();
        }

        void Go(GameState next, IState<GameDirector> state)
        {
            GameState previous = State;
            State = next;
            _machine.Set(state);
            EventBus<GameStateChanged>.Publish(new GameStateChanged { Previous = previous, Current = next });
        }

        void SaveProfile() => _save.Save(ProfileData.SaveKey, _profile);

        void BeginEnding(bool victory, float delay)
        {
            if (_ending) return;
            _ending = true;
            _endingVictory = victory;
            _endAt = Time.unscaledTime + delay;
            _offerAt = -1f;
        }

        void FinishRun(bool victory)
        {
            bool best = _score > _profile.BestScore;
            if (best) _profile.BestScore = _score;
            _profile.BestWave = Mathf.Max(_profile.BestWave, _waves.WaveIndex + 1);
            _profile.TotalKills += _kills;
            if (victory) _profile.Victories++;
            SaveProfile();

            EventBus<RunEnded>.Publish(new RunEnded
            {
                Victory = victory,
                Score = _score,
                Kills = _kills,
                Wave = _waves.WaveIndex + 1,
                Duration = _runTime,
                NewBest = best
            });

            _ending = false;
            _waves.Stop();
            if (victory) Go(GameState.Victory, VictoryStateInstance);
            else Go(GameState.GameOver, GameOverState);
        }

        void OfferUpgrades()
        {
            UpgradeRoller.Roll(_content.Upgrades, IsUpgradeAvailable, _content.Level.UpgradeChoices, _rng, _offer);
            if (_offer.Count == 0)
            {
                _waves.BeginWave(_waves.WaveIndex + 1, NextWaveDelay);
                return;
            }

            EventBus<UpgradeOffered>.Publish(new UpgradeOffered { Options = _offer.ToArray(), Wave = _waves.WaveIndex + 1 });
            Go(GameState.UpgradeSelect, UpgradeState);
        }

        bool IsUpgradeAvailable(UpgradeDefinition upgrade)
        {
            if (!_player.Stats.CanTake(upgrade)) return false;
            return upgrade.UnlockWeapon == null || !_player.Weapons.Owns(upgrade.UnlockWeapon);
        }

        void AddScore(int amount, int kills)
        {
            _score += amount;
            _kills += kills;
            EventBus<ScoreChanged>.Publish(new ScoreChanged { Score = _score, Kills = _kills });
        }

        void OnEnemyKilled(EnemyKilled e)
        {
            if (State != GameState.Playing) return;
            AddScore(e.Score, 1);
            float stop = e.IsBoss ? _content.BossKillHitStop : _content.KillHitStop;
            _hitStopUntil = Mathf.Max(_hitStopUntil, Time.unscaledTime + stop);
        }

        void OnHitStop(HitStopRequested e)
        {
            _hitStopUntil = Mathf.Max(_hitStopUntil, Time.unscaledTime + e.Duration);
        }

        void OnPlayerDied(PlayerDied e)
        {
            if (State == GameState.Playing) BeginEnding(false, PlayerDeathDelay);
        }

        void OnBossDefeated(BossDefeated e)
        {
            if (State == GameState.Playing) BeginEnding(true, VictoryDelay);
        }

        void OnWaveCleared(WaveCleared e)
        {
            if (State != GameState.Playing || _ending) return;

            LevelDefinition level = _content.Level;
            AddScore(level.ScorePerWaveCleared * e.Wave, 0);

            if (e.Wave >= e.TotalWaves)
            {
                BeginEnding(true, VictoryDelay);
                return;
            }

            _player.Heal(_player.Health.Max * level.HealOnWaveClearFraction);
            _player.Weapons.RefillAll();
            _offerAt = Time.time + level.WaveClearDelay;
        }

        sealed class Menu : IState<GameDirector>
        {
            public void Enter(GameDirector d) => d._player.Active = false;

            public void Exit(GameDirector d)
            {
            }

            public void Tick(GameDirector d, float dt)
            {
            }
        }

        sealed class Playing : IState<GameDirector>
        {
            public void Enter(GameDirector d) => d._player.Active = d._player.IsAlive;

            public void Exit(GameDirector d)
            {
            }

            public void Tick(GameDirector d, float dt)
            {
                d._runTime += Time.deltaTime;

                if (d._ending)
                {
                    if (Time.unscaledTime >= d._endAt) d.FinishRun(d._endingVictory);
                    return;
                }

                if (d._input.PausePressed)
                {
                    d.Pause();
                    return;
                }

                if (d._offerAt >= 0f && Time.time >= d._offerAt)
                {
                    d._offerAt = -1f;
                    d.OfferUpgrades();
                }
            }
        }

        sealed class UpgradeSelect : IState<GameDirector>
        {
            public void Enter(GameDirector d) => d._player.Active = false;

            public void Exit(GameDirector d)
            {
            }

            public void Tick(GameDirector d, float dt)
            {
                int slot = d._input.SlotPressed;
                if (slot >= 0 && slot < d._offer.Count) d.ChooseUpgrade(d._offer[slot].Id);
            }
        }

        sealed class Paused : IState<GameDirector>
        {
            public void Enter(GameDirector d) => d._player.Active = false;

            public void Exit(GameDirector d)
            {
            }

            public void Tick(GameDirector d, float dt)
            {
                if (d._input.PausePressed) d.Resume();
            }
        }

        sealed class Finished : IState<GameDirector>
        {
            public void Enter(GameDirector d) => d._player.Active = false;

            public void Exit(GameDirector d)
            {
            }

            public void Tick(GameDirector d, float dt)
            {
            }
        }
    }
}
