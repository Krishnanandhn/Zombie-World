using UnityEngine;
using UnityEngine.InputSystem;

namespace PeakRover
{
    // Keyboard, gamepad, mouse or touch: right half of the screen is gas, left half is brake.
    public sealed class HillInput
    {
        int _polledFrame = -1;
        bool _gas;
        bool _brake;
        bool _anyPressed;

        public bool Gas { get { Poll(); return _gas; } }
        public bool Brake { get { Poll(); return _brake; } }
        public bool AnyPressed { get { Poll(); return _anyPressed; } }

        void Poll()
        {
            if (_polledFrame == Time.frameCount) return;
            _polledFrame = Time.frameCount;

            bool gas = false;
            bool brake = false;
            _anyPressed = false;

            Keyboard kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed || kb.wKey.isPressed || kb.upArrowKey.isPressed) gas = true;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed || kb.sKey.isPressed || kb.downArrowKey.isPressed) brake = true;
                if (kb.anyKey.wasPressedThisFrame) _anyPressed = true;
            }

            Gamepad gp = Gamepad.current;
            if (gp != null)
            {
                if (gp.rightTrigger.ReadValue() > 0.1f || gp.buttonSouth.isPressed || gp.leftStick.x.ReadValue() > 0.4f) gas = true;
                if (gp.leftTrigger.ReadValue() > 0.1f || gp.buttonWest.isPressed || gp.leftStick.x.ReadValue() < -0.4f) brake = true;
                if (gp.buttonSouth.wasPressedThisFrame) _anyPressed = true;
            }

            float half = Screen.width * 0.5f;
            Touchscreen ts = Touchscreen.current;
            if (ts != null)
            {
                var touches = ts.touches;
                for (int i = 0; i < touches.Count; i++)
                {
                    var t = touches[i];
                    if (t.press.wasPressedThisFrame) _anyPressed = true;
                    if (!t.press.isPressed) continue;
                    if (t.position.ReadValue().x >= half) gas = true;
                    else brake = true;
                }
            }

            Mouse mouse = Mouse.current;
            if (mouse != null)
            {
                if (mouse.leftButton.wasPressedThisFrame) _anyPressed = true;
                if (mouse.leftButton.isPressed)
                {
                    if (mouse.position.ReadValue().x >= half) gas = true;
                    else brake = true;
                }
            }

            _gas = gas;
            _brake = brake;
        }
    }
}
