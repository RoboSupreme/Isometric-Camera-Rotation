using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
/*
public class TileMapController : MonoBehaviour
{
    // Tilemap Reference
    public Tilemap tilemap;

    // Tile Data Storage
    public Dictionary<Vector3Int, TileData> tileDataMap = new Dictionary<Vector3Int, TileData>();
    
    // Height Visualization Scale
    public float heightVisualizationScale = 0.5f;
    
    // Isometric grid settings
    [Header("Isometric Settings")]
    public float tileWidth = 1f;
    public float tileHeight = 0.5f;  // Standard 2:1 isometric ratio
    
    // Debug visualization
    public bool showHeightDebug = true;
    public Color heightDebugColor = new Color(1f, 0.5f, 0f, 0.5f);
    
    // Player Character Reference
    public GameObject playerCharacter;
    private Vector3Int playerPosition;
    
    // Dictionary to track topmost blocks at each 2D position
    private Dictionary<Vector2Int, Vector3Int> topmostBlocks = new Dictionary<Vector2Int, Vector3Int>();
    
    // Variables for jumping
    private bool isJumping = false;
    private float jumpHeight = 0.5f;
    private float jumpDuration = 0.5f;
    
    void Start()
    {
        // Initialize tilemap component if not assigned
        if (tilemap == null)
        {
            tilemap = GetComponent<Tilemap>();
        }
        
        // Scan for z-positioned tiles
        ScanTilemapLayers();
        
        // Initialize tile data
        InitializeTileData();
        
        // Update topmost blocks tracking
        UpdateTopmostBlocks();
        
        // Debug all topmost tiles
        DebugTopmostTiles();
        
        // Position player at the start
        if (playerCharacter != null)
        {
            SnapPlayerToNearestTile();
        }
        else
        {
            Debug.LogWarning("Player character not assigned to TileMapController!");
        }
    }
    
    void Update()
    {
        // Handle WASD/arrow key movement for the player
        if (playerCharacter != null)
        {
            HandlePlayerMovement();
        }
        
        // Handle mouse clicks for tile bounce effect
        HandleMouseClicks();
    }
    
    // Track the last hovered tile for hover effects
    private Vector3Int? lastHoveredTile = null;
    
    // Handle mouse hover and click effects for tiles
    private void HandleMouseClicks()
    {
        // Get the mouse position in world space
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0; // Reset Z since we're working in 2D
        
        // Convert to grid position
        Vector3Int hoveredCell = tilemap.WorldToCell(mouseWorldPos);
        
        // If the mouse is hovering over a valid tile
        if (IsTileAtPosition(hoveredCell))
        {
            // Get the topmost tile at this position using the topmostBlocks dictionary
            Vector2Int key = new Vector2Int(hoveredCell.x, hoveredCell.y);
            if (topmostBlocks.TryGetValue(key, out Vector3Int topmostTile))
            {
                // If we're hovering over a new tile
                if (lastHoveredTile == null || !lastHoveredTile.Value.Equals(topmostTile))
                {
                    // Debug which cell is being hovered
                    Debug.Log($"[HOVER] Mouse hovering at grid pos {topmostTile}");
                    
                    // Remember this tile for future implementation
                    lastHoveredTile = topmostTile;
                    
                    // Animation code removed - will be reimplemented with a new approach
                }
            }
        }
        else
        {
            // We're not hovering over any tile
            lastHoveredTile = null;
        }
    }
    
    // Check if there's a tile at the given position
    private bool IsTileAtPosition(Vector3Int position)
    {
        // Check if this position exists in our topmostBlocks dictionary
        Vector2Int key = new Vector2Int(position.x, position.y);
        return topmostBlocks.ContainsKey(key);
    }
    
    // This coroutine has been removed for a new animation approach
    
    // This method has been removed for a new animation approach
    
    // These animation utility methods have been removed
    
    // Initialize tile data for all tiles in the grid
    private void InitializeTileData()
    {
        // Clear existing data
        tileDataMap.Clear();
        
        // Get bounds of the tilemap
        BoundsInt bounds = tilemap.cellBounds;
        
        // Loop through all tiles in the tilemap
        foreach (Vector3Int position in bounds.allPositionsWithin)
        {
            if (tilemap.HasTile(position))
            {
                // Create new tile data
                TileData newTileData = new TileData(position);
                
                // IMPORTANT: ALWAYS use Z position as the height
                // This ensures consistency between position and height value
                newTileData.Height = position.z;
                
                // Add to dictionary
                tileDataMap.Add(position, newTileData);
                
                Debug.Log($"Added tile at ({position.x}, {position.y}, {position.z}) with height {newTileData.Height}");
            }
        }
        
        Debug.Log($"Initialized tile data for {tileDataMap.Count} tiles");
    }
    
    // Scan the tilemap for tiles at different Z heights
    private void ScanTilemapLayers()
    {
        BoundsInt bounds = tilemap.cellBounds;
        HashSet<int> uniqueZLevels = new HashSet<int>();
        
        // Find all unique Z levels in the tilemap
        foreach (Vector3Int position in bounds.allPositionsWithin)
        {
            if (tilemap.HasTile(position) && position.z != 0)
            {
                uniqueZLevels.Add(position.z);
            }
        }
        
        // Log the found Z levels
        if (uniqueZLevels.Count > 0)
        {
            Debug.Log($"Found {uniqueZLevels.Count} height levels in tilemap: {string.Join(", ", uniqueZLevels)}");
        }
        else
        {
            Debug.Log("No height levels found in tilemap (all tiles are at Z=0)");
        }
    }
    
    // Update the topmost blocks tracking
    private void UpdateTopmostBlocks()
    {
        topmostBlocks.Clear();
        
        // Find the highest tile at each X,Y position
        foreach (var tileEntry in tileDataMap)
        {
            Vector3Int position3D = tileEntry.Key;
            TileData tileData = tileEntry.Value;
            
            // Create a 2D position key (X,Y only)
            Vector2Int position2D = new Vector2Int(position3D.x, position3D.y);
            
            // If we haven't seen this position or this tile has a higher Z coordinate
            if (!topmostBlocks.ContainsKey(position2D) || 
                position3D.z > topmostBlocks[position2D].z)
            {
                topmostBlocks[position2D] = position3D;
                Debug.Log($"[TOP] Set topmost tile at ({position2D.x}, {position2D.y}) to tile with z={position3D.z}");
            }
        }
        
        Debug.Log($"Updated topmost blocks: {topmostBlocks.Count} positions tracked");
    }
    
    // Debug output showing all topmost tiles
    private void DebugTopmostTiles()
    {
        Debug.Log($"=== TOPMOST TILES (Total: {topmostBlocks.Count}) ===");
        foreach (var entry in topmostBlocks)
        {
            Vector2Int pos2D = entry.Key;
            Vector3Int pos3D = entry.Value;
            float height = tileDataMap.ContainsKey(pos3D) ? tileDataMap[pos3D].Height : 0f;
            Debug.Log($"Tile at ({pos2D.x}, {pos2D.y}) → 3D position: ({pos3D.x}, {pos3D.y}, {pos3D.z}), Height: {height}");
        }
    }
    
    // Get the topmost tile at a specific 2D position
    public Vector3Int GetTopmostTile(Vector2Int position2D)
    {
        if (topmostBlocks.ContainsKey(position2D))
        {
            return topmostBlocks[position2D];
        }
        
        // Default fallback if no topmost tile is found
        return new Vector3Int(position2D.x, position2D.y, 0);
    }
    
    // Set height of a specific tile and update tracking
    public void SetTileHeight(Vector3Int position, float height)
    {
        if (!tileDataMap.ContainsKey(position))
        {
            tileDataMap[position] = new TileData(position);
        }
        
        tileDataMap[position].Height = height;
        
        // Update topmost blocks after changing height
        UpdateTopmostBlocks();
    }
    
    // Position a game object at the correct height for a tile
    public void AdjustObjectToHeight(GameObject obj, Vector3Int position)
    {
        if (!tileDataMap.ContainsKey(position))
        {
            return;
        }
        
        // Get the base position for this tile
        Vector3 worldPos = tilemap.GetCellCenterWorld(position);
        
        // Handle specific case where (-4, 2, 2) should match (-3, 3, 0)
        // This means Z=2 is equivalent to moving (x+1, y+1)
        Vector3 finalPos;
        
        if (position.z > 0)
        {
            // Calculate equivalent isometric position based on height
            int newX = position.x + Mathf.FloorToInt(position.z / 2.0f);
            int newY = position.y + Mathf.FloorToInt(position.z / 2.0f);
            
            // Get the world position for the equivalent flat position
            finalPos = tilemap.GetCellCenterWorld(new Vector3Int(newX, newY, 0));
            
            // IMPORTANT: For odd Z values, add 0.25 to Y
            if (position.z % 2 != 0) // If Z is odd
            {
                finalPos.y += 0.25f;
                Debug.Log($"ODD Z: Adding 0.25 to Y position for Z={position.z}");
            }
            
            Debug.Log($"HEIGHT CONVERSION: ({position.x}, {position.y}, {position.z}) → ({newX}, {newY}, 0) with y-adjustment: {finalPos.y:F2}");
        }
        else
        {
            // If no height, just use the regular position
            finalPos = worldPos;
        }
        
        // Always subtract 0.01 from Y position to prevent characters being covered by tiles
        // No need to check for Player tag since we want this for all movable objects
        float yOffset = -0.01f;
        
        // Apply the y offset to ensure visibility above tiles
        // Use the original Z value from the position parameter to maintain height in transform
        obj.transform.position = new Vector3(finalPos.x, finalPos.y + yOffset, position.z);
        
        // Example validation:
        // (-4, 2, 2) should have same position as (-3, 3, 0) → (-3.00, 0.25, 0.00)
        
        // Additional debug info
        Debug.Log($"Position at height {position.z}: world position = ({obj.transform.position.x:F2}, {obj.transform.position.y:F2}, {obj.transform.position.z:F2})");
        
        // Log the exact position
        Debug.Log($"Positioned object at grid ({position.x}, {position.y}, {position.z}) → world pos ({obj.transform.position.x:F2}, {obj.transform.position.y:F2}, {obj.transform.position.z:F2})");
    }
    
    // Place character on a specific tile (with height adjustment)
    public void PlaceCharacterOnTile(GameObject character, Vector3Int cellPosition)
    {
        AdjustObjectToHeight(character, cellPosition);
    }
    
    // Snap player to the nearest tile
    private void SnapPlayerToNearestTile()
    {
        // Log original character position
        Vector3 originalPosition = playerCharacter.transform.position;
        Debug.Log($"[SNAP] Original character position: {originalPosition}");
        
        // Get the nearest tile position in 2D
        Vector3Int cellPosition = tilemap.WorldToCell(originalPosition);
        Vector2Int position2D = new Vector2Int(cellPosition.x, cellPosition.y);
        Debug.Log($"[SNAP] Initial cell position: ({cellPosition.x}, {cellPosition.y}, {cellPosition.z})");
        
        // Get all tiles at this X,Y position
        List<Vector3Int> tilesAtPosition = new List<Vector3Int>();
        foreach (var entry in tileDataMap.Keys)
        {
            if (entry.x == position2D.x && entry.y == position2D.y)
            {
                tilesAtPosition.Add(entry);
                Debug.Log($"[SNAP] Found tile at ({entry.x}, {entry.y}, {entry.z}) with height {tileDataMap[entry].Height}");
            }
        }
        
        // Get the topmost tile at this position
        Vector3Int topmostTile = GetTopmostTile(position2D);
        Debug.Log($"[SNAP] Topmost tile: ({topmostTile.x}, {topmostTile.y}, {topmostTile.z}) with z-height {topmostTile.z}");
        
        // Store as current player position
        playerPosition = topmostTile;
        
        // Position the player on this tile
        PlaceCharacterOnTile(playerCharacter, topmostTile);
        Vector3 newPosition = playerCharacter.transform.position;
        Debug.Log($"[SNAP] Player snapped from {originalPosition} to {newPosition} on tile ({topmostTile.x}, {topmostTile.y}, {topmostTile.z})");
    }
    
    // Convert world position to grid position (for mouse clicks and other world inputs)
    public Vector3Int WorldToGridPosition(Vector3 worldPosition)
    {
        // Use our IsometricCoordinate for the conversion
        IsometricCoordinate iso = IsometricCoordinate.FromWorldPosition(worldPosition, tileWidth, tileHeight, heightVisualizationScale);
        return iso.ToVector3Int();
    }
    
    // Convert grid position to world position
    public Vector3 GridToWorldPosition(Vector3Int gridPosition)
    {
        IsometricCoordinate iso = new IsometricCoordinate(gridPosition);
        Vector3 worldPos = iso.ToWorldPosition(tileWidth, tileHeight, heightVisualizationScale);
        
        // Add height offset
        float heightOffset = gridPosition.z * heightVisualizationScale;
        return new Vector3(worldPos.x, worldPos.y + heightOffset, 0);
    }
    
    // Handle player movement with WASD/arrow keys directly on grid coordinates
    private void HandlePlayerMovement()
    {
        // Check for jump input
        if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
        {
            StartCoroutine(JumpCoroutine());
            return; // Skip other movement during jump start
        }
        
        // If already jumping, don't allow movement
        if (isJumping)
        {
            return;
        }
        
        // Get current position
        Vector3Int currentPos = playerPosition;
        Vector3Int newPosition = currentPos;
        bool moved = false;
        
        // Remapped keys with W and S swapped
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            // W now moves X+1 (East) [was S/D]
            newPosition.x += 1;
            moved = true;
            Debug.Log($"[GRID] W key: Moving X+1 from ({currentPos.x}, {currentPos.y}) to ({newPosition.x}, {newPosition.y})");
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            // S now moves X-1 (West) [was W/A]
            newPosition.x -= 1;
            moved = true;
            Debug.Log($"[GRID] S key: Moving X-1 from ({currentPos.x}, {currentPos.y}) to ({newPosition.x}, {newPosition.y})");
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            // A now moves Y+1 (North) [swapped with D]
            newPosition.y += 1;
            moved = true;
            Debug.Log($"[GRID] A key: Moving Y+1 from ({currentPos.x}, {currentPos.y}) to ({newPosition.x}, {newPosition.y})");
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            // D now moves Y-1 (South) [swapped with A]
            newPosition.y -= 1;
            moved = true;
            Debug.Log($"[GRID] D key: Moving Y-1 from ({currentPos.x}, {currentPos.y}) to ({newPosition.x}, {newPosition.y})");
        }
        
        // If player tried to move, handle the movement
        if (moved)
        {
            // Convert to 2D position for tile lookups
            Vector2Int position2D = new Vector2Int(newPosition.x, newPosition.y);
            
            Debug.Log($"[GRID] Attempting to move to position ({newPosition.x}, {newPosition.y})");
            
            // Log all tiles at this position for debugging
            List<Vector3Int> tilesAtPosition = new List<Vector3Int>();
            foreach (var entry in tileDataMap.Keys)
            {
                if (entry.x == position2D.x && entry.y == position2D.y)
                {
                    tilesAtPosition.Add(entry);
                    Debug.Log($"[GRID] Found tile at ({entry.x}, {entry.y}, {entry.z}) with height {tileDataMap[entry].Height}");
                }
            }
            
            // Get the topmost tile at this position
            Vector3Int topmostTile = GetTopmostTile(position2D);
            
            // Check for Z position changes - explicitly log height differences
            int zDifference = topmostTile.z - playerPosition.z;
            string heightChangeMessage = zDifference > 0 ? 
                $"[GRID] Moving UP by {zDifference} Z levels" : 
                (zDifference < 0 ? $"[GRID] Moving DOWN by {Mathf.Abs(zDifference)} Z levels" : "[GRID] Staying on same Z level");
            
            Debug.Log($"[GRID] Topmost tile selected: ({topmostTile.x}, {topmostTile.y}, {topmostTile.z})");
            Debug.Log(heightChangeMessage);
            
            // Check if there's a valid tile at this position
            if (tilemap.HasTile(topmostTile))
            {
                Vector3 oldPosition = playerCharacter.transform.position;
                Vector3Int oldTilePos = playerPosition;
                
                // Update to new position, ensuring Z matches the topmost tile
                playerPosition = topmostTile;
                PlaceCharacterOnTile(playerCharacter, topmostTile);
                
                Vector3 newPos = playerCharacter.transform.position;
                Debug.Log($"[GRID] Player moved from ({oldTilePos.x}, {oldTilePos.y}, {oldTilePos.z}) to ({topmostTile.x}, {topmostTile.y}, {topmostTile.z})");
                Debug.Log($"[GRID] World position change: {oldPosition} → {newPos}");
            }
            else
            {
                Debug.Log($"[GRID] Movement blocked - no valid tile at ({newPosition.x}, {newPosition.y})");
            }
        }
    }
    
    // Draw height indicators in the editor for debugging
    void OnDrawGizmos()
    {
        if (showHeightDebug && tilemap != null && tileDataMap != null)
        {
            Gizmos.color = heightDebugColor;
            
            foreach (var tileEntry in tileDataMap)
            {
                Vector3Int cellPosition = tileEntry.Key;
                TileData tileData = tileEntry.Value;
                
                if (tileData.Height > 0)
                {
                    Vector3 worldPos = tilemap.GetCellCenterWorld(cellPosition);
                    
                    // Draw a cube at each height level
                    for (int i = 0; i < Mathf.FloorToInt(tileData.Height); i++)
                    {
                        float yOffset = i * heightVisualizationScale;
                        Vector3 cubePos = new Vector3(worldPos.x, worldPos.y + yOffset, worldPos.z);
                        
                        // Size decreases with height for a nice visual effect
                        float size = 0.5f - (i * 0.1f);
                        size = Mathf.Max(0.2f, size);
                        
                        Gizmos.DrawCube(cubePos, new Vector3(size, size, 0.1f));
                    }
                }
            }
        }
    }
    
    // Coroutine for jump animation
    private IEnumerator JumpCoroutine()
    {
        isJumping = true;
        Debug.Log("Jump started!");
        
        // Get the IsometricSorter component if it exists
        IsometricSorter sorter = playerCharacter.GetComponent<IsometricSorter>();
        
        // Store the original position
        Vector3 startPosition = playerCharacter.transform.position;
        float originalY = startPosition.y;
        
        // Jump up phase
        float jumpUpTime = jumpDuration / 2f;
        float elapsedTime = 0;
        
        while (elapsedTime < jumpUpTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / jumpUpTime; // Normalized time (0 to 1)
            float heightProgress = Mathf.Sin(t * Mathf.PI * 0.5f); // Smooth easing
            
            // Calculate new position with jump height
            float currentJumpHeight = jumpHeight * heightProgress;
            float newY = originalY + currentJumpHeight;
            
            // Update the position
            playerCharacter.transform.position = new Vector3(
                startPosition.x,
                newY,
                startPosition.z
            );
            
            // Update the sorting order based on jump height
            if (sorter != null)
            {
                sorter.SetJumpHeight(currentJumpHeight);
            }
            
            yield return null; // Wait for next frame
        }
        
        // Jump down phase
        float jumpDownTime = jumpDuration / 2f;
        elapsedTime = 0;
        
        while (elapsedTime < jumpDownTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / jumpDownTime; // Normalized time (0 to 1)
            float heightProgress = Mathf.Cos(t * Mathf.PI * 0.5f); // Smooth easing
            
            // Calculate new position with jump height
            float currentJumpHeight = jumpHeight * heightProgress;
            float newY = originalY + currentJumpHeight;
            
            // Update the position
            playerCharacter.transform.position = new Vector3(
                startPosition.x,
                newY,
                startPosition.z
            );
            
            // Update the sorting order based on jump height
            if (sorter != null)
            {
                sorter.SetJumpHeight(currentJumpHeight);
            }
            
            yield return null; // Wait for next frame
        }
        
        // Ensure we land exactly at the original position
        playerCharacter.transform.position = startPosition;
        
        // Reset sorting order
        if (sorter != null)
        {
            sorter.SetJumpHeight(0);
        }
        
        Debug.Log("Jump completed!");
        isJumping = false;
    }
}
*/