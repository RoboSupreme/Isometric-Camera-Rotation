using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class IsometricSorter : MonoBehaviour
{
    [Header("Sorting Settings")]
    public SpriteRenderer spriteRenderer;
    
    // Higher values put the object in front
    public int baseOrderOffset = 0;
    
    // Special settings for player characters
    [Header("Player Settings")]
    public bool isPlayer = false;
    public int playerBoost = 1000; // Very high value to ensure player is always on top
    
    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Check if this is tagged as a player
        if (gameObject.CompareTag("Player") && !isPlayer)
        {
            isPlayer = true;
            Debug.Log("Player tag detected, enabling isPlayer automatically");
        }
    }
    
    private void Start()
    {
        // Initial sort update
        UpdateSortingOrder();
    }
    
    private void Update()
    {
        // Update sorting every frame to ensure consistent visibility
        UpdateSortingOrder();
    }
    
    public void UpdateSortingOrder()
    {
        if (spriteRenderer == null) return;
        
        Vector3 pos = transform.position;
        
        // Proper isometric sorting formula: y - (x + z)
        // This is the classic isometric sorting formula used in many isometric games
        float sortingValue = pos.y - (pos.x + pos.z);
        
        // Convert to an integer sorting order (negate because lower value = higher priority)
        int order = Mathf.RoundToInt(-sortingValue * 100);
        
        // Add Z height component to the sorting order (higher Z = higher priority)
        order += Mathf.RoundToInt(pos.z * 1000);
        
        // Add base offset
        order += baseOrderOffset;
        
        // If this is the player, add a large boost to ensure it's always on top of tiles
        if (isPlayer)
        {
            order += playerBoost;
        }
        
        // Apply the calculated order
        spriteRenderer.sortingOrder = order;
    }
    
    // Called from the jump system to temporarily adjust sorting during jumps
    public void SetJumpHeight(float jumpHeight)
    {
        if (jumpHeight > 0)
        {
            // Directly set a high sort order during jumps to ensure visibility
            spriteRenderer.sortingOrder = baseOrderOffset + (isPlayer ? playerBoost : 0) + 5000;
        }
        else
        {
            // Reset to normal sorting when not jumping
            UpdateSortingOrder();
        }
    }
}
