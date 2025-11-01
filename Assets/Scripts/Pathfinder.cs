using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
/*
// A simple pathfinding utility class
public static class Pathfinder
{
    // Find a path between two points on the grid
    public static List<Vector3Int> FindPath(TileMapController tileMap, Vector3Int start, Vector3Int target)
    {
        // If start or target are the same, return just the target
        if (start == target)
        {
            return new List<Vector3Int> { target };
        }
        
        // Dictionary to track the best previous node for each node
        Dictionary<Vector3Int, Vector3Int> cameFrom = new Dictionary<Vector3Int, Vector3Int>();
        
        // Dictionary to track the cost so far to reach each node
        Dictionary<Vector3Int, float> costSoFar = new Dictionary<Vector3Int, float>();
        
        // Priority queue (implemented as a sorted list for simplicity)
        List<Vector3IntWithPriority> frontier = new List<Vector3IntWithPriority>();
        
        // Initialize with start position
        frontier.Add(new Vector3IntWithPriority(start, 0));
        costSoFar[start] = 0;
        
        bool pathFound = false;
        
        // Process nodes in order of priority
        while (frontier.Count > 0)
        {
            // Get the node with lowest priority (cost)
            frontier.Sort((a, b) => a.priority.CompareTo(b.priority));
            Vector3Int current = frontier[0].position;
            frontier.RemoveAt(0);
            
            // If we've reached the target, we're done
            if (current == target)
            {
                pathFound = true;
                break;
            }
            
            // Check neighbors
            foreach (Vector3Int next in GetNeighboursForCell(current))
            {
                // Skip if not a valid tile
                if (!tileMap.tilemap.HasTile(next))
                {
                    continue;
                }
                
                // Get movement cost to this neighbor
                float movementCost = 1f; // Default cost
                
                // If we have tile data, use its movement cost
                if (tileMap.tileDataMap != null && tileMap.tileDataMap.ContainsKey(next))
                {
                    movementCost = tileMap.tileDataMap[next].MovementCost;
                    
                    // Consider height difference
                    if (tileMap.tileDataMap.ContainsKey(current))
                    {
                        // Get heights as floats
                        float currentHeight = tileMap.tileDataMap[current].Height;
                        float nextHeight = tileMap.tileDataMap[next].Height;
                        float heightDiff = Mathf.Abs(nextHeight - currentHeight);
                        
                        // Add cost for height changes
                        movementCost += heightDiff * 0.5f;
                        
                        // Default max jump height
                        int maxJumpHeight = 1;
                        
                        // Skip if height difference is too great
                        if (heightDiff > maxJumpHeight)
                        {
                            continue;
                        }
                    }
                }
                
                // Calculate new cost
                float newCost = costSoFar[current] + movementCost;
                
                // If we've found a better path to this node
                if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                {
                    costSoFar[next] = newCost;
                    
                    // Calculate priority using Manhattan distance heuristic
                    float priority = newCost + ManhattanDistance(next, target);
                    frontier.Add(new Vector3IntWithPriority(next, priority));
                    
                    // Record how we got here
                    cameFrom[next] = current;
                }
            }
        }
        
        // If no path was found, return null
        if (!pathFound)
        {
            return null;
        }
        
        // Reconstruct the path from target to start
        List<Vector3Int> path = new List<Vector3Int>();
        Vector3Int current2 = target;
        
        // Work backwards from target to start
        while (current2 != start)
        {
            path.Add(current2);
            current2 = cameFrom[current2];
        }
        
        // Add the start position
        path.Add(start);
        
        // Reverse the path to go from start to target
        path.Reverse();
        
        return path;
    }
    
    // Get neighboring cells (up, down, left, right)
    public static Vector3Int[] GetNeighboursForCell(Vector3Int cell)
    {
        return new Vector3Int[]
        {
            new Vector3Int(cell.x, cell.y + 1, cell.z), // Up
            new Vector3Int(cell.x, cell.y - 1, cell.z), // Down
            new Vector3Int(cell.x + 1, cell.y, cell.z), // Right
            new Vector3Int(cell.x - 1, cell.y, cell.z)  // Left
        };
    }
    
    // Calculate Manhattan distance between two grid cells
    private static float ManhattanDistance(Vector3Int a, Vector3Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
    
    // Helper struct for priority queue
    private struct Vector3IntWithPriority
    {
        public Vector3Int position;
        public float priority;
        
        public Vector3IntWithPriority(Vector3Int pos, float prio)
        {
            position = pos;
            priority = prio;
        }
    }
}
*/