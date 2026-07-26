using UnityEngine;

[RequireComponent(typeof(Renderer))]
[ExecuteAlways]
public class ItemInfoComponent : MonoBehaviour
{
    private static readonly int IsSceneLayerID = Shader.PropertyToID("_IsSceneLayer");
    [SerializeField] private LayerMask sceneLayerMask;

    [SerializeField] int countValue;

    [SerializeField] int moneyValue;

    private Renderer _renderer;
    private MaterialPropertyBlock _mpb;

    private void OnEnable()
    {
        Apply();
    }

    private void OnValidate()
    {
        Apply();
    }

    public int GetPointValue()
    {
        Debug.Log("moneyvalue: " + moneyValue + " countvalue: " + countValue + " moneyvalue: " + moneyValue);
        return moneyValue * countValue ;
    }
    public ItemInfoData GetInfo()
    {
        return new ItemInfoData(countValue, moneyValue);
    }

    public void Apply()
    {
        if (_renderer == null) _renderer = GetComponent<Renderer>();
        if (_mpb == null) _mpb = new MaterialPropertyBlock();

        bool isSceneLayer = (sceneLayerMask.value & (1 << gameObject.layer)) != 0;

        _renderer.GetPropertyBlock(_mpb);
        _mpb.SetFloat(IsSceneLayerID, isSceneLayer ? 1f : 0f);
        _renderer.SetPropertyBlock(_mpb);
    }
}