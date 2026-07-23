using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using Grigios;
public class InventorySystem : Singleton<InventorySystem>
{
    public List<ItemInfoData> inventory = new List<ItemInfoData>();

    [SerializeField] private int actualWeight;
    [SerializeField] private int maxWeight = 200;
    [SerializeField] private int actualItemCount;
    [SerializeField] private int maxItemCount = 3;
    [SerializeField] private int inventoryValue; // Valore monetario dell'inventario
    [SerializeField] private int moneyHeld; // Soldi che ha effettivamente il player
    public bool AddInventory(ItemInfoData item)
    {
        if(item.Weight <= (maxWeight - actualWeight) && item.CountValue <= (maxItemCount - actualItemCount))
        {
            actualWeight += item.Weight;
            actualItemCount += item.CountValue;
            inventoryValue += item.MoneyValue;
            inventory.Add(item);
            return true;
        }
        else
        {
            if(item.Weight > (maxWeight - actualWeight) && item.CountValue <= (maxItemCount - actualItemCount))
            {
                return false;
            }
            else if(item.CountValue > (maxItemCount - actualItemCount) && item.Weight <= (maxWeight - actualWeight))
            {
                return false;
            }
            else
            {
                return false;
            }
        }
    }

    public void SellInventory()
    {
        AddMoney(inventoryValue);

        inventoryValue = 0;

        actualWeight = 0;

        actualItemCount = 0;

        inventory.Clear();
    }


    public void AddMoney(int m)
    {
        moneyHeld += m;
    }

    public bool RemoveMoneyCheck(int m) // Controlla se effettivamente hai i soldi per comprare
    {
        return moneyHeld - m >= 0;
    }

    public void RemoveMoney(int m)
    {
        moneyHeld -= m;
    }
}
