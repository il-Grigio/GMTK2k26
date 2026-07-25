using System;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public enum ShooterFaction
    {
        Player,
        NPC
    }

    private Vector3 shootDir;
    private float moveSpeed;
    private ShooterFaction shooterFaction = ShooterFaction.Player;
    private SaloonNPC shooterNpc; // valorizzato solo se a sparare è stato un NPC, altrimenti null (= player)

    [SerializeField] TrailRenderer trailRenderer;
    [SerializeField] private GameObject hitPrefab;

    // Overload di compatibilità con il codice esistente del player: se non specifichi chi spara,
    // si assume sia il player (nessuna modifica richiesta lato player).
    public void Setup(Vector3 shootDir, float speed)
    {
        Setup(shootDir, speed, ShooterFaction.Player, null);
    }

    public void Setup(Vector3 shootDir, float speed, ShooterFaction faction, SaloonNPC shooterNpc)
    {
        this.shootDir = shootDir;
        this.moveSpeed = speed;
        this.shooterFaction = faction;
        this.shooterNpc = shooterNpc;

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
            EffectPoolManager.Instance.Get(EffectPoolManager.EffectType.bullet, contact.point, Quaternion.FromToRotation(Vector3.up, contact.normal));
        }

        TryApplyDamage(other.collider);

        Deactivate();
    }

    private void TryApplyDamage(Collider hitCollider)
    {
        Debug.Log("Bullet ha colpito: " + hitCollider.name);
        SaloonNPC hitNpc = hitCollider.GetComponentInParent<SaloonNPC>();

        // Fuoco amico disattivato: se a sparare è stato un NPC, il colpo non fa danno ad altri NPC
        // (le sparatorie NPC contro NPC sono già gestite dalla logica in SaloonNPC, non dalla fisica del proiettile)
        if (shooterFaction == ShooterFaction.NPC && hitNpc != null)
        {
            Debug.Log("NPC trovato: " + (hitNpc != null));
            return;
        }

        IDamageable damageable = hitCollider.GetComponentInParent<IDamageable>();
        Debug.Log("Damageable trovato: " + (damageable != null));
        if (damageable == null) return;

        // shooterNpc è null quando a sparare è il player: stessa convenzione usata nel resto del progetto
        damageable.TakeDamage(shooterNpc);
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
