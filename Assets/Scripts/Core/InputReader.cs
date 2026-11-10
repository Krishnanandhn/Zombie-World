using UnityEngine;
using UnityEngine.InputSystem;

namespace ZombieWorld
{
    // Keyboard / gamepad / floating touch-or-mouse joystick, polled once per frame on demand.
    public sealed class InputReader
    {
        int _polledFrame = -1;
        Vector2 _move;
        bool _dragging;
        Vector2 _origin;
        Vector2 _current;
        bool _anyPressed;

        public static float JoystickRadiusPx => Mathf.Min(Screen.width, Screen.height) * 0.12f;

        public Vector2 Move { get { Poll(); return _move; } }
        public bool JoystickActive { get { Poll(); return _dragging; } }
        public Vector2 JoystickOrigin { get { Poll(); return _origin; } }
        public Vector2 JoystickKnob
        {
            get
            {
                Poll();
                return _origin + Vector2.ClampMagnitude(_current - _origin, JoystickRadiusPx);
            }
        }
        public bool AnyPressed { get { Poll(); return _anyPressed; } }

        void Poll()
        {
            if (_polledFrame == Time.frameCount) return;
            _polledFrame = Time.frameCount;

            Vector2 move = Vector2.zero;
            _anyPressed = false;

            Keyboard kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) move.y += 1f;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) move.y -= 1f;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) move.x -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) move.x += 1f;
                if (kb.anyKey.wasPressedThisFrame) _anyPressed = true;
            }

            Gamepad gp = Gamepad.current;
            if (gp != null)
            {
                move += gp.leftStick.ReadValue();
                if (gp.buttonSouth.wasPressedThisFrame) _anyPressed = true;
            }

            Pointer ptr = Pointer.current;
            if (ptr != null)
            {
                Vector2 pos = ptr.position.ReadValue();
                if (ptr.press.wasPressedThisFrame)
                {
                    _dragging = true;
                    _origin = pos;
                    _anyPressed = true;
                }

                if (_dragging)
                {
                    if (ptr.press.isPressed)
                    {
                        _current = pos;
                        move += Vector2.ClampMagnitude((pos - _origin) / JoystickRadiusPx, 1f);
                    }
                    else
                    {
                        _dragging = false;
                    }
                }
            }

            move = Vector2.ClampMagnitude(move, 1f);
            _move = move.sqrMagnitude < 0.01f ? Vector2.zero : move;
        }
    }
}
