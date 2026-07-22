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

    private void OnEnable()
    {
        _input.OnInteractAction -= ItemToInventory;
        _input.OnInteractAction += ItemToInventory;
    }

    private void OnDisable()
    {
        _input.OnInteractAction -= ItemToInventory;
    }

    private void ItemToInventory(Vector2 mousePos)
    {
        Ray ray = cam.ScreenPointToRay(_input.MousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance: 300f))
        {
            var target = hitInfo.collider.gameObject;
            if (((1 << target.layer) & itemLayerMask) != 0)
            {
                var itemInfo = target.GetComponent<Item_Infos>();
                if (itemInfo != null)
                {
                    Inventory_System.Instance.AddInventory(itemInfo);
                }
            }
        }
    }

}
