using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class that stores all relevant data for a single tile in the grid
/// </summary>
public class TileData
{
    // Basic properties
    public Vector3Int Position { get; set; }
    public float Height { get; set; } = 0f; // Using float to support fractional heights (0.5 tiles)
    public int MovementCost { get; set; } = 1;
    public bool IsOccupied { get; set; } = false;
    public GameObject Occupant { get; set; } = null;
    public int OccupantTeam { get; set; } = -1; // -1 = no team, 0 = player, 1 = enemy

    // Tile properties
    public enum TileType
    {
        Normal,
        Obstacle,
        Water,
        Lava,
        Ice,
        Teleporter
    }

    public TileType Type { get; set; } = TileType.Normal;

    // Environmental effects
    public List<EnvironmentalEffect> Effects { get; set; } = new List<EnvironmentalEffect>();

    // Constructor
    public TileData(Vector3Int position, int height = 0)
    {
        Position = position;
        Height = height;
    }

    // Methods to modify tile data
    public void AddEffect(EnvironmentalEffect effect)
    {
        // Check if effect already exists, if so update duration
        for (int i = 0; i < Effects.Count; i++)
        {
            if (Effects[i].EffectType == effect.EffectType)
            {
                Effects[i] = effect; // Replace with new effect
                return;
            }
        }
        
        Effects.Add(effect);
    }

    public void RemoveEffect(EnvironmentalEffect.EffectTypes effectType)
    {
        Effects.RemoveAll(e => e.EffectType == effectType);
    }

    public void ClearEffects()
    {
        Effects.Clear();
    }

    public bool HasEffectOfType(EnvironmentalEffect.EffectTypes effectType)
    {
        return Effects.Exists(e => e.EffectType == effectType);
    }

    // Check if a character can move to this tile (considering height from originHeight)
    public bool IsAccessibleFrom(float originHeight, float maxJumpHeight)
    {
        if (IsOccupied) return false;
        
        // Check height difference
        float heightDifference = Mathf.Abs(Height - originHeight);
        return heightDifference <= maxJumpHeight;
    }

    // Apply all active effects to a character
    public void ApplyEffectsToCharacter(GameObject character)
    {
        foreach (var effect in Effects)
        {
            effect.ApplyEffect(character);
        }
    }
}
