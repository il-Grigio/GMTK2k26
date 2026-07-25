using UnityEngine;

// Da mettere sullo stesso GameObject del Renderer (o su un parent, con
// GetComponentsInChildren) che usa "Custom/ToonShader4Level".
//
// Se il layer del GameObject e' "OBJECT_SCENE", imposta la property
// _IsSceneLayer a 1 tramite MaterialPropertyBlock: questo forza l'outline
// a bianco nello shader SENZA modificare/duplicare il materiale condiviso.
[RequireComponent(typeof(Renderer))]
[ExecuteAlways]
public class SceneLayerOutlineOverride : MonoBehaviour
{
    private static readonly int IsSceneLayerID = Shader.PropertyToID("_IsSceneLayer");

    // LayerMask nell'Inspector: seleziona il layer "OBJECT_SCENE" con la
    // tendina (supporta anche piu' layer contemporaneamente se un giorno
    // serve, es. OBJECT_SCENE + un altro layer "bianco").
    [SerializeField] private LayerMask sceneLayerMask;

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

    // Chiamalo se cambi il layer a runtime (gameObject.layer = ...)
    public void Apply()
    {
        if (_renderer == null) _renderer = GetComponent<Renderer>();
        if (_mpb == null) _mpb = new MaterialPropertyBlock();

        // Una LayerMask e' una bitmask: il bit N e' acceso se il layer N
        // e' incluso nella maschera. Per sapere se il layer del
        // GameObject rientra nella maschera, si sposta 1 di "gameObject.layer"
        // posizioni e si fa un AND bit a bit con il valore della maschera.
        bool isSceneLayer = (sceneLayerMask.value & (1 << gameObject.layer)) != 0;

        _renderer.GetPropertyBlock(_mpb);
        _mpb.SetFloat(IsSceneLayerID, isSceneLayer ? 1f : 0f);
        _renderer.SetPropertyBlock(_mpb);
    }
}