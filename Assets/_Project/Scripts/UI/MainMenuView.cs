using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Project.UI
{
    internal sealed class MainMenuView
    {
        readonly RectTransform _root;
        readonly Text _bestText;

        public GameObject Root => _root.gameObject;

        public MainMenuView(Transform canvas, UnityAction onStart, UnityAction onQuit)
        {
            _root = UiKit.Rect("MainMenu", canvas);
            UiKit.Stretch(_root);

            Image dim = UiKit.Panel("Dim", _root, new Color(0.02f, 0.02f, 0.03f, 0.55f), true);
            UiKit.Stretch(dim.rectTransform);

            Text title = UiKit.Label("Title", _root, "ZOMBIE SURVIVAL", 128, TextAnchor.MiddleCenter, UiPalette.Accent);
            UiKit.Place(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 280f), new Vector2(1600f, 160f));

            Text subtitle = UiKit.Label("Subtitle", _root, "Hold the yard. Upgrade between waves. Put down the Abomination.", 36,
                TextAnchor.MiddleCenter, UiPalette.Text, FontStyle.Normal);
            UiKit.Place(subtitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 190f), new Vector2(1500f, 50f));

            Button start = UiKit.MakeButton("StartButton", _root, "START RUN", new Vector2(420f, 92f), UiPalette.Accent, onStart);
            UiKit.Place((RectTransform)start.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 40f), new Vector2(420f, 92f));

            Button quit = UiKit.MakeButton("QuitButton", _root, "QUIT", new Vector2(420f, 72f), new Color(0.25f, 0.27f, 0.3f), onQuit);
            UiKit.Place((RectTransform)quit.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -70f), new Vector2(420f, 72f));

            _bestText = UiKit.Label("Best", _root, string.Empty, 30, TextAnchor.MiddleCenter, UiPalette.Gold);
            UiKit.Place(_bestText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -170f), new Vector2(1200f, 44f));

            Text controls = UiKit.Label("Controls", _root,
                "WASD  Move      Mouse  Aim      Hold LMB  Shoot      R  Reload      SHIFT / SPACE  Dash      1-4 / Q  Weapons      ESC  Pause\n" +
                "Gamepad:  Left stick move   Right stick aim   RT shoot   B dash   X reload   RB switch",
                24, TextAnchor.MiddleCenter, UiPalette.TextDim, FontStyle.Normal);
            UiKit.Place(controls.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 40f), new Vector2(1700f, 90f));

            _root.gameObject.SetActive(false);
        }

        public void SetBest(int bestScore, int bestWave, int victories)
        {
            _bestText.text = bestScore > 0 ? $"BEST SCORE  {bestScore:N0}     BEST WAVE  {bestWave}     VICTORIES  {victories}" : string.Empty;
        }
    }
}
