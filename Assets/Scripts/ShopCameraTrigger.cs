using System;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem; // per Cinemachine 3.x

public class ShopCameraTrigger : MonoBehaviour
{
    [SerializeField] private CinemachineCamera shopCamera; // CinemachineVirtualCamera in v2
    [SerializeField] private int shopPriority = 15;
    [SerializeField] private int defaultPriority = 5;
    
    [SerializeField] Transform playerTargetPosition;
    [SerializeField] Transform exitPlayerTargetPosition;

    InputHandler inputHandler;
    private Transform player;
    private void Awake()
    {
        inputHandler = InputHandler.Instance;
    }

    private void OnEnable()
    {
        if (inputHandler.OnExitAction != null)
            inputHandler.OnExitAction -= ExitShop;
        inputHandler.OnExitAction += ExitShop;
    }
    private void OnDisable()
    {
        if (inputHandler.OnExitAction != null)
            inputHandler.OnExitAction -= ExitShop;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        inputHandler.CurrentState = InputHandler.State.Shop;
        shopCamera.Priority = shopPriority;
        player = other.transform;
        other.transform.position = playerTargetPosition.position;
        other.transform.rotation = playerTargetPosition.rotation;
    }
    
    public void ExitShop()
    {
        inputHandler.CurrentState = InputHandler.State.Game;
        shopCamera.Priority = defaultPriority;
        player.position = exitPlayerTargetPosition.position;
        player.rotation = exitPlayerTargetPosition.rotation;
    }

}