using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Vector3 shootDir;
    private float moveSpeed;

    public void Setup(Vector3 shootDir, float speed)
    {
        this.shootDir = shootDir;
        this.moveSpeed = speed;
        Destroy(gameObject, 2f);
    }

    private void Update()
    { 
        transform.position += shootDir * moveSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Destroy(other.gameObject);
            gameObject.SetActive(false);
        }
    }
}
