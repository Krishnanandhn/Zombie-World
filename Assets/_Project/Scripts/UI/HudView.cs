using UnityEngine;
using UnityEngine.UI;

namespace Project.UI
{
    // In-game overlay. Pure view: UIManager pushes values in, this class only lays out and animates them.
    internal sealed class HudView
    {
        const float LowHealthFraction = 0.3f;
        const float HintSeconds = 14f;

        readonly RectTransform _root;
        readonly Image _healthFill;
        readonly Text _healthText;
        readonly Image _dashFill;
        readonly Text _waveText;
        readonly Text _remainingText;
        readonly Text _scoreText;
        readonly Text _killsText;
        readonly Text _weaponText;
        readonly Text _ammoText;
        readonly Image _reloadFill;
        readonly GameObject _reloadRoot;
        readonly GameObject _bossRoot;
        readonly Image _bossFill;
        readonly Text _bossName;
        readonly Text _banner;
        readonly Text _subBanner;
        readonly Text _hints;
        readonly Image _vignette;

        float _health = 1f;
        float _maxHealth = 1f;
        float _bannerTimer;
        float _bannerDuration = 1f;
        float _hitFlash;
        float _reloadStart;
        float _reloadDuration;
        float _dashStart = -100f;
        float _dashCooldown = 1f;
        float _hintTimer;
        bool _reloading;
        Color _bannerColor = Color.white;

        public GameObject Root => _root.gameObject;

        public HudView(Transform canvas)
        {
            _root = UiKit.Rect("Hud", canvas);
            UiKit.Stretch(_root);

            _vignette = UiKit.Panel("Vignette", _root, new Color(0.8f, 0f, 0f, 0f));
            _vignette.sprite = UiKit.Vignette;
            UiKit.Stretch(_vignette.rectTransform);

            // Health (bottom-left)
            Image healthBg = UiKit.Panel("HealthBg", _root, new Color(0f, 0f, 0f, 0.6f));
            UiKit.Place(healthBg.rectTransform, Vector2.zero, Vector2.zero, new Vector2(40f, 44f), new Vector2(520f, 36f));
            _healthFill = UiKit.Panel("HealthFill", healthBg.transform, UiPalette.Health);
            UiKit.Stretch(_healthFill.rectTransform, 4f);
            UiKit.MakeFilled(_healthFill);
            _healthText = UiKit.Label("HealthText", healthBg.transform, "100 / 100", 24, TextAnchor.MiddleCenter, UiPalette.Text);
            UiKit.Stretch(_healthText.rectTransform);
            Text healthLabel = UiKit.Label("HealthLabel", _root, "HEALTH", 20, TextAnchor.LowerLeft, UiPalette.TextDim);
            UiKit.Place(healthLabel.rectTransform, Vector2.zero, Vector2.zero, new Vector2(44f, 84f), new Vector2(200f, 28f));

            Image dashBg = UiKit.Panel("DashBg", _root, new Color(0f, 0f, 0f, 0.6f));
            UiKit.Place(dashBg.rectTransform, Vector2.zero, Vector2.zero, new Vector2(40f, 28f), new Vector2(520f, 10f));
            _dashFill = UiKit.Panel("DashFill", dashBg.transform, UiPalette.Cyan);
            UiKit.Stretch(_dashFill.rectTransform, 1f);
            UiKit.MakeFilled(_dashFill);

            // Weapon (bottom-right)
            _ammoText = UiKit.Label("Ammo", _root, "12 / 12", 72, TextAnchor.LowerRight, UiPalette.Text);
            UiKit.Place(_ammoText.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-40f, 40f), new Vector2(420f, 90f));
            _weaponText = UiKit.Label("Weapon", _root, "M9 Sidearm", 30, TextAnchor.LowerRight, UiPalette.Gold);
            UiKit.Place(_weaponText.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-40f, 132f), new Vector2(520f, 40f));

