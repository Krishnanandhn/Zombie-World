using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Project.UI
{
    internal sealed class PauseView
    {
        readonly RectTransform _root;

        public GameObject Root => _root.gameObject;

        public PauseView(Transform canvas, float master, float music, float sfx, UnityAction<float> onMaster, UnityAction<float> onMusic,
            UnityAction<float> onSfx, UnityAction onResume, UnityAction onRestart, UnityAction onMenu)
        {
            _root = UiKit.Rect("Pause", canvas);
            UiKit.Stretch(_root);

            Image dim = UiKit.Panel("Dim", _root, UiPalette.Dim, true);
            UiKit.Stretch(dim.rectTransform);

            Image panel = UiKit.Panel("Panel", _root, UiPalette.Panel);
            UiKit.Place(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760f, 800f));

            Text title = UiKit.Label("Title", panel.transform, "PAUSED", 80, TextAnchor.MiddleCenter, UiPalette.Text);
            UiKit.Place(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(700f, 100f));

            AddSlider(panel.transform, "MASTER", master, -170f, UiPalette.Gold, onMaster);
            AddSlider(panel.transform, "MUSIC", music, -250f, UiPalette.Cyan, onMusic);
            AddSlider(panel.transform, "EFFECTS", sfx, -330f, UiPalette.Green, onSfx);

            AddButton(panel.transform, "RESUME", -450f, UiPalette.Accent, onResume);
            AddButton(panel.transform, "RESTART RUN", -550f, new Color(0.25f, 0.27f, 0.3f), onRestart);
            AddButton(panel.transform, "MAIN MENU", -650f, new Color(0.25f, 0.27f, 0.3f), onMenu);

            _root.gameObject.SetActive(false);
        }

        static void AddSlider(Transform parent, string label, float value, float y, Color color, UnityAction<float> onChanged)
        {
            Text text = UiKit.Label(label + "Label", parent, label, 28, TextAnchor.MiddleLeft, UiPalette.TextDim);
            UiKit.Place(text.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(50f, y), new Vector2(200f, 40f));

            Slider slider = UiKit.MakeSlider(label + "Slider", parent, new Vector2(420f, 24f), color, value, onChanged);
            UiKit.Place((RectTransform)slider.transform, new Vector2(1f, 1f), new Vector2(1f, 0.5f), new Vector2(-60f, y), new Vector2(420f, 24f));
        }

        static void AddButton(Transform parent, string label, float y, Color color, UnityAction onClick)
        {
            Button b = UiKit.MakeButton(label + "Button", parent, label, new Vector2(460f, 76f), color, onClick);
            UiKit.Place((RectTransform)b.transform, new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, y), new Vector2(460f, 76f));
        }
    }
}
