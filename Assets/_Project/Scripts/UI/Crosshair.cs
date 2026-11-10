using UnityEngine;
using UnityEngine.UI;

namespace Project.UI
{
    // Four-bar crosshair that follows the pointer; replaces the system cursor during play.
    internal sealed class Crosshair
    {
        const float Gap = 10f;
        const float Length = 14f;
        const float Thickness = 3f;

        readonly RectTransform _root;
        readonly Canvas _canvas;

        public Crosshair(Transform parent, Canvas canvas)
        {
            _canvas = canvas;
            _root = UiKit.Rect("Crosshair", parent);
            _root.anchorMin = Vector2.zero;
            _root.anchorMax = Vector2.zero;
            _root.sizeDelta = Vector2.zero;

            AddBar(new Vector2(0f, Gap + Length * 0.5f), new Vector2(Thickness, Length));
            AddBar(new Vector2(0f, -(Gap + Length * 0.5f)), new Vector2(Thickness, Length));
            AddBar(new Vector2(Gap + Length * 0.5f, 0f), new Vector2(Length, Thickness));
            AddBar(new Vector2(-(Gap + Length * 0.5f), 0f), new Vector2(Length, Thickness));

            Image dot = UiKit.Panel("Dot", _root, new Color(1f, 0.3f, 0.2f, 0.95f));
            UiKit.Place(dot.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(4f, 4f));
            _root.gameObject.SetActive(false);
        }

        void AddBar(Vector2 pos, Vector2 size)
        {
            Image bar = UiKit.Panel("Bar", _root, new Color(1f, 1f, 1f, 0.9f));
            UiKit.Place(bar.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), pos, size);
        }

        public void SetVisible(bool visible)
        {
            if (_root.gameObject.activeSelf != visible) _root.gameObject.SetActive(visible);
        }

        public void SetPosition(Vector2 screenPosition)
        {
            _root.anchoredPosition = screenPosition / _canvas.scaleFactor;
        }
    }
}
