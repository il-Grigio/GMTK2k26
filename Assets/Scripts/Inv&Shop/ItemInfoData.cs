using UnityEngine;

public class ItemInfoData
{
    public int Weight { get; private set; }
    public int CountValue { get; private set; }
    public int MoneyValue { get; private set; }
    public ItemInfoData(int countValue, int moneyValue )
    {
        this.CountValue = countValue;
        this.MoneyValue = moneyValue;
    }

}
