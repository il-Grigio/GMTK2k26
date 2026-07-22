using Unity.VisualScripting;
using UnityEngine;

// Script dato come potere al Player.
// Trova item attorno al Player.
// Seleziona quell'item e chiama l'Inventory_Sistem
public class Item_Hold : MonoBehaviour
{
    private InputHandler _input;
    private Camera cam;

    [SerializeField] LayerMask itemLayerMask;

    private void Awake()
    {
        _input = GetComponent<InputHandler>();
    }

    private void Start()
    {
        cam = Camera.main;
    }

    private void OnEnable()
    {
        if(_input.OnInteractAction != null)
        {
            _input.OnInteractAction -= ItemToInventory;
        }
        _input.OnInteractAction += ItemToInventory;
    }

    private void OnDisable()
    {
        if (_input.OnInteractAction != null)
        {
            _input.OnInteractAction -= ItemToInventory;
        }
    }

    private void ItemToInventory(Vector2 mousePos)
    {
        Ray ray = cam.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance: 300f, itemLayerMask))
        {
            var target = hitInfo.collider.gameObject;
            
            var itemInfo = target.GetComponent<ItemInfoComponent>().GetInfo();
            if (itemInfo != null)
            {
                if (Inventory_System.Instance.AddInventory(itemInfo))
                {
                    target.gameObject.SetActive(false);
                }
            }
            
        }
    }

}
