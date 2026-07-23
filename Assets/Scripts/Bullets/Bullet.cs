using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Vector3 shootDir;
    private float moveSpeed;

    [SerializeField] TrailRenderer trailRenderer;

    public void Setup(Vector3 shootDir, float speed)
    {
        this.shootDir = shootDir;
        this.moveSpeed = speed;

        if (trailRenderer != null)
        {
            trailRenderer.Clear();
        }

        Invoke(nameof(Deactivate), 2f);
    }

    private void Update()
    { 
        transform.position += shootDir * (moveSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Destroy(other.gameObject);
            Deactivate();
        }
    }
    private void Deactivate()
    {
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }
}
