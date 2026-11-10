using UnityEngine;
using UnityEngine.UI;

namespace PeakRover
{
    public sealed class HillHud : MonoBehaviour
    {
        static readonly Color FuelOk = new Color(0.98f, 0.78f, 0.24f);
        static readonly Color FuelLow = new Color(0.85f, 0.27f, 0.24f);

        HillGame _game;
        Font _font;
        RectTransform _fuelFill;
        Image _fuelFillImage;
        Image _gasPedal;
        Image _brakePedal;
        Text _distanceText;
        Text _coinText;
        Text _messageText;

        float _lastFuel = -1f;
        bool _lastLow;
        int _lastDistance = -1;
        int _lastCoins = -1;
        int _messageKey = int.MinValue;
        bool _lastGas;
        bool _lastBrake;

        public void Init(HillGame game)
        {
            _game = game;
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var canvasGo = new GameObject("HillCanvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            var root = (RectTransform)canvasGo.transform;

            var fuelBg = NewImage("FuelBg", root, new Color(0f, 0f, 0f, 0.5f),
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(30f, -30f), new Vector2(380f, 40f));
            var fill = NewImage("FuelFill", fuelBg.rectTransform, FuelOk,
                Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero);
            _fuelFillImage = fill;
            _fuelFill = fill.rectTransform;
            _fuelFill.anchorMin = Vector2.zero;
            _fuelFill.anchorMax = Vector2.one;
            _fuelFill.offsetMin = Vector2.zero;
            _fuelFill.offsetMax = Vector2.zero;
            var fuelLabel = NewText("FuelLabel", fuelBg.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(380f, 40f), 28, TextAnchor.MiddleCenter);
            fuelLabel.text = "FUEL";
            fuelLabel.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

            _distanceText = NewText("Distance", root, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-30f, -24f),
                new Vector2(600f, 60f), 46, TextAnchor.UpperRight);
            _coinText = NewText("Coins", root, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -24f),
                new Vector2(500f, 60f), 40, TextAnchor.UpperCenter);
            _coinText.color = new Color(1f, 0.85f, 0.3f);
            _messageText = NewText("Message", root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(1400f, 500f), 64, TextAnchor.MiddleCenter);

            _brakePedal = NewImage("BrakePedal", root, new Color(1f, 1f, 1f, 0.2f),
                new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(30f, 30f), new Vector2(280f, 140f));
            _gasPedal = NewImage("GasPedal", root, new Color(1f, 1f, 1f, 0.2f),
                new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-30f, 30f), new Vector2(280f, 140f));
            var brakeLabel = NewText("BrakeLabel", _brakePedal.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(280f, 140f), 36, TextAnchor.MiddleCenter);
            brakeLabel.text = "BRAKE";
            var gasLabel = NewText("GasLabel", _gasPedal.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(280f, 140f), 36, TextAnchor.MiddleCenter);
            gasLabel.text = "GAS";
        }

        void Update()
        {
            CarController car = _game.Car;

            float fuel = Mathf.Clamp01(car.Fuel01);
            if (!Mathf.Approximately(fuel, _lastFuel))
            {
                _lastFuel = fuel;
                _fuelFill.anchorMax = new Vector2(fuel, 1f);
                bool low = fuel < 0.25f;
                if (low != _lastLow)
                {
                    _lastLow = low;
                    _fuelFillImage.color = low ? FuelLow : FuelOk;
                }
            }

            if (_game.DistanceM != _lastDistance)
            {
                _lastDistance = _game.DistanceM;
                _distanceText.text = _lastDistance + " m";
            }

            if (_game.Coins != _lastCoins)
            {
                _lastCoins = _game.Coins;
                _coinText.text = "COINS " + _lastCoins;
            }

            bool gas = _game.Input.Gas;
            bool brake = _game.Input.Brake;
            if (gas != _lastGas)
            {
                _lastGas = gas;
                _gasPedal.color = new Color(1f, 1f, 1f, gas ? 0.5f : 0.2f);
            }
            if (brake != _lastBrake)
            {
                _lastBrake = brake;
                _brakePedal.color = new Color(1f, 1f, 1f, brake ? 0.5f : 0.2f);
            }

            UpdateMessage();
        }

        void UpdateMessage()
        {
            int key;
            if (_game.State == HillState.GameOver) key = 2;
            else if (Time.time - _game.RunStartTime < 5f) key = 1;
            else key = 0;

            if (key == _messageKey) return;
            _messageKey = key;

            if (key == 2)
                _messageText.text = _game.Reason + "\n" + _game.DistanceM + " m   Coins " + _game.Coins
                    + "\nBest " + _game.BestM + " m\nTap or press any key to retry";
            else if (key == 1)
                _messageText.text = "Right / D = GAS     Left / A = BRAKE\nIn the air: gas leans back, brake leans forward";
            else
                _messageText.text = string.Empty;
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

        Image NewImage(string objectName, Transform parent, Color color, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            RectTransform rt = NewRect(objectName, parent, anchor, pivot, pos, size);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        Text NewText(string objectName, Transform parent, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size, int fontSize, TextAnchor align)
        {
            RectTransform rt = NewRect(objectName, parent, anchor, pivot, pos, size);
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
    }
}
