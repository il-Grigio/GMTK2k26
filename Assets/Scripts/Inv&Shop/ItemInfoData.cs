using UnityEngine;

public class ItemInfoData
{
    public int Weight { get; private set; }
    public int MoneyValue { get; private set; }
    public ItemInfoData(int weightValue, int moneyValue )
    {
        this.MoneyValue = moneyValue;
        
        //TODO check if this value is fine
        this.Weight = weightValue;
    }

}
