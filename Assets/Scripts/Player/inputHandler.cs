using UnityEngine;
using UnityEngine.InputSystem; 

public class inputHandler : MonoBehaviour
{
    public Vector2 InputVector { get; private set; }
    public Vector2 MousePosition { get; private set; }

    void Update()
    {
        float h = 0f;
        float v = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                h = 1f;
            else if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                h = -1f;

            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
                v = 1f;
            else if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                v = -1f;
        }

        InputVector = new Vector2(h, v);

        if (Mouse.current != null)
        {
            MousePosition = Mouse.current.position.ReadValue();
        }
    }
}