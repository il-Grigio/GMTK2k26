using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using Grigios;
using TMPro;
using FMODUnity;
public class InventorySystem : Singleton<InventorySystem>
{
    public List<ItemInfoData> inventory = new List<ItemInfoData>();

    [Header("Inventario")]
    [SerializeField] private int actualItemCount;
    [SerializeField] private int maxItemCount = 3;
    [SerializeField] private int inventoryValue; // Valore monetario dell'inventario
    [SerializeField] private int moneyHeld; // Soldi che ha effettivamente il player

    [Header("Max Bullet")]
    [SerializeField] int maxBullet = 3;
    [SerializeField] int bulletInInventory;

    [Header("UI")]
    [SerializeField] TextMeshProUGUI moneyText;
    [SerializeField] TextMeshProUGUI actualItemCountUI;
    [SerializeField] TextMeshProUGUI actualBulletUI;

    private void Start()
    {
        FixInventoryText();
        FixMoneyText();
    }
    public bool AddInventory(ItemInfoData item)
    {
        if(item.CountValue <= (maxItemCount - actualItemCount))
        {
            actualItemCount += item.CountValue;
            inventoryValue += item.MoneyValue;
            inventory.Add(item);
            FixInventoryText();
            return true;
        }
        else
        {
            return false; // Gestisci eccezione in UI
        }
    }

    public void SellInventory()
    {
        AddMoney(inventoryValue);

        inventoryValue = 0;

        actualItemCount = 0;

        inventory.Clear();

        AudioManager.Instance.PlayOneShot(FMODEventsManager.Instance.soldSound, transform.position);
        FixMoneyText();
        FixInventoryText();
    }


    public void AddMoney(int m)
    {
        moneyHeld += m;
        FixMoneyText();
    }

    public bool RemoveMoneyCheck(int m) // Controlla se effettivamente hai i soldi per comprare
    {
        return moneyHeld - m >= 0;
    }

    public void RemoveMoney(int m)
    {
        moneyHeld -= m;
        FixMoneyText();

    }

    public void HalfMoney()
    {
        moneyHeld /= 2;
        FixMoneyText();

    }

    public void MultMoney()
    {
        moneyHeld = Mathf.RoundToInt(moneyHeld * 1.5f);
        FixMoneyText();
    }

    public void AddBullet()
    {
        maxBullet++;
        bulletInInventory = maxBullet;
        AudioManager.Instance.PlayOneShot(FMODEventsManager.Instance.megIn, transform.position);
        FixInventoryText();
    }

    public void RemoveBullet()
    {
        bulletInInventory--;
        FixInventoryText();
    }

    public bool CanShoot()
    {
        if(bulletInInventory <= 0)
        {
            AudioManager.Instance.PlayOneShot(FMODEventsManager.Instance.megOut, transform.position);
            return false;
        }
        else
        {
            return true;
        }
    }

    private void FixMoneyText()
    {
        moneyText.text = moneyHeld.ToString();
    }

    private void FixInventoryText()
    {
        actualItemCountUI.text = actualItemCount.ToString() + "/" + maxItemCount.ToString();
        actualBulletUI.text = bulletInInventory.ToString() + "/" + maxBullet.ToString();
    }
}
