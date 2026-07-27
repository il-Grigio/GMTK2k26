using UnityEngine;

// Script dato come potere al Player.
// Trova item attorno al Player.
// Seleziona quell'item e chiama l'Inventory_Sistem
public class PlayerStealItem : PlayerInteraction
{
    [Range(0f, 1f)] public float stealthSkill = 0.5f;
    [SerializeField] LayerMask itemLayerMask;
    [SerializeField] LayerMask villagerLayerMask;
    [SerializeField] float interactionRadius = 2f;
    [SerializeField] float suspicionCheckRadius = 20f; // deve coprire il raggio di vista max dei villici

    [SerializeField] private int increaseTimerMULT = 1;

    [Header("Points for stealing")]
    [SerializeField] private int pointsForStealing = 100;


    private StealableItem currentItem;
    protected override void OnEnable()
    {
        if (_input.OnInteractAction != null)
        {
            _input.OnInteractAction -= ItemToInventory;
        }
        _input.OnInteractAction += ItemToInventory;
    }

    protected override void OnDisable()
    {
        if (_input.OnInteractAction != null)
        {
            _input.OnInteractAction -= ItemToInventory;
        }
    }

    private void ItemToInventory(Vector2 mousePos)
    {
        if (currentItem == null) return;
        var itemInfo = currentItem.ItemInfo;
        if (itemInfo != null && InventorySystem.Instance.AddInventory(itemInfo.GetInfo()))
        {
            float currentStealth = stealthSkill;
            currentItem.Steal(transform, Mathf.Clamp01(currentStealth));
            PointSystem.Instance.AddScore(itemInfo.GetPointValue());
        }
    }
    private void Update()
    {
        Ray ray = cam.ScreenPointToRay(_input.MousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance: 300f, itemLayerMask))
        {
            var target = hitInfo.collider.gameObject;
            if ((transform.position - target.transform.position).sqrMagnitude > interactionRadius * interactionRadius) return;
            if (target.TryGetComponent(out StealableItem item))
            {
                currentItem = item;
                GameManager.Instance.ShowItemInfo(currentItem.ItemInfo.GetInfo(), _input.MousePosition);
            }
        }
        else if (currentItem != null)
        {
            currentItem = null;
            GameManager.Instance.ShowItemInfo(null, _input.MousePosition);
        }
    }
    
}