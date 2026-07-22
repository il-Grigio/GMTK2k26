using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Vector3 shootDir;
    private float moveSpeed;
    private ObjectPool pool;

    public void Setup(Vector3 shootDir, float speed, ObjectPool pool)
    {
        this.shootDir = shootDir;
        this.moveSpeed = speed;
        this.pool = pool;

        Invoke("Deactivate", 2f);
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
            Deactivate();
        }
    }
    private void Deactivate()
    {
        if(pool != null)
            pool.ReturnObject(this.gameObject);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }
}
