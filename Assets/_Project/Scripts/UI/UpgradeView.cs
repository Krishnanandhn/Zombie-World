using System;
using Project.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Project.UI
{
    internal sealed class UpgradeView
    {
        const int MaxCards = 4;
        const float CardWidth = 400f;
        const float CardHeight = 500f;

        sealed class Card
        {
            public GameObject Root;
            public Image Header;
            public Text Rarity;
            public Text Name;
            public Text Description;
            public Text Key;
            public string UpgradeId;
        }

        readonly RectTransform _root;
        readonly Text _title;
        readonly Card[] _cards = new Card[MaxCards];

        public GameObject Root => _root.gameObject;

        public UpgradeView(Transform canvas, Action<string> onPick)
        {
            _root = UiKit.Rect("Upgrades", canvas);
            UiKit.Stretch(_root);

            Image dim = UiKit.Panel("Dim", _root, UiPalette.Dim, true);
            UiKit.Stretch(dim.rectTransform);

            _title = UiKit.Label("Title", _root, "WAVE CLEARED", 84, TextAnchor.MiddleCenter, UiPalette.Gold);
            UiKit.Place(_title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 380f), new Vector2(1600f, 110f));

            Text sub = UiKit.Label("Subtitle", _root, "Choose an upgrade   (press 1 / 2 / 3)", 36, TextAnchor.MiddleCenter, UiPalette.Text, FontStyle.Normal);
            UiKit.Place(sub.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 300f), new Vector2(1600f, 50f));

            RectTransform row = UiKit.Rect("Cards", _root);
            UiKit.Place(row, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -30f), new Vector2(1800f, CardHeight));
            var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 40f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            for (int i = 0; i < MaxCards; i++) _cards[i] = BuildCard(row, i, onPick);

            _root.gameObject.SetActive(false);
        }

        Card BuildCard(Transform parent, int index, Action<string> onPick)
        {
            var card = new Card();
            Button button = UiKit.MakeButton("Card" + index, parent, string.Empty, new Vector2(CardWidth, CardHeight), new Color(0.1f, 0.11f, 0.14f, 0.96f), null);
            card.Root = button.gameObject;
            Transform t = button.transform;
            Destroy(t.Find("Label").gameObject);

            card.Header = UiKit.Panel("Header", t, UiPalette.Common);
            UiKit.Place(card.Header.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(CardWidth, 14f));

            card.Rarity = UiKit.Label("Rarity", t, "COMMON", 26, TextAnchor.MiddleCenter, UiPalette.Common);
            UiKit.Place(card.Rarity.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(CardWidth - 40f, 36f));

            card.Name = UiKit.Label("Name", t, "Upgrade", 44, TextAnchor.UpperCenter, UiPalette.Text);
            UiKit.Place(card.Name.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -110f), new Vector2(CardWidth - 50f, 120f));

            card.Description = UiKit.Label("Description", t, string.Empty, 32, TextAnchor.UpperCenter, UiPalette.TextDim, FontStyle.Normal);
            UiKit.Place(card.Description.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -250f), new Vector2(CardWidth - 60f, 170f));

            card.Key = UiKit.Label("Key", t, $"[ {index + 1} ]", 40, TextAnchor.MiddleCenter, UiPalette.Gold);
            UiKit.Place(card.Key.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 24f), new Vector2(CardWidth, 54f));

            button.onClick.AddListener(() =>
            {
                if (!string.IsNullOrEmpty(card.UpgradeId)) onPick(card.UpgradeId);
            });
            return card;
        }

        static void Destroy(GameObject go) => UnityEngine.Object.Destroy(go);

        public void Show(UpgradeDefinition[] options, int wave)
        {
            _title.text = $"WAVE {wave} CLEARED";
            for (int i = 0; i < _cards.Length; i++)
            {
                Card card = _cards[i];
                bool active = options != null && i < options.Length;
                card.Root.SetActive(active);
                if (!active) continue;

                UpgradeDefinition u = options[i];
                Color c = ColorFor(u.Rarity);
                card.UpgradeId = u.Id;
                card.Header.color = c;
                card.Rarity.color = c;
                card.Rarity.text = u.Rarity.ToString().ToUpperInvariant();
                card.Name.text = u.DisplayName;
                card.Description.text = u.Description;
            }
        }

        static Color ColorFor(UpgradeRarity rarity)
        {
            switch (rarity)
            {
                case UpgradeRarity.Epic: return UiPalette.Epic;
                case UpgradeRarity.Rare: return UiPalette.Rare;
                default: return UiPalette.Common;
            }
        }
    }
}
