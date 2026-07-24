using UnityEngine;
using static Unity.Collections.AllocatorManager;

public class GamblingStation : MonoBehaviour
{
    [SerializeField] LayerMask gamblingLayer;
    [SerializeField] float interactionRadius = 4;
    private InputHandler _input;
    private Camera cam;

    private void Awake()
    {
        _input = InputHandler.Instance;
        cam = Camera.main;
    }

    protected void OnEnable()
    {
        if (_input.OnShopAction != null)
        {
            _input.OnShopAction -= GambleAction;
        }
        _input.OnShopAction += GambleAction;
    }

    protected void OnDisable()
    {
        if (_input.OnShopAction != null)
        {
            _input.OnShopAction -= GambleAction;
        }
    }

    void GambleAction(Vector2 mousePos)
    {
        Ray ray = cam.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance: 300f, gamblingLayer))
        {
            
        }

    }
}
