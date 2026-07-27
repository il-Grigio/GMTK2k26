using Grigios;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] UIItemInfo uiItemInfo;

    private void Awake()
    {
        uiItemInfo.gameObject.SetActive(false);
    }
    public void ShowItemInfo(ItemInfoData infoData, Vector3 mousePosition)
    {
        uiItemInfo.OnMouseHover(infoData, mousePosition);
    }
}
