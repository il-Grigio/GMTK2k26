using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    private inputHandler _input;

    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletForce = 20f;

    private void Awake()
    {
        _input = GetComponent<inputHandler>();
    }

    private void Update()
    {
        if (_input.IsShooting)
            Shoot();
    }

    private void Shoot()
    {
        GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
            bullet.Setup(firePoint.forward, bulletForce);
    }
}
