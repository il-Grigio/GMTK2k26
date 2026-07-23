using UnityEngine;

public class ShopItemHorse : MonoBehaviour, IShopItem
{
    [SerializeField] private int cost = 50;

    public void Buy(Shop shop)
    {
        shop.BuyHorse(cost);
    }
}