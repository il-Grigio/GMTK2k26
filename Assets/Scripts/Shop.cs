using System;
using UnityEngine;

public class Shop : MonoBehaviour
{
    [SerializeField] LayerMask shopLayer;
    [SerializeField] float interactionRadius = 4;
    [SerializeField] ShopCameraTrigger shopTrigger;
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
            _input.OnShopAction -= Click;
        }
        _input.OnShopAction += Click;
    }

    protected void OnDisable()
    {
        if (_input.OnShopAction != null)
        {
            _input.OnShopAction -= Click;
        }
    }

    void Click(Vector2 mousePos)
    {
        Ray ray = cam.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance: 300f, shopLayer))
        {
            if (hitInfo.collider.TryGetComponent<IShopItem>(out var item))
            {
                item.Buy(this);
            }
        }
        else
        {
            shopTrigger.ExitShop();
        }
    }
    public void BuyHorse(int m)
    {
        if (HasEnoughMoney(m))
        {
            InventorySystem.Instance.RemoveMoney(m);
            
            PointSystem.Instance.cavalliRaccolti += 1;
            // Instantiate(cavallo, cavalloSpawnPoint.position, Quaternion.identity);
        }

    }

    public void BuyBullet(int m)
    {
        if (HasEnoughMoney(m))
        {
            InventorySystem.Instance.RemoveMoney(m);
        }
    }

    public void BuyDynamite(int m)
    {
        if (HasEnoughMoney(m))
        {
            InventorySystem.Instance.RemoveMoney(m);
        }
    }

    public bool HasEnoughMoney(int m)
    {
        return InventorySystem.Instance.RemoveMoneyCheck(m);
    }
}
