using UnityEngine;
using UnityEngine.UI;

namespace Project.UI
{
    // Pooled floating damage text projected from world space onto the overlay canvas.
    internal sealed class DamageNumbers
    {
        const int PoolSize = 40;
        const float Lifetime = 0.8f;
        const float RiseSpeed = 1.8f;

        sealed class Entry
        {
            public Text Text;
            public Vector3 World;
            public float Age;
            public bool Active;
        }

        readonly Entry[] _entries = new Entry[PoolSize];
        readonly Canvas _canvas;
        Camera _camera;
        int _next;

        public DamageNumbers(Transform parent, Canvas canvas)
        {
            _canvas = canvas;
            for (int i = 0; i < PoolSize; i++)
            {
                Text text = UiKit.Label("Damage" + i, parent, string.Empty, 34, TextAnchor.MiddleCenter, Color.white);
                text.rectTransform.sizeDelta = new Vector2(160f, 50f);
                text.rectTransform.anchorMin = Vector2.zero;
                text.rectTransform.anchorMax = Vector2.zero;
                text.gameObject.SetActive(false);
                _entries[i] = new Entry { Text = text };
            }
        }

        public void Spawn(Vector3 world, float amount, bool crit)
        {
            Entry e = _entries[_next];
            _next = (_next + 1) % PoolSize;

            e.World = world + new Vector3(Random.Range(-0.3f, 0.3f), 0.5f, Random.Range(-0.3f, 0.3f));
            e.Age = 0f;
            e.Active = true;
            e.Text.text = Mathf.CeilToInt(amount).ToString();
            e.Text.color = crit ? UiPalette.Gold : Color.white;
            e.Text.fontSize = crit ? 52 : 34;
            e.Text.gameObject.SetActive(true);
        }

        public void Clear()
        {
            for (int i = 0; i < _entries.Length; i++)
            {
                _entries[i].Active = false;
                _entries[i].Text.gameObject.SetActive(false);
            }
        }

        public void Tick(float dt)
        {
            if (_camera == null) _camera = Camera.main;
            if (_camera == null) return;

            for (int i = 0; i < _entries.Length; i++)
            {
                Entry e = _entries[i];
                if (!e.Active) continue;

                e.Age += dt;
                if (e.Age >= Lifetime)
                {
                    e.Active = false;
                    e.Text.gameObject.SetActive(false);
                    continue;
                }

                Vector3 screen = _camera.WorldToScreenPoint(e.World + Vector3.up * (e.Age * RiseSpeed));
                if (screen.z <= 0f)
                {
                    e.Text.gameObject.SetActive(false);
                    continue;
                }

                e.Text.gameObject.SetActive(true);
                e.Text.rectTransform.anchoredPosition = new Vector2(screen.x, screen.y) / _canvas.scaleFactor;
                Color c = e.Text.color;
                c.a = 1f - Mathf.Clamp01((e.Age - Lifetime * 0.5f) / (Lifetime * 0.5f));
                e.Text.color = c;
            }
        }
    }
}
