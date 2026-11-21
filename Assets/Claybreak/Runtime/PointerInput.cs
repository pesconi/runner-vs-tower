using UnityEngine;
using UnityEngine.InputSystem;

namespace Claybreak
{
    /// <summary>One finger (or the mouse in the Editor): pressed this frame, held, and where. No allocations.</summary>
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

        /// <summary>
        /// The held pointer's screen position and whether it went down this frame; false when nothing is held (the
        /// finger wins over the mouse).
        /// </summary>
        public static bool TryGetHeld(out Vector2 position, out bool pressedThisFrame)
        {
            Touchscreen touch = Touchscreen.current;
            if (touch != null && touch.primaryTouch.press.isPressed)
            {
                position = touch.primaryTouch.position.ReadValue();
                pressedThisFrame = touch.primaryTouch.press.wasPressedThisFrame;
                return true;
            }

            Mouse mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.isPressed)
            {
                position = mouse.position.ReadValue();
                pressedThisFrame = mouse.leftButton.wasPressedThisFrame;
                return true;
            }

            position = default;
            pressedThisFrame = false;
            return false;
        }
    }
}
