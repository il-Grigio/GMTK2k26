using System;
using UnityEngine;

public class PlayerInteractionShooting : PlayerInteraction
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletForce = 20f;
    [SerializeField] private Animator anim;
    protected override void OnEnable()
    {
        if (_input.OnShoot != null)
            _input.OnShoot -= Shoot;
        _input.OnShoot += Shoot;
    }

    protected override void OnDisable()
    {
        if (_input.OnShoot != null)
            _input.OnShoot -= Shoot;
    }

    private void Shoot()
    {
        if (!InventorySystem.Instance.CanShoot()) return;

        InventorySystem.Instance.RemoveBullet();
        if (anim != null)
            anim.SetTrigger("Shoot");

        Bullet bullet = BulletManager.Instance.GetObject();

        bullet.transform.position = firePoint.position;
        bullet.transform.rotation = firePoint.rotation;

        if (bullet != null)
        {
            CameraShake.Instance.StartShake(0.5f,15f,0.25f);
            bullet.Setup(firePoint.forward, bulletForce);
        }
    }
}
