using UnityEngine;

// Basic stub for interactable objects like obstacles
public class InteractableObject : MonoBehaviour
{
    public int hp = 50;
    
    // Apply damage to the object
    public void TakeDamage(int amount)
    {
        hp -= amount;
        if (hp <= 0)
        {
            Debug.Log($"{gameObject.name} has been destroyed!");
            // Handle destruction
            Destroy(gameObject);
        }
    }
}
