using System;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Vector3 shootDir;
    private float moveSpeed;

    [SerializeField] TrailRenderer trailRenderer;
    [SerializeField] private GameObject hitPrefab;

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

    private void OnCollisionEnter(Collision other)
    {
        if (hitPrefab != null)
        {
            ContactPoint contact = other.contacts[0];
            Instantiate(hitPrefab, contact.point, Quaternion.LookRotation(contact.normal));
        }

        Deactivate();
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
