using Project.Core;
using Project.Data;
using UnityEngine;

namespace Project.Audio
{
    // Maps gameplay events to sounds and music so no gameplay code ever talks to audio.
    public sealed class AudioEventBinder : MonoBehaviour
    {
        const float AmbientGroanChance = 0.15f;

        IAudioService _audio;

        public void Init(IAudioService audio) => _audio = audio;

        void OnEnable()
        {
            EventBus<WeaponFired>.Subscribe(OnWeaponFired);
            EventBus<WeaponEmptyClick>.Subscribe(OnEmptyClick);
            EventBus<ReloadStarted>.Subscribe(OnReloadStarted);
            EventBus<EnemyDamaged>.Subscribe(OnEnemyDamaged);
            EventBus<EnemyKilled>.Subscribe(OnEnemyKilled);
            EventBus<EnemySpawned>.Subscribe(OnEnemySpawned);
            EventBus<EnemyProjectileFired>.Subscribe(OnEnemyProjectile);
            EventBus<BossRoar>.Subscribe(OnBossRoar);
            EventBus<BossCharging>.Subscribe(OnBossCharging);
            EventBus<BossSlam>.Subscribe(OnBossSlam);
            EventBus<BossDefeated>.Subscribe(OnBossDefeated);
            EventBus<PlayerDamaged>.Subscribe(OnPlayerDamaged);
            EventBus<PlayerDied>.Subscribe(OnPlayerDied);
            EventBus<PlayerDashed>.Subscribe(OnPlayerDashed);
            EventBus<PickupCollected>.Subscribe(OnPickup);
            EventBus<WaveStarted>.Subscribe(OnWaveStarted);
            EventBus<WaveCleared>.Subscribe(OnWaveCleared);
            EventBus<UpgradeOffered>.Subscribe(OnUpgradeOffered);
            EventBus<UpgradeChosen>.Subscribe(OnUpgradeChosen);
            EventBus<GameStateChanged>.Subscribe(OnStateChanged);
        }

        void OnDisable()
        {
            EventBus<WeaponFired>.Unsubscribe(OnWeaponFired);
            EventBus<WeaponEmptyClick>.Unsubscribe(OnEmptyClick);
            EventBus<ReloadStarted>.Unsubscribe(OnReloadStarted);
            EventBus<EnemyDamaged>.Unsubscribe(OnEnemyDamaged);
            EventBus<EnemyKilled>.Unsubscribe(OnEnemyKilled);
            EventBus<EnemySpawned>.Unsubscribe(OnEnemySpawned);
            EventBus<EnemyProjectileFired>.Unsubscribe(OnEnemyProjectile);
            EventBus<BossRoar>.Unsubscribe(OnBossRoar);
            EventBus<BossCharging>.Unsubscribe(OnBossCharging);
            EventBus<BossSlam>.Unsubscribe(OnBossSlam);
            EventBus<BossDefeated>.Unsubscribe(OnBossDefeated);
            EventBus<PlayerDamaged>.Unsubscribe(OnPlayerDamaged);
            EventBus<PlayerDied>.Unsubscribe(OnPlayerDied);
            EventBus<PlayerDashed>.Unsubscribe(OnPlayerDashed);
            EventBus<PickupCollected>.Unsubscribe(OnPickup);
            EventBus<WaveStarted>.Unsubscribe(OnWaveStarted);
            EventBus<WaveCleared>.Unsubscribe(OnWaveCleared);
            EventBus<UpgradeOffered>.Unsubscribe(OnUpgradeOffered);
            EventBus<UpgradeChosen>.Unsubscribe(OnUpgradeChosen);
            EventBus<GameStateChanged>.Unsubscribe(OnStateChanged);
        }

        void OnWeaponFired(WeaponFired e) => _audio.PlaySfx(e.Sfx, 0.8f);

        void OnEmptyClick(WeaponEmptyClick e) => _audio.PlaySfx(SfxId.EmptyClick);

        void OnReloadStarted(ReloadStarted e) => _audio.PlaySfx(SfxId.Reload, 0.7f);

        void OnEnemyDamaged(EnemyDamaged e) => _audio.PlaySfx(SfxId.ZombieHit, e.IsCrit ? 0.9f : 0.55f, e.IsBoss ? 0.02f : 0.12f);

        void OnEnemyKilled(EnemyKilled e)
        {
            if (e.IsBoss) _audio.PlaySfx(SfxId.BossDeath, 1f, 0f);
            else _audio.PlaySfx(SfxId.ZombieDeath, 0.7f, 0.15f);
        }

        void OnEnemySpawned(EnemySpawned e)
        {
            if (e.IsBoss) _audio.PlaySfx(SfxId.BossRoar, 1f, 0f);
            else if (Random.value < AmbientGroanChance) _audio.PlaySfx(SfxId.ZombieGroan, 0.35f, 0.25f);
        }

        void OnEnemyProjectile(EnemyProjectileFired e) => _audio.PlaySfx(SfxId.SpitShot, 0.6f);

        void OnBossRoar(BossRoar e) => _audio.PlaySfx(SfxId.BossRoar, 1f, 0f);

        void OnBossCharging(BossCharging e) => _audio.PlaySfx(SfxId.BossCharge, 0.9f, 0f);

        void OnBossSlam(BossSlam e) => _audio.PlaySfx(SfxId.BossSlam, 1f, 0.03f);

        void OnBossDefeated(BossDefeated e) => _audio.PlayMusic(MusicId.None);

        void OnPlayerDamaged(PlayerDamaged e) => _audio.PlaySfx(SfxId.PlayerHurt, 0.9f);

        void OnPlayerDied(PlayerDied e) => _audio.PlaySfx(SfxId.PlayerDeath, 1f, 0f);

        void OnPlayerDashed(PlayerDashed e) => _audio.PlaySfx(SfxId.Dash, 0.7f);

        void OnPickup(PickupCollected e) => _audio.PlaySfx(SfxId.Pickup, 0.8f, 0.03f);

        void OnWaveStarted(WaveStarted e)
        {
            _audio.PlaySfx(SfxId.WaveStart, 0.9f, 0f);
            _audio.PlayMusic(e.IsBoss ? MusicId.Boss : MusicId.Combat);
        }

        void OnWaveCleared(WaveCleared e) => _audio.PlaySfx(SfxId.WaveClear, 0.9f, 0f);

        void OnUpgradeOffered(UpgradeOffered e) => _audio.PlaySfx(SfxId.UpgradeOffer, 0.8f, 0f);

        void OnUpgradeChosen(UpgradeChosen e) => _audio.PlaySfx(SfxId.UpgradePick, 0.9f, 0f);

        void OnStateChanged(GameStateChanged e)
        {
            switch (e.Current)
            {
                case GameState.Menu:
                    _audio.PlayMusic(MusicId.Menu);
                    break;
                case GameState.Playing:
                    if (e.Previous == GameState.Menu || e.Previous == GameState.GameOver || e.Previous == GameState.Victory)
                    {
                        _audio.PlayMusic(MusicId.Combat);
                    }

                    break;
                case GameState.GameOver:
                    _audio.PlayMusic(MusicId.None);
                    _audio.PlaySfx(SfxId.GameOver, 1f, 0f);
                    break;
                case GameState.Victory:
                    _audio.PlayMusic(MusicId.Menu);
                    _audio.PlaySfx(SfxId.Victory, 1f, 0f);
                    break;
            }
        }
    }
}
