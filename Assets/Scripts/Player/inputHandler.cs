using System;
using UnityEngine;
using UnityEngine.InputSystem; 

public class inputHandler : MonoBehaviour
{
    public Vector2 InputVector { get; private set; }
    public Vector2 MousePosition { get; private set; }

    public bool IsShooting { get; private set; }


    public Action OnShoot;
    public Action OnInteractAction;
    
    private Vector2 _moveInput;
    private Vector2 _lookInput;
    private bool _isSprinting;
    private float _verticalVelocity;
    private float _pitch;

    void Update()
    {
        if (Mouse.current != null)
        {
            MousePosition = Mouse.current.position.ReadValue();
            IsShooting = Mouse.current.leftButton.wasPressedThisFrame;
        }
    }
    private void OnMove(InputValue value)
    {
        InputVector = value.Get<Vector2>();
    }
 
    private void OnLook(InputValue value)
    {
        MousePosition = value.Get<Vector2>();
    }
 
    private void OnAttack(InputValue value)
    {
        if (value.isPressed)
        {
            Debug.Log("Attack!");
            OnShoot?.Invoke();
        }
    }
 
    private void OnInteract(InputValue value)
    {
        if (value.isPressed)
        {
            Debug.Log("Interact!");
            OnInteractAction?.Invoke();
        }
    }
 
    private void OnSprint(InputValue value)
    {
        _isSprinting = value.isPressed;
    }

}