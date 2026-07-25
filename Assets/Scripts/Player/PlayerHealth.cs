using UnityEngine;

// Esempio minimo: mettilo sul GameObject del player (con Tag = "Player")
// oppure implementa IDamageable direttamente nel tuo PlayerController esistente.
public class PlayerHealth : MonoBehaviour, IDamageable
{
    public int maxHealth = 3;
    public int currentHealth = 3;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(SaloonNPC shooter)
    {
        int dmg = 1;
        currentHealth -= dmg;
        Debug.Log($"Il player è stato colpito da {shooter.npcName} per {dmg} danni! Vita: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Debug.Log("Il player è morto.");
            // TODO: game over / respawn / ragdoll
        }
    }
}
