using System;
using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    private InputHandler _input;

    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletForce = 20f;

    private void Awake()
    {
        _input = GetComponent<InputHandler>();
    }

    private void OnEnable()
    {
        if (_input.OnShoot != null)
            _input.OnShoot -= Shoot;
        _input.OnShoot += Shoot;
    }

    private void OnDisable()
    {
        if (_input.OnShoot != null)
            _input.OnShoot -= Shoot;
    }

    private void Shoot()
    {
        Bullet bullet = BulletManager.Instance.GetObject();

        bullet.transform.position = firePoint.position;
        bullet.transform.rotation = firePoint.rotation;

        if (bullet != null)
            bullet.Setup(firePoint.forward, bulletForce);
    }
}
