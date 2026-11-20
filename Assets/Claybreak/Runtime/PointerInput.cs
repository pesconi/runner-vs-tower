using UnityEngine.InputSystem;

namespace Claybreak
{
    /// <summary>One finger (or the mouse in the Editor): pressed this frame and held. No allocations.</summary>
    public static class PointerInput
    {
        public static bool IsHeld
        {
            get
            {
                if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
                {
                    return true;
                }

                return Mouse.current != null && Mouse.current.leftButton.isPressed;
            }
        }

        public static bool WasPressedThisFrame
        {
            get
            {
                if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
                {
                    return true;
                }

                return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
            }
        }
    }
}
