using UnityEngine;
using UnityEngine.UI;

namespace ZombieWorld
{
    public sealed class HUD : MonoBehaviour
    {
        GameManager _gm;
        Font _font;
        Sprite _circle;
        Canvas _canvas;
        RectTransform _canvasRt;
        RectTransform _healthFill;
        RectTransform _joyBase;
        RectTransform _joyKnob;
        Text _scoreText;
        Text _waveText;
        Text _messageText;
        Text _debugText;

        float _lastHealthPct = -1f;
        int _lastScore = -1;
        int _lastWave = -1;
        int _lastRemaining = -1;
        int _messageKey = int.MinValue;
        float _debugTimer;

        public void Init(GameManager gm)
        {
            _gm = gm;
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _circle = CreateCircleSprite();

            var canvasGo = new GameObject("HUDCanvas");
            canvasGo.transform.SetParent(transform, false);
            _canvas = canvasGo.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            _canvasRt = (RectTransform)canvasGo.transform;

            var healthBg = NewImage("HealthBg", _canvasRt, null, new Color(0f, 0f, 0f, 0.5f),
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(30f, -30f), new Vector2(360f, 36f));
            var fill = NewImage("HealthFill", healthBg.rectTransform, null, new Color(0.85f, 0.27f, 0.24f),
                Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
            _healthFill = fill.rectTransform;
            _healthFill.anchorMin = Vector2.zero;
            _healthFill.anchorMax = Vector2.one;
            _healthFill.offsetMin = Vector2.zero;
            _healthFill.offsetMax = Vector2.zero;

            _scoreText = NewText("Score", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-30f, -24f),
                new Vector2(500f, 60f), 44, TextAnchor.UpperRight);
            _waveText = NewText("Wave", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -24f),
                new Vector2(700f, 60f), 38, TextAnchor.UpperCenter);
            _messageText = NewText("Message", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(1400f, 500f), 72, TextAnchor.MiddleCenter);
            _debugText = NewText("Debug", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(20f, 16f),
                new Vector2(900f, 40f), 24, TextAnchor.LowerLeft);
            _debugText.color = new Color(1f, 1f, 1f, 0.7f);

            _joyBase = NewImage("JoystickBase", _canvasRt, _circle, new Color(1f, 1f, 1f, 0.25f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(220f, 220f)).rectTransform;
            _joyKnob = NewImage("JoystickKnob", _canvasRt, _circle, new Color(1f, 1f, 1f, 0.55f),
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(100f, 100f)).rectTransform;
            _joyBase.gameObject.SetActive(false);
            _joyKnob.gameObject.SetActive(false);
        }

        void Update()
        {
            float pct = Mathf.Clamp01((float)_gm.Player.Health / GameConfig.PlayerMaxHealth);
            if (!Mathf.Approximately(pct, _lastHealthPct))
            {
                _lastHealthPct = pct;
                _healthFill.anchorMax = new Vector2(pct, 1f);
            }

            if (_gm.Score != _lastScore)
            {
                _lastScore = _gm.Score;
                _scoreText.text = "SCORE " + _lastScore;
            }

            int wave = _gm.Spawner.Wave;
            int remaining = _gm.Spawner.Remaining;
            if (wave != _lastWave || remaining != _lastRemaining)
            {
                _lastWave = wave;
                _lastRemaining = remaining;
                _waveText.text = wave == 0 ? "GET READY" : "WAVE " + wave + "   LEFT " + remaining;
            }

            UpdateMessage(wave);
            UpdateJoystick();
            UpdateDebug();
        }

        void UpdateMessage(int wave)
        {
            int key;
            if (_gm.State == GameState.GameOver) key = -1;
            else if (wave == 0) key = -2;
            else if (Time.time - _gm.Spawner.WaveStartTime < 2f) key = wave;
            else key = 0;

            if (key == _messageKey) return;
            _messageKey = key;

            if (key == -1)
                _messageText.text = "GAME OVER\nScore " + _gm.Score + "   Best " + _gm.Best + "\nTap or press any key to restart";
            else if (key == -2)
                _messageText.text = "Drag or WASD to move\nAuto-fire at the nearest zombie";
            else if (key > 0)
                _messageText.text = "WAVE " + key;
            else
                _messageText.text = string.Empty;
        }

        void UpdateJoystick()
        {
            bool active = _gm.State == GameState.Playing && _gm.Controls.JoystickActive;
            if (active != _joyBase.gameObject.activeSelf)
            {
                _joyBase.gameObject.SetActive(active);
                _joyKnob.gameObject.SetActive(active);
            }
            if (!active) return;

            float diameter = 2f * InputReader.JoystickRadiusPx / _canvas.scaleFactor;
            _joyBase.sizeDelta = new Vector2(diameter, diameter);
            _joyKnob.sizeDelta = new Vector2(diameter * 0.45f, diameter * 0.45f);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRt, _gm.Controls.JoystickOrigin, null, out Vector2 basePos);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRt, _gm.Controls.JoystickKnob, null, out Vector2 knobPos);
            _joyBase.anchoredPosition = basePos;
            _joyKnob.anchoredPosition = knobPos;
        }

        void UpdateDebug()
        {
            _debugTimer -= Time.unscaledDeltaTime;
            if (_debugTimer > 0f) return;
            _debugTimer = 0.25f;

            int fps = Mathf.RoundToInt(1f / Mathf.Max(Time.smoothDeltaTime, 0.0001f));
            _debugText.text = "FPS " + fps
                + "   Zombies " + _gm.Zombies.ActiveCount + " (pool " + _gm.Zombies.PooledTotal + ")"
                + "   Bullets " + _gm.Bullets.ActiveCount + " (pool " + _gm.Bullets.PooledTotal + ")";
        }

        RectTransform NewRect(string objectName, Transform parent, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(objectName, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return rt;
        }

        Image NewImage(string objectName, Transform parent, Sprite sprite, Color color, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            RectTransform rt = NewRect(objectName, parent, anchor, pivot, pos, size);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        Text NewText(string objectName, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size, int fontSize, TextAnchor align)
        {
            RectTransform rt = NewRect(objectName, _canvasRt, anchor, pivot, pos, size);
            var text = rt.gameObject.AddComponent<Text>();
            text.font = _font;
            text.fontSize = fontSize;
            text.alignment = align;
            text.color = Color.white;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            var shadow = rt.gameObject.AddComponent<Shadow>();
            shadow.effectDistance = new Vector2(2f, -2f);
            return text;
        }

        static Sprite CreateCircleSprite()
        {
            const int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            var pixels = new Color32[size * size];
            float c = (size - 1) * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c));
                    float a = Mathf.Clamp01(c - dist + 0.5f);
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255f));
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
