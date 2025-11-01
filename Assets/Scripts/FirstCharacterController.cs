using UnityEngine;

// Basic stub for the character controller
public class FirstCharacterController : MonoBehaviour
{
    // Combat properties
    public int hp = 100;
    public int maxHp = 100;
    public int combatMovementRange = 5;
    public int combatAttackRange = 2;
    public int attackDamage = 20;
    
    // Effects properties
    public bool isStunned = false;
    public int temporaryMovementBonus = 0;
    public int temporaryMovementPenalty = 0;
    
    // Apply damage to the character
    public void TakeDamage(int amount)
    {
        hp -= amount;
        if (hp <= 0)
        {
            hp = 0;
            Debug.Log($"{gameObject.name} has been defeated!");
            // Handle death
        }
    }
    
    // Called when the character is moved to a new tile position
    public void OnPositionChanged(Vector3Int newTilePosition)
    {
        Debug.Log($"{gameObject.name} moved to tile {newTilePosition}");
        // You can add any character-specific logic here when position changes
    }
    
    // Heal the character
    public void Heal(int amount)
    {
        hp = Mathf.Min(hp + amount, maxHp);
    }
    
    // Attack an enemy
    public void Attack(EnemyController enemy, int damageBonus = 0)
    {
        if (enemy != null)
        {
            int damage = attackDamage + damageBonus;
            Debug.Log($"{gameObject.name} attacks for {damage} damage!");
            enemy.TakeDamage(damage);
        }
    }
    
    // Attack an object
    public void Attack(InteractableObject obstacle, int damageBonus = 0)
    {
        if (obstacle != null)
        {
            int damage = attackDamage + damageBonus;
            Debug.Log($"{gameObject.name} attacks object for {damage} damage!");
            obstacle.TakeDamage(damage);
        }
    }
}
