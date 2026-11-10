using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Project.UI
{
    internal static class UiPalette
    {
        public static readonly Color Panel = new Color(0.05f, 0.06f, 0.08f, 0.82f);
        public static readonly Color Dim = new Color(0f, 0f, 0f, 0.62f);
        public static readonly Color Accent = new Color(0.9f, 0.18f, 0.12f);
        public static readonly Color Health = new Color(0.85f, 0.15f, 0.15f);
        public static readonly Color HealthLow = new Color(1f, 0.45f, 0.1f);
        public static readonly Color Boss = new Color(0.8f, 0.1f, 0.35f);
        public static readonly Color Text = new Color(0.94f, 0.94f, 0.92f);
        public static readonly Color TextDim = new Color(0.65f, 0.68f, 0.7f);
        public static readonly Color Gold = new Color(1f, 0.82f, 0.25f);
        public static readonly Color Cyan = new Color(0.3f, 0.9f, 1f);
        public static readonly Color Green = new Color(0.4f, 1f, 0.5f);
        public static readonly Color Common = new Color(0.62f, 0.68f, 0.72f);
        public static readonly Color Rare = new Color(0.25f, 0.6f, 1f);
        public static readonly Color Epic = new Color(0.78f, 0.35f, 1f);
    }

    // Small factory helpers so screens can be built in code without prefabs.
    internal static class UiKit
    {
        static Font _font;
        static Sprite _vignette;
        static Sprite _white;

        public static Font Font => _font != null ? _font : _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        public static Sprite White
        {
            get
            {
                if (_white != null) return _white;
                Texture2D tex = Texture2D.whiteTexture;
                _white = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                return _white;
            }
        }

        // Filled images need a sprite to honour fillAmount.
        public static Image MakeFilled(Image image)
        {
            image.sprite = White;
            image.type = Image.Type.Filled;
            image.fillMethod = Image.FillMethod.Horizontal;
            image.fillAmount = 1f;
            return image;
        }

        // White edge-weighted radial sprite; tint it with Image.color for damage flashes.
        public static Sprite Vignette
        {
            get
            {
                if (_vignette != null) return _vignette;

                const int size = 128;
                var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
                var pixels = new Color32[size * size];
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float u = (x + 0.5f) / size * 2f - 1f;
                        float v = (y + 0.5f) / size * 2f - 1f;
                        float r = Mathf.Sqrt(u * u + v * v) / 1.414f;
                        float a = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.4f, 1f, r));
                        pixels[y * size + x] = new Color(1f, 1f, 1f, a);
                    }
                }

                tex.SetPixels32(pixels);
                tex.Apply();
                _vignette = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
                return _vignette;
            }
        }

        public static RectTransform Rect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        public static void Place(RectTransform rt, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        {
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = position;
            rt.sizeDelta = size;
        }

        public static void Stretch(RectTransform rt, float margin = 0f)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(margin, margin);
            rt.offsetMax = new Vector2(-margin, -margin);
        }

        public static Image Panel(string name, Transform parent, Color color, bool raycast = false)
        {
            RectTransform rt = Rect(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = raycast;
            return img;
        }

        public static Text Label(string name, Transform parent, string text, int size, TextAnchor align, Color color,
            FontStyle style = FontStyle.Bold, bool shadow = true)
        {
            RectTransform rt = Rect(name, parent);
            var t = rt.gameObject.AddComponent<Text>();
            t.font = Font;
            t.text = text;
            t.fontSize = size;
            t.alignment = align;
            t.color = color;
            t.fontStyle = style;
            t.raycastTarget = false;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            if (shadow)
            {
                var s = rt.gameObject.AddComponent<Shadow>();
                s.effectColor = new Color(0f, 0f, 0f, 0.85f);
                s.effectDistance = new Vector2(2f, -2f);
            }

            return t;
        }

        public static Button MakeButton(string name, Transform parent, string label, Vector2 size, Color color, UnityAction onClick)
        {
            Image bg = Panel(name, parent, color, true);
            bg.rectTransform.sizeDelta = size;

            var button = bg.gameObject.AddComponent<Button>();
            button.targetGraphic = bg;
            ColorBlock block = button.colors;
            block.normalColor = new Color(0.85f, 0.85f, 0.85f);
            block.highlightedColor = Color.white;
            block.pressedColor = new Color(0.65f, 0.65f, 0.65f);
            block.selectedColor = new Color(0.85f, 0.85f, 0.85f);
            block.fadeDuration = 0.06f;
            button.colors = block;
            if (onClick != null) button.onClick.AddListener(onClick);

            Text text = Label("Label", bg.transform, label, 34, TextAnchor.MiddleCenter, UiPalette.Text);
            Stretch(text.rectTransform);
            return button;
        }

        public static Slider MakeSlider(string name, Transform parent, Vector2 size, Color fillColor, float value, UnityAction<float> onChanged)
        {
            RectTransform root = Rect(name, parent);
            root.sizeDelta = size;

            Image bg = Panel("Background", root, new Color(0.15f, 0.16f, 0.18f, 1f), true);
            Stretch(bg.rectTransform);

            RectTransform fillArea = Rect("Fill Area", root);
            Stretch(fillArea);
            Image fill = Panel("Fill", fillArea, fillColor);
            fill.rectTransform.anchorMin = Vector2.zero;
            fill.rectTransform.anchorMax = new Vector2(0f, 1f);
            fill.rectTransform.sizeDelta = Vector2.zero;

            RectTransform handleArea = Rect("Handle Slide Area", root);
            Stretch(handleArea);
            handleArea.offsetMin = new Vector2(12f, 0f);
            handleArea.offsetMax = new Vector2(-12f, 0f);
            Image handle = Panel("Handle", handleArea, Color.white, true);
            handle.rectTransform.sizeDelta = new Vector2(24f, size.y + 12f);

            var slider = root.gameObject.AddComponent<Slider>();
            slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = value;
            if (onChanged != null) slider.onValueChanged.AddListener(onChanged);
            return slider;
        }
    }
}
