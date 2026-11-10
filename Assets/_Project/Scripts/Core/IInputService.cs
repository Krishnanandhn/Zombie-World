using UnityEngine;

namespace Project.Core
{
    public interface IInputService
    {
        Vector2 Move { get; }
        Vector2 AimStick { get; }
        Vector2 PointerPosition { get; }
        bool UsingGamepad { get; }
        bool FireHeld { get; }
        bool DashPressed { get; }
        bool ReloadPressed { get; }
        bool PausePressed { get; }
        bool NextWeaponPressed { get; }

        // Zero-based number-row slot pressed this frame, or -1.
        int SlotPressed { get; }
    }
}
