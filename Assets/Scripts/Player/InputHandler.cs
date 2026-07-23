using System;
using Grigios;
using UnityEngine;
using UnityEngine.InputSystem; 

public class InputHandler : Singleton<InputHandler>
{
    public enum State
    {
        Menu,
        Game,
        Shop
    }
    public Vector2 InputVector { get; private set; }
    public Vector3 MousePosition { get; private set; }

    public Action OnShoot;
    public Action<Vector2> OnInteractAction;
    public Action<Vector2> OnShopAction;
    public Action OnExitAction;
    
    private Vector3 internalMousePosition;
    private Vector2 _moveInput;
    private Vector2 _lookInput;
    private bool _isSprinting;
    private float _verticalVelocity;
    private float _pitch;

    private State _currentState = State.Game;
    public State CurrentState
    {
        get => _currentState;
        set
        {
            InputVector = Vector2.zero;
            MousePosition = Vector3.zero;
            _isSprinting = false;
            _currentState = value;
        }
    }
    private void OnMove(InputValue value)
    {
        if (CurrentState == State.Game)
            InputVector = value.Get<Vector2>();
    }
 
    private void OnLook(InputValue value)
    {
        var v2 = value.Get<Vector2>();
        internalMousePosition = new Vector3(v2.x, v2.y, 0);
        if (CurrentState != State.Game) return;
        if (v2.magnitude > 0.5f)
        {
            MousePosition = internalMousePosition;
            //Debug.Log(MousePosition);
        }
    }
 
    private void OnAttack(InputValue value)
    {
        if (value.isPressed)
        {
            if (CurrentState == State.Game) 
                OnShoot?.Invoke();
            else if (CurrentState == State.Shop)
                OnShopAction?.Invoke(internalMousePosition);
        }
    }

    private void OnExit(InputValue value)
    {
        if (value.isPressed)
        {
            if (CurrentState == State.Shop)
                OnExitAction?.Invoke();
        }
    }
 
    private void OnInteract(InputValue value)
    {
        if (value.isPressed)
        {
            if (CurrentState == State.Shop)
                OnShopAction?.Invoke(internalMousePosition);
            else if (CurrentState == State.Game)
            {
                OnInteractAction?.Invoke(internalMousePosition);
            }
        }
    }
 
    private void OnSprint(InputValue value)
    {
        _isSprinting = value.isPressed;
    }

}