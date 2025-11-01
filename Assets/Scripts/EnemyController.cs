using UnityEngine;

// Basic stub for enemy controller
public class EnemyController : MonoBehaviour
{
    // Combat properties
    public int hp = 100;
    public int maxHp = 100;
    public int attackDamage = 15;
    
    // Effects properties
    public bool isStunned = false;
    public int temporaryMovementBonus = 0;
    public int temporaryMovementPenalty = 0;
    
    // Apply damage to the enemy
    public void TakeDamage(int amount)
    {
        hp -= amount;
        if (hp <= 0)
        {
            hp = 0;
            Debug.Log($"{gameObject.name} has been defeated!");
            // Handle death
            Destroy(gameObject, 0.5f); // Destroy after a small delay
        }
    }
    
    // Heal the enemy
    public void Heal(int amount)
    {
        hp = Mathf.Min(hp + amount, maxHp);
    }
}
