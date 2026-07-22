using System;
using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    private InputHandler _input;

    [SerializeField] private ObjectPool bulletPool;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletForce = 20f;

    private void Awake()
    {
        _input = GetComponent<InputHandler>();
    }

    private void OnEnable()
    {
        _input.OnShoot -= Shoot;
        _input.OnShoot += Shoot;
    }

    private void OnDisable()
    {
        _input.OnShoot -= Shoot;
    }

    private void Shoot()
    {
        GameObject bulletObj = bulletPool.GetObject();

        bulletObj.transform.position = firePoint.position;
        bulletObj.transform.rotation = firePoint.rotation;

        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
            bullet.Setup(firePoint.forward, bulletForce, bulletPool);
    }
}
