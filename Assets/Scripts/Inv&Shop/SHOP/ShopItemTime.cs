using UnityEngine;

public class ShopItemTime : MonoBehaviour, IShopItem
{
    [SerializeField] private int cost = 40;

    public void Buy(Shop shop)
    {
        shop.BuyTime(cost);
    }
}