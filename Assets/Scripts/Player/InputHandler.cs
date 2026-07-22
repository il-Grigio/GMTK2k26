using System;
using UnityEngine;
using UnityEngine.InputSystem; 

public class InputHandler : MonoBehaviour
{
    public Vector2 InputVector { get; private set; }
    public Vector3 MousePosition { get; private set; }

    public bool IsShooting { get; private set; }


    public Action OnShoot;
    public Action<Vector2> OnInteractAction;
    
    private Vector2 _moveInput;
    private Vector2 _lookInput;
    private bool _isSprinting;
    private float _verticalVelocity;
    private float _pitch;

    //void Update()
    //{
    //    if (Mouse.current != null)
    //    {
    //        MousePosition = Mouse.current.position.ReadValue();
    //        IsShooting = Mouse.current.leftButton.wasPressedThisFrame;
    //    }
    //}
    private void OnMove(InputValue value)
    {
        InputVector = value.Get<Vector2>();
    }
 
    private void OnLook(InputValue value)
    {
        var v2 = value.Get<Vector2>();
        if (v2.magnitude > 0.5f)
        {
            MousePosition = new Vector3(v2.x, v2.y, 0);
            //Debug.Log(MousePosition);
        }
    }
 
    private void OnAttack(InputValue value)
    {
        if (value.isPressed)
        {
            OnShoot?.Invoke();
        }
    }
 
    private void OnInteract(InputValue value)
    {
        if (value.isPressed)
        {
            Debug.Log("Interact!");
            OnInteractAction?.Invoke(MousePosition);
        }
    }
 
    private void OnSprint(InputValue value)
    {
        _isSprinting = value.isPressed;
    }

}