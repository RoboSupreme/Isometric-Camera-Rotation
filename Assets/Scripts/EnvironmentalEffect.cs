using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class that represents a temporary environmental effect on a tile
/// </summary>
public class EnvironmentalEffect
{
    public enum EffectTypes
    {
        Fire,          // Damage over time
        Poison,        // Damage over time, different visual
        Healing,       // Healing over time
        SpeedBoost,    // Increases movement range temporarily
        SpeedPenalty,  // Decreases movement range temporarily
        Stun,          // Prevents movement
        Teleport       // Teleports character to another tile
    }

    public EffectTypes EffectType { get; private set; }
    public float Duration { get; set; }       // Duration in seconds, -1 for permanent
    public float TickRate { get; set; }       // How often the effect ticks (in seconds)
    public float NextTickTime { get; set; }   // When the next tick will occur
    public int Potency { get; set; }          // How strong the effect is
    public GameObject VisualEffect { get; set; } // Visual representation of the effect

    // Constructor
    public EnvironmentalEffect(EffectTypes effectType, float duration = 5f, int potency = 1, float tickRate = 1f)
    {
        EffectType = effectType;
        Duration = duration;
        Potency = potency;
        TickRate = tickRate;
        NextTickTime = Time.time + tickRate;
    }

    // Update the effect (returns false if the effect has expired)
    public bool Update()
    {
        if (Duration > 0)
        {
            Duration -= Time.deltaTime;
            if (Duration <= 0)
            {
                return false; // Effect has expired
            }
        }

        // Check if it's time for the next tick
        if (Time.time >= NextTickTime)
        {
            NextTickTime = Time.time + TickRate;
            return true; // Time to apply the tick effect
        }

        return true; // Effect is still active but not ticking
    }

    // Apply the effect to a character
    public void ApplyEffect(GameObject character)
    {
        // Get relevant component from character
        FirstCharacterController characterController = character.GetComponent<FirstCharacterController>();
        EnemyController enemyController = character.GetComponent<EnemyController>();

        if (characterController != null)
        {
            ApplyToPlayerCharacter(characterController);
        }
        else if (enemyController != null)
        {
            ApplyToEnemyCharacter(enemyController);
        }
    }

    // Apply tick effect (called when the effect ticks)
    public void ApplyTickEffect(GameObject character)
    {
        // Get relevant component from character
        FirstCharacterController characterController = character.GetComponent<FirstCharacterController>();
        EnemyController enemyController = character.GetComponent<EnemyController>();

        if (characterController != null)
        {
            ApplyTickToPlayerCharacter(characterController);
        }
        else if (enemyController != null)
        {
            ApplyTickToEnemyCharacter(enemyController);
        }
    }

    // Apply effect to player character
    private void ApplyToPlayerCharacter(FirstCharacterController character)
    {
        switch (EffectType)
        {
            case EffectTypes.SpeedBoost:
                character.temporaryMovementBonus += Potency;
                break;
            case EffectTypes.SpeedPenalty:
                character.temporaryMovementPenalty += Potency;
                break;
            case EffectTypes.Stun:
                character.isStunned = true;
                break;
            case EffectTypes.Teleport:
                // Teleport will be handled separately through the tile system
                break;
            // Initial application for DoT/HoT effects doesn't do anything
            // They work on tick
        }
    }

    // Apply effect to enemy character
    private void ApplyToEnemyCharacter(EnemyController enemy)
    {
        switch (EffectType)
        {
            case EffectTypes.SpeedBoost:
                enemy.temporaryMovementBonus += Potency;
                break;
            case EffectTypes.SpeedPenalty:
                enemy.temporaryMovementPenalty += Potency;
                break;
            case EffectTypes.Stun:
                enemy.isStunned = true;
                break;
            case EffectTypes.Teleport:
                // Teleport will be handled separately through the tile system
                break;
            // Initial application for DoT/HoT effects doesn't do anything
            // They work on tick
        }
    }

    // Apply tick effect to player character
    private void ApplyTickToPlayerCharacter(FirstCharacterController character)
    {
        switch (EffectType)
        {
            case EffectTypes.Fire:
                character.TakeDamage(Potency * 5);
                break;
            case EffectTypes.Poison:
                character.TakeDamage(Potency * 3);
                break;
            case EffectTypes.Healing:
                character.Heal(Potency * 5);
                break;
            // Other effects don't have tick functionality
        }
    }

    // Apply tick effect to enemy character
    private void ApplyTickToEnemyCharacter(EnemyController enemy)
    {
        switch (EffectType)
        {
            case EffectTypes.Fire:
                enemy.TakeDamage(Potency * 5);
                break;
            case EffectTypes.Poison:
                enemy.TakeDamage(Potency * 3);
                break;
            case EffectTypes.Healing:
                enemy.Heal(Potency * 5);
                break;
            // Other effects don't have tick functionality
        }
    }
}
