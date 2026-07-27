// Punto di aggancio tra SaloonNPC e il tuo Player.
// Implementa questa interfaccia sul tuo PlayerController / PlayerHealth reale.
public interface IDamageable
{
    void TakeDamage(NPCController shooter);
}
