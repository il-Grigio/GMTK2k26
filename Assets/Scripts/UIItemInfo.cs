using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIItemInfo : MonoBehaviour
{
    [SerializeField] private Image mask;
    [SerializeField] private TextMeshProUGUI tfWeight;
    [SerializeField] private TextMeshProUGUI tfPrice;
    [SerializeField] private float openTime = 0.2f;
    
    float fillAmount;

    Coroutine currentCoroutine;
    private bool isOpening;
    
    public void OnMouseHover(ItemInfoData infoData, Vector3 mousePosition)
    {
        if (infoData == null)
        {
            Hide();
            return;
        }
        gameObject.SetActive(true);
        if (!isOpening)
            Show();
        transform.position = mousePosition;
        tfPrice.text = "Price: " + infoData.MoneyValue;
        tfWeight.text = "Weight: " + infoData.Weight;
    }

    void Show()
    {
        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);
        StartCoroutine(ShowMask());
    }
    
    void Hide()
    {
        isOpening = false;
        if (currentCoroutine != null)
            StopCoroutine(currentCoroutine);
        StartCoroutine(HideMask());
    }

    IEnumerator ShowMask()
    {
        isOpening = true;
        float t = fillAmount * openTime;
        while (t < openTime)
        {
            t += Time.deltaTime;
            fillAmount = t / openTime;
            mask.fillAmount = fillAmount;
            yield return null;
        }

        fillAmount = 1;
        mask.fillAmount = 1;
    }
    IEnumerator HideMask()
    {
        float t = fillAmount * openTime;
        while (t > 0)
        {
            t -= Time.deltaTime;
            fillAmount = t / openTime;
            mask.fillAmount = fillAmount;
            yield return null;
        }

        fillAmount = 0;
        mask.fillAmount = 0;
        gameObject.SetActive(false);
    }
}
