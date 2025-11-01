using UnityEngine;

// Basic stub for booster cards that provide bonuses
public class BoosterCard : MonoBehaviour
{
    public enum BoostType
    {
        Health,
        Attack,
        Movement,
        Defense
    }
    
    public BoostType boostType = BoostType.Health;
    public int boostAmount = 10;
    
    // Apply the boost effect to a character
    public void ApplyBoost(FirstCharacterController character)
    {
        switch (boostType)
        {
            case BoostType.Health:
                character.Heal(boostAmount);
                Debug.Log($"Applied health boost of {boostAmount} to {character.name}");
                break;
                
            case BoostType.Attack:
                character.attackDamage += boostAmount;
                Debug.Log($"Applied attack boost of {boostAmount} to {character.name}");
                break;
                
            case BoostType.Movement:
                character.temporaryMovementBonus += boostAmount;
                Debug.Log($"Applied movement boost of {boostAmount} to {character.name}");
                break;
                
            case BoostType.Defense:
                // If you add defense later
                Debug.Log($"Applied defense boost of {boostAmount} to {character.name}");
                break;
        }
    }
    
    // Remove the booster card from the map
    public void RemoveBoosterCardFromMap(GameObject boosterCardObject)
    {
        Destroy(boosterCardObject);
    }
}