            Image reloadBg = UiKit.Panel("ReloadBg", _root, new Color(0f, 0f, 0f, 0.6f));
            UiKit.Place(reloadBg.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-40f, 26f), new Vector2(300f, 10f));
            _reloadRoot = reloadBg.gameObject;
            _reloadFill = UiKit.Panel("ReloadFill", reloadBg.transform, UiPalette.Gold);
            UiKit.Stretch(_reloadFill.rectTransform, 1f);
            UiKit.MakeFilled(_reloadFill);
            _reloadRoot.SetActive(false);

            // Wave / enemies (top-center)
            _waveText = UiKit.Label("Wave", _root, "WAVE 1 / 8", 46, TextAnchor.UpperCenter, UiPalette.Text);
            UiKit.Place(_waveText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(700f, 60f));
            _remainingText = UiKit.Label("Remaining", _root, "", 28, TextAnchor.UpperCenter, UiPalette.TextDim);
            UiKit.Place(_remainingText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -78f), new Vector2(700f, 40f));

            // Score (top-right)
            _scoreText = UiKit.Label("Score", _root, "0", 56, TextAnchor.UpperRight, UiPalette.Gold);
            UiKit.Place(_scoreText.rectTransform, Vector2.one, Vector2.one, new Vector2(-40f, -24f), new Vector2(500f, 70f));
            _killsText = UiKit.Label("Kills", _root, "0 KILLS", 26, TextAnchor.UpperRight, UiPalette.TextDim);
            UiKit.Place(_killsText.rectTransform, Vector2.one, Vector2.one, new Vector2(-40f, -92f), new Vector2(500f, 36f));

            // Boss bar
            RectTransform boss = UiKit.Rect("Boss", _root);
            UiKit.Place(boss, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -130f), new Vector2(900f, 70f));
            _bossRoot = boss.gameObject;
            _bossName = UiKit.Label("BossName", boss, "", 30, TextAnchor.UpperCenter, new Color(1f, 0.5f, 0.55f));
            UiKit.Place(_bossName.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(900f, 36f));
            Image bossBg = UiKit.Panel("BossBg", boss, new Color(0f, 0f, 0f, 0.7f));
            UiKit.Place(bossBg.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(900f, 26f));
            _bossFill = UiKit.Panel("BossFill", bossBg.transform, UiPalette.Boss);
            UiKit.Stretch(_bossFill.rectTransform, 3f);
            UiKit.MakeFilled(_bossFill);
            _bossRoot.SetActive(false);

            // Banner (center)
            _banner = UiKit.Label("Banner", _root, "", 110, TextAnchor.MiddleCenter, Color.white);
            UiKit.Place(_banner.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 200f), new Vector2(1500f, 140f));
            _subBanner = UiKit.Label("SubBanner", _root, "", 42, TextAnchor.MiddleCenter, UiPalette.Text);
            UiKit.Place(_subBanner.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 110f), new Vector2(1500f, 60f));

            _hints = UiKit.Label("Hints", _root, "WASD move   |   Mouse aim   |   Hold LMB shoot   |   R reload   |   SHIFT dash   |   1-4 / Q weapons   |   ESC pause",
                24, TextAnchor.LowerCenter, UiPalette.TextDim);
            UiKit.Place(_hints.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 14f), new Vector2(1400f, 36f));

            Root.SetActive(false);
        }

        public void OnRunStarted()
        {
            _hintTimer = HintSeconds;
            _bossRoot.SetActive(false);
            _banner.text = string.Empty;
            _subBanner.text = string.Empty;
            _reloading = false;
            _reloadRoot.SetActive(false);
            _dashStart = -100f;
            _hitFlash = 0f;
        }

        public void SetHealth(float current, float max)
        {
            _health = current;
            _maxHealth = Mathf.Max(1f, max);
            _healthFill.fillAmount = Mathf.Clamp01(current / _maxHealth);
            _healthText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(_maxHealth)}";
            _healthFill.color = current / _maxHealth <= LowHealthFraction ? UiPalette.HealthLow : UiPalette.Health;
        }

        public void SetWave(int wave, int total) => _waveText.text = $"WAVE {wave} / {total}";

        public void SetRemaining(int remaining) => _remainingText.text = remaining > 0 ? $"ZOMBIES LEFT  {remaining}" : string.Empty;

        public void SetScore(int score, int kills)
        {
            _scoreText.text = score.ToString("N0");
            _killsText.text = kills == 1 ? "1 KILL" : $"{kills} KILLS";
        }

        public void SetWeapon(string weaponName, int ammo, int magazine, int slot, int slotCount)
        {
            _weaponText.text = slotCount > 1 ? $"[{slot + 1}] {weaponName}" : weaponName;
            _ammoText.text = $"{ammo} / {magazine}";
            _ammoText.color = ammo <= Mathf.Max(1, magazine / 4) ? UiPalette.HealthLow : UiPalette.Text;
        }

        public void StartReload(float duration)
        {
            _reloading = true;
            _reloadStart = Time.unscaledTime;
            _reloadDuration = Mathf.Max(0.05f, duration);
            _reloadRoot.SetActive(true);
        }

        public void StopReload()
        {
            _reloading = false;
            _reloadRoot.SetActive(false);
        }

        public void StartDash(float cooldown)
        {
            _dashStart = Time.unscaledTime;
            _dashCooldown = Mathf.Max(0.1f, cooldown);
        }

        public void ShowBoss(string bossName)
        {
            _bossName.text = bossName.ToUpperInvariant();
            _bossFill.fillAmount = 1f;
            _bossRoot.SetActive(true);
        }

        public void SetBossHealth(float current, float max) => _bossFill.fillAmount = max > 0f ? Mathf.Clamp01(current / max) : 0f;

        public void HideBoss() => _bossRoot.SetActive(false);

        public void ShowBanner(string title, string subtitle, Color color, float duration)
        {
            _banner.text = title;
            _subBanner.text = subtitle;
            _bannerColor = color;
            _bannerDuration = Mathf.Max(0.1f, duration);
            _bannerTimer = _bannerDuration;
        }

        public void HitFlash() => _hitFlash = 1f;

        public void Tick(float dt)
        {
            if (_bannerTimer > 0f)
            {
                _bannerTimer -= dt;
                float t = Mathf.Clamp01(_bannerTimer / _bannerDuration);
                float alpha = Mathf.Min(1f, t * 4f, (1f - t) * 8f + 0.001f);
                float scale = 1f + (1f - t) * 0.12f;
                _banner.color = new Color(_bannerColor.r, _bannerColor.g, _bannerColor.b, alpha);
                _subBanner.color = new Color(UiPalette.Text.r, UiPalette.Text.g, UiPalette.Text.b, alpha);
                _banner.rectTransform.localScale = Vector3.one * scale;
                if (_bannerTimer <= 0f)
                {
                    _banner.text = string.Empty;
                    _subBanner.text = string.Empty;
                }
            }

            _hitFlash = Mathf.MoveTowards(_hitFlash, 0f, dt * 2.5f);
            float low = _health / _maxHealth <= LowHealthFraction && _health > 0f ? 0.25f + Mathf.Sin(Time.unscaledTime * 6f) * 0.12f : 0f;
            _vignette.color = new Color(0.85f, 0f, 0f, Mathf.Max(_hitFlash * 0.7f, low));

            if (_reloading) _reloadFill.fillAmount = Mathf.Clamp01((Time.unscaledTime - _reloadStart) / _reloadDuration);

            _dashFill.fillAmount = Mathf.Clamp01((Time.unscaledTime - _dashStart) / _dashCooldown);

            if (_hintTimer > 0f)
            {
                _hintTimer -= dt;
                _hints.color = new Color(UiPalette.TextDim.r, UiPalette.TextDim.g, UiPalette.TextDim.b, Mathf.Clamp01(_hintTimer));
            }
            else if (_hints.color.a > 0f)
            {
                _hints.color = new Color(1f, 1f, 1f, 0f);
            }
        }
    }
}
