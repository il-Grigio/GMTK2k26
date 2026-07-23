using UnityEngine;

public class StealableItem : MonoBehaviour
{
    [Tooltip("Quanto è rumoroso/visibile rubare questo oggetto (0 = furtivo, 1 = plateale)")]
    [Range(0f, 1f)] public float baseNoise = 0.3f;

    public void Steal(Transform thief, float playerStealthSkill)
    {
        // stealthValue finale: più alto = meno probabilità di essere notato
        float stealthValue = Mathf.Clamp01(playerStealthSkill - baseNoise);

        SaloonManager.Instance.NotifyTheft(thief, stealthValue, transform.position);

        // qui la tua logica di raccolta oggetto (inventario, distruggi oggetto, ecc.)
        Debug.Log($"Oggetto '{name}' rubato da {thief.name}");
        Destroy(gameObject);
    }
}
