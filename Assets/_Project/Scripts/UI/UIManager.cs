using Project.Core;
using Project.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Project.UI
{
    // Presenter: translates game events into view updates and view buttons into IGameFlow commands.
    public sealed class UIManager : MonoBehaviour
    {
        static readonly Color WaveColor = new Color(1f, 0.85f, 0.3f);
        static readonly Color ClearColor = new Color(0.4f, 1f, 0.5f);
        static readonly Color BossColor = new Color(1f, 0.2f, 0.3f);

        IAudioService _audio;
        ProfileData _profile;
        IInputService _input;
        Canvas _canvas;
        HudView _hud;
        MainMenuView _menu;
        PauseView _pause;
        UpgradeView _upgrades;
        EndView _end;
        DamageNumbers _numbers;
        Crosshair _crosshair;

        UpgradeDefinition[] _offer;
        int _offerWave;
        RunEnded _lastRun;
        int _totalWaves;
        GameState _state = GameState.Boot;

        public void Init(IAudioService audio, ProfileData profile, IInputService input)
        {
            _audio = audio;
            _profile = profile;
            _input = input;

            EnsureEventSystem();
            BuildCanvas();

            _menu = new MainMenuView(_canvas.transform, () => Command(f => f.StartRun()), () => Command(f => f.Quit()));
            _hud = new HudView(_canvas.transform);
            _numbers = new DamageNumbers(_canvas.transform, _canvas);
            _upgrades = new UpgradeView(_canvas.transform, id => Command(f => f.ChooseUpgrade(id)));
            _pause = new PauseView(_canvas.transform, _audio.MasterVolume, _audio.MusicVolume, _audio.SfxVolume,
                v => _audio.MasterVolume = v, v => _audio.MusicVolume = v, v => _audio.SfxVolume = v,
                () => Command(f => f.Resume()), () => Command(f => f.Restart()), () => Command(f => f.ReturnToMenu()));
            _end = new EndView(_canvas.transform, () => Command(f => f.Restart()), () => Command(f => f.ReturnToMenu()));
            _crosshair = new Crosshair(_canvas.transform, _canvas);
        }

        void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;

            var go = new GameObject("EventSystem");
            go.transform.SetParent(transform, false);
            go.AddComponent<EventSystem>();
            go.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }

        void BuildCanvas()
        {
            var go = new GameObject("UICanvas");
            go.transform.SetParent(transform, false);
            _canvas = go.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 10;

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();
        }

        void Command(System.Action<IGameFlow> action)
        {
            _audio.PlaySfx(SfxId.UiClick, 0.8f, 0.02f);
            IGameFlow flow = Services.Get<IGameFlow>();
            if (flow != null) action(flow);
        }

        void OnEnable()
        {
            EventBus<GameStateChanged>.Subscribe(OnStateChanged);
            EventBus<RunStarted>.Subscribe(OnRunStarted);
            EventBus<RunEnded>.Subscribe(OnRunEnded);
            EventBus<WaveCountdown>.Subscribe(OnWaveCountdown);
            EventBus<WaveStarted>.Subscribe(OnWaveStarted);
            EventBus<WaveCleared>.Subscribe(OnWaveCleared);
            EventBus<EnemiesRemainingChanged>.Subscribe(OnRemaining);
            EventBus<EnemyDamaged>.Subscribe(OnEnemyDamaged);
            EventBus<ScoreChanged>.Subscribe(OnScore);
            EventBus<PlayerHealthChanged>.Subscribe(OnPlayerHealth);
            EventBus<PlayerDamaged>.Subscribe(OnPlayerDamaged);
            EventBus<PlayerDashed>.Subscribe(OnDashed);
            EventBus<WeaponAmmoChanged>.Subscribe(OnAmmo);
            EventBus<ReloadStarted>.Subscribe(OnReloadStarted);
            EventBus<ReloadFinished>.Subscribe(OnReloadFinished);
            EventBus<BossSpawned>.Subscribe(OnBossSpawned);
            EventBus<BossHealthChanged>.Subscribe(OnBossHealth);
            EventBus<BossDefeated>.Subscribe(OnBossDefeated);
            EventBus<UpgradeOffered>.Subscribe(OnUpgradeOffered);
        }

        void OnDisable()
        {
            EventBus<GameStateChanged>.Unsubscribe(OnStateChanged);
            EventBus<RunStarted>.Unsubscribe(OnRunStarted);
            EventBus<RunEnded>.Unsubscribe(OnRunEnded);
            EventBus<WaveCountdown>.Unsubscribe(OnWaveCountdown);
            EventBus<WaveStarted>.Unsubscribe(OnWaveStarted);
            EventBus<WaveCleared>.Unsubscribe(OnWaveCleared);
            EventBus<EnemiesRemainingChanged>.Unsubscribe(OnRemaining);
            EventBus<EnemyDamaged>.Unsubscribe(OnEnemyDamaged);
            EventBus<ScoreChanged>.Unsubscribe(OnScore);
            EventBus<PlayerHealthChanged>.Unsubscribe(OnPlayerHealth);
            EventBus<PlayerDamaged>.Unsubscribe(OnPlayerDamaged);
            EventBus<PlayerDashed>.Unsubscribe(OnDashed);
            EventBus<WeaponAmmoChanged>.Unsubscribe(OnAmmo);
            EventBus<ReloadStarted>.Unsubscribe(OnReloadStarted);
            EventBus<ReloadFinished>.Unsubscribe(OnReloadFinished);
            EventBus<BossSpawned>.Unsubscribe(OnBossSpawned);
            EventBus<BossHealthChanged>.Unsubscribe(OnBossHealth);
            EventBus<BossDefeated>.Unsubscribe(OnBossDefeated);
            EventBus<UpgradeOffered>.Unsubscribe(OnUpgradeOffered);
            Cursor.visible = true;
        }

        void Update()
        {
            if (_hud == null) return;

            float dt = Time.unscaledDeltaTime;
            _hud.Tick(dt);
            _numbers.Tick(dt);

            bool showCrosshair = _state == GameState.Playing && !_input.UsingGamepad;
            _crosshair.SetVisible(showCrosshair);
            if (showCrosshair) _crosshair.SetPosition(_input.PointerPosition);
            Cursor.visible = !showCrosshair;
        }

        void OnStateChanged(GameStateChanged e)
        {
            _state = e.Current;
            if (_menu == null) return;

            _menu.Root.SetActive(e.Current == GameState.Menu);
            _hud.Root.SetActive(e.Current == GameState.Playing || e.Current == GameState.Paused || e.Current == GameState.UpgradeSelect);
            _pause.Root.SetActive(e.Current == GameState.Paused);
            _upgrades.Root.SetActive(e.Current == GameState.UpgradeSelect);
            _end.Root.SetActive(e.Current == GameState.GameOver || e.Current == GameState.Victory);

            switch (e.Current)
            {
                case GameState.Menu:
                    _menu.SetBest(_profile.BestScore, _profile.BestWave, _profile.Victories);
                    _numbers.Clear();
                    break;
                case GameState.UpgradeSelect:
                    _upgrades.Show(_offer, _offerWave);
                    break;
                case GameState.GameOver:
                case GameState.Victory:
                    _end.Show(_lastRun.Victory, _lastRun.Score, _lastRun.Kills, _lastRun.Wave, _lastRun.Duration, _lastRun.NewBest);
                    break;
            }
        }

        void OnRunStarted(RunStarted e)
        {
            _totalWaves = e.TotalWaves;
            _hud.OnRunStarted();
            _hud.SetWave(1, _totalWaves);
            _hud.SetRemaining(0);
            _numbers.Clear();
        }

        void OnRunEnded(RunEnded e) => _lastRun = e;

        void OnWaveCountdown(WaveCountdown e)
        {
            if (e.SecondsLeft <= 0f) return;
            _hud.SetWave(e.Wave, _totalWaves);
            _hud.ShowBanner("GET READY", $"Wave {e.Wave} in {Mathf.CeilToInt(e.SecondsLeft)}", WaveColor, 1f);
        }

        void OnWaveStarted(WaveStarted e)
        {
            _hud.SetWave(e.Wave, e.TotalWaves);
            if (e.IsBoss) _hud.ShowBanner("THE ABOMINATION", "BOSS WAVE", BossColor, 2.6f);
            else _hud.ShowBanner($"WAVE {e.Wave}", "Survive", WaveColor, 1.8f);
        }

        void OnWaveCleared(WaveCleared e)
        {
            _hud.ShowBanner("WAVE CLEARED", "+ bonus score", ClearColor, 1.6f);
            _hud.SetRemaining(0);
        }

        void OnRemaining(EnemiesRemainingChanged e) => _hud.SetRemaining(e.Remaining);

        void OnEnemyDamaged(EnemyDamaged e) => _numbers.Spawn(e.Position, e.Amount, e.IsCrit);

        void OnScore(ScoreChanged e) => _hud.SetScore(e.Score, e.Kills);

        void OnPlayerHealth(PlayerHealthChanged e) => _hud.SetHealth(e.Current, e.Max);

        void OnPlayerDamaged(PlayerDamaged e) => _hud.HitFlash();

        void OnDashed(PlayerDashed e) => _hud.StartDash(e.Cooldown);

        void OnAmmo(WeaponAmmoChanged e) => _hud.SetWeapon(e.WeaponName, e.Ammo, e.Magazine, e.Slot, e.SlotCount);

        void OnReloadStarted(ReloadStarted e) => _hud.StartReload(e.Duration);

        void OnReloadFinished(ReloadFinished e) => _hud.StopReload();

        void OnBossSpawned(BossSpawned e) => _hud.ShowBoss(e.Name);

        void OnBossHealth(BossHealthChanged e) => _hud.SetBossHealth(e.Current, e.Max);

        void OnBossDefeated(BossDefeated e)
        {
            _hud.HideBoss();
            _hud.ShowBanner("ABOMINATION SLAIN", "The yard is quiet", ClearColor, 2.6f);
        }

        void OnUpgradeOffered(UpgradeOffered e)
        {
            _offer = e.Options;
            _offerWave = e.Wave;
        }
    }
}
