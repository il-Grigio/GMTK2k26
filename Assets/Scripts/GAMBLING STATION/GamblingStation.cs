using UnityEditor.Rendering;
using UnityEngine;
using static Unity.Collections.AllocatorManager;

public class GamblingStation : MonoBehaviour
{
    [SerializeField] LayerMask gamblingLayer;
    [SerializeField] float interactionRadius = 4;
    private InputHandler _input;
    private Camera cam;
    [SerializeField] CameraShakeSimpleScript cameraShakeSimpleScript;

    private void Awake()
    {
        _input = InputHandler.Instance;
        cam = Camera.main;
    }

    protected void OnEnable()
    {
        if (_input.OnInteractAction != null)
        {
            _input.OnInteractAction -= GambleAction;
        }
        _input.OnInteractAction += GambleAction;
    }

    protected void OnDisable()
    {
        if (_input.OnInteractAction != null)
        {
            _input.OnInteractAction -= GambleAction;
        }
    }

    void GambleAction(Vector2 mousePos)
    {
        Ray ray = cam.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance: 300f, gamblingLayer))
        {
            if(Random.value < 0.5f)
            {
                InventorySystem.Instance.HalfMoney();
 
                if (cameraShakeSimpleScript && cameraShakeSimpleScript != null) cameraShakeSimpleScript.ShakeCamera();
            }
            else
            {
                InventorySystem.Instance.MultMoney();
            }
        }

    }
}
