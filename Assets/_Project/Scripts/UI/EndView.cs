using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Project.UI
{
    internal sealed class EndView
    {
        readonly RectTransform _root;
        readonly Text _title;
        readonly Text _stats;
        readonly Text _best;

        public GameObject Root => _root.gameObject;

        public EndView(Transform canvas, UnityAction onRestart, UnityAction onMenu)
        {
            _root = UiKit.Rect("EndScreen", canvas);
            UiKit.Stretch(_root);

            Image dim = UiKit.Panel("Dim", _root, new Color(0f, 0f, 0f, 0.7f), true);
            UiKit.Stretch(dim.rectTransform);

            _title = UiKit.Label("Title", _root, "YOU DIED", 140, TextAnchor.MiddleCenter, UiPalette.Accent);
            UiKit.Place(_title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 300f), new Vector2(1600f, 180f));

            _stats = UiKit.Label("Stats", _root, string.Empty, 44, TextAnchor.UpperCenter, UiPalette.Text, FontStyle.Normal);
            UiKit.Place(_stats.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 130f), new Vector2(1200f, 230f));

            _best = UiKit.Label("Best", _root, string.Empty, 52, TextAnchor.MiddleCenter, UiPalette.Gold);
            UiKit.Place(_best.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -110f), new Vector2(1200f, 70f));

            Button restart = UiKit.MakeButton("RestartButton", _root, "TRY AGAIN", new Vector2(420f, 92f), UiPalette.Accent, onRestart);
            UiKit.Place((RectTransform)restart.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -230f), new Vector2(420f, 92f));

            Button menu = UiKit.MakeButton("MenuButton", _root, "MAIN MENU", new Vector2(420f, 72f), new Color(0.25f, 0.27f, 0.3f), onMenu);
            UiKit.Place((RectTransform)menu.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -340f), new Vector2(420f, 72f));

            _root.gameObject.SetActive(false);
        }

        public void Show(bool victory, int score, int kills, int wave, float duration, bool newBest)
        {
            _title.text = victory ? "SURVIVED" : "YOU DIED";
            _title.color = victory ? UiPalette.Green : UiPalette.Accent;
            int minutes = Mathf.FloorToInt(duration / 60f);
            int seconds = Mathf.FloorToInt(duration % 60f);
            _stats.text = $"SCORE  {score:N0}\nZOMBIES KILLED  {kills}\nWAVE REACHED  {wave}\nTIME  {minutes}:{seconds:00}";
            _best.text = newBest ? "NEW BEST SCORE!" : string.Empty;
        }
    }
}
