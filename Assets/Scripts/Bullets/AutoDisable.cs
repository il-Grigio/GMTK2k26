using UnityEngine;

public class AutoDisable : MonoBehaviour
{
    [SerializeField] private float lifeTime = 2f; 
    private void OnEnable()
    {
        Invoke(nameof(DisableObject), lifeTime);
    }

    private void DisableObject()
    {
        gameObject.SetActive(false); 
    }

    private void OnDisable()
    {
        CancelInvoke();
    }
}
