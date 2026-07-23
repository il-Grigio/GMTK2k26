using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem; // per Cinemachine 3.x

public class ShopCameraTrigger : MonoBehaviour
{
    [SerializeField] private CinemachineCamera shopCamera; // CinemachineVirtualCamera in v2
    [SerializeField] private PlayerInput playerInput;       // o il tuo script di movimento
    [SerializeField] private int shopPriority = 15;
    [SerializeField] private int defaultPriority = 5;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        shopCamera.Priority = shopPriority;
        SetPlayerInput(false);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        shopCamera.Priority = defaultPriority;
        SetPlayerInput(true);
    }

    private void SetPlayerInput(bool enabled)
    {
        // Se usi il nuovo Input System con PlayerInput component
        if (playerInput != null)
            playerInput.enabled = enabled;

        // In alternativa, se hai un tuo PlayerController:
        // playerController.enabled = enabled;
    }
}