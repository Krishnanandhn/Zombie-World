using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Core
{
    // Input System actions built in code so bindings live in one place and can be rebound at runtime.
    public sealed class InputService : IInputService, System.IDisposable
    {
        const float GamepadDeadzone = 0.2f;

        readonly InputActionMap _map = new InputActionMap("Gameplay");
        readonly InputAction _move;
        readonly InputAction _aim;
        readonly InputAction _fire;
        readonly InputAction _dash;
        readonly InputAction _reload;
        readonly InputAction _pause;
        readonly InputAction _nextWeapon;

        int _polledFrame = -1;
        bool _usingGamepad;

        public InputService()
        {
            _move = _map.AddAction("Move", InputActionType.Value);
            _move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            _move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
            _move.AddBinding("<Gamepad>/leftStick");

            _aim = _map.AddAction("Aim", InputActionType.Value, "<Gamepad>/rightStick");

            _fire = _map.AddAction("Fire", InputActionType.Button);
            _fire.AddBinding("<Mouse>/leftButton");
            _fire.AddBinding("<Gamepad>/rightTrigger");

            _dash = _map.AddAction("Dash", InputActionType.Button);
            _dash.AddBinding("<Keyboard>/leftShift");
            _dash.AddBinding("<Keyboard>/space");
            _dash.AddBinding("<Gamepad>/buttonEast");

            _reload = _map.AddAction("Reload", InputActionType.Button);
            _reload.AddBinding("<Keyboard>/r");
            _reload.AddBinding("<Gamepad>/buttonWest");

            _pause = _map.AddAction("Pause", InputActionType.Button);
            _pause.AddBinding("<Keyboard>/escape");
            _pause.AddBinding("<Keyboard>/p");
            _pause.AddBinding("<Gamepad>/start");

            _nextWeapon = _map.AddAction("NextWeapon", InputActionType.Button);
            _nextWeapon.AddBinding("<Keyboard>/q");
            _nextWeapon.AddBinding("<Gamepad>/rightShoulder");

            _map.Enable();
        }

        public Vector2 Move => Vector2.ClampMagnitude(_move.ReadValue<Vector2>(), 1f);

        public Vector2 AimStick => _aim.ReadValue<Vector2>();

        public Vector2 PointerPosition => Pointer.current != null ? Pointer.current.position.ReadValue() : Vector2.zero;

        public bool UsingGamepad
        {
            get
            {
                Poll();
                return _usingGamepad;
            }
        }

        public bool FireHeld => _fire.IsPressed();

        public bool DashPressed => _dash.WasPressedThisFrame();

        public bool ReloadPressed => _reload.WasPressedThisFrame();

        public bool PausePressed => _pause.WasPressedThisFrame();

        public bool NextWeaponPressed => _nextWeapon.WasPressedThisFrame();

        public int SlotPressed
        {
            get
            {
                Keyboard kb = Keyboard.current;
                if (kb == null) return -1;
                if (kb.digit1Key.wasPressedThisFrame) return 0;
                if (kb.digit2Key.wasPressedThisFrame) return 1;
                if (kb.digit3Key.wasPressedThisFrame) return 2;
                if (kb.digit4Key.wasPressedThisFrame) return 3;
                if (kb.digit5Key.wasPressedThisFrame) return 4;
                return -1;
            }
        }

        void Poll()
        {
            if (_polledFrame == Time.frameCount) return;
            _polledFrame = Time.frameCount;

            Gamepad gp = Gamepad.current;
            if (gp != null && (gp.rightStick.ReadValue().magnitude > GamepadDeadzone
                               || gp.leftStick.ReadValue().magnitude > GamepadDeadzone
                               || gp.rightTrigger.isPressed))
            {
                _usingGamepad = true;
                return;
            }

            Mouse mouse = Mouse.current;
            if (mouse != null && mouse.delta.ReadValue().sqrMagnitude > 0.01f) _usingGamepad = false;
        }

        public void Dispose()
        {
            _map.Disable();
            _map.Dispose();
        }
    }
}
