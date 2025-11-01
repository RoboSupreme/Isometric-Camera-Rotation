using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
/*
// Basic stub for game manager - implements Singleton pattern
public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }
    
    // Game mode enum
    public enum GameMode
    {
        ExplorationMode,
        CombatMode,
        DialogueMode
    }
    
    // Current game mode
    public GameMode currentGameMode = GameMode.CombatMode;
    
    // Object tracking dictionary
    public Dictionary<Vector3Int, GameObject> objectPositions = new Dictionary<Vector3Int, GameObject>();
    
    // Movement flags
    public bool isMoving = false;
    public bool isCombatMovementMode = false;
    public bool isCombatAttackMode = false;
    
    private void Awake()
    {
        // Singleton implementation
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    // Move a game object along a path
    public void MoveGameObjectAlongPath(GameObject obj, List<Vector3Int> path)
    {
        if (obj != null && path != null && path.Count > 0)
        {
            // Update object positions dictionary
            Vector3Int oldPos = Vector3Int.FloorToInt(obj.transform.position);
            Vector3Int newPos = path[path.Count - 1];
            
            // Remove from old position
            if (objectPositions.ContainsKey(oldPos) && objectPositions[oldPos] == obj)
            {
                objectPositions.Remove(oldPos);
            }
            
            // Add to new position
            objectPositions[newPos] = obj;
            
            // Start movement coroutine
            StartCoroutine(MoveAlongPathCoroutine(obj, path));
        }
    }
    
    // Coroutine to handle movement along a path
    private IEnumerator MoveAlongPathCoroutine(GameObject obj, List<Vector3Int> path)
    {
        isMoving = true;
        
        // Get the tilemap to convert cells to world positions
        Tilemap tilemap = FindAnyObjectByType<Tilemap>();
        if (tilemap == null)
        {
            Debug.LogError("No Tilemap found!");
            isMoving = false;
            yield break;
        }
        
        // Convert cell positions to world positions
        List<Vector3> worldPath = new List<Vector3>();
        foreach (Vector3Int cell in path)
        {
            worldPath.Add(tilemap.GetCellCenterWorld(cell));
        }
        
        // Movement speed
        float moveSpeed = 5f;
        
        // Move along each segment of the path
        for (int i = 0; i < worldPath.Count; i++)
        {
            Vector3 startPos = obj.transform.position;
            Vector3 targetPos = worldPath[i];
            
            // Adjust for height if we have a TileMapController
            TileMapController tileMapController = tilemap.GetComponent<TileMapController>();
            if (tileMapController != null)
            {
                // Get the current cell position
                Vector3Int cellPos = path[i];
                
                // Adjust the target position's y-coordinate based on height
                if (tileMapController.tileDataMap.ContainsKey(cellPos))
                {
                    float heightOffset = tileMapController.tileDataMap[cellPos].Height * tileMapController.heightVisualizationScale;
                    targetPos.y += heightOffset;
                }
            }
            
            // Preserve z position (for sorting)
            targetPos.z = startPos.z;
            
            // Move towards the target
            float journeyLength = Vector3.Distance(startPos, targetPos);
            float startTime = Time.time;
            
            while (obj.transform.position != targetPos)
            {
                float distCovered = (Time.time - startTime) * moveSpeed;
                float fractionOfJourney = distCovered / journeyLength;
                
                obj.transform.position = Vector3.Lerp(startPos, targetPos, fractionOfJourney);
                
                if (fractionOfJourney >= 1.0f)
                {
                    obj.transform.position = targetPos;
                }
                
                yield return null;
            }
        }
        
        isMoving = false;
    }
}
*/