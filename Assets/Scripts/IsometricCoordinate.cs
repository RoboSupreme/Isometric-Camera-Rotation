using UnityEngine;

/// <summary>
/// Represents a coordinate in an isometric grid system
/// </summary>
public struct IsometricCoordinate
{
    public int x;
    public int y;
    public int z; // Height level

    public IsometricCoordinate(int x, int y, int z = 0)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    // Create from a Vector3Int
    public IsometricCoordinate(Vector3Int position)
    {
        this.x = position.x;
        this.y = position.y;
        this.z = position.z;
    }

    // Convert to Vector3Int
    public Vector3Int ToVector3Int()
    {
        return new Vector3Int(x, y, z);
    }

    // Convert to Vector2Int (ignoring Z)
    public Vector2Int ToVector2Int()
    {
        return new Vector2Int(x, y);
    }

    // Convert isometric coordinate to world position - using standard Unity approach
    public Vector3 ToWorldPosition(float tileWidth = 1f, float tileHeight = 1f, float heightScale = 1f)
    {
        // Direct conversion - matching what Unity's tilemap does by default
        float worldX = x;
        float worldY = y;
        float worldZ = 0;
        
        // Add height based on z-coordinate
        float heightOffset = z * heightScale;
        worldY += heightOffset;

        return new Vector3(worldX, worldY, worldZ);
    }

    // Convert world position to isometric coordinate - using standard Unity approach
    public static IsometricCoordinate FromWorldPosition(Vector3 worldPosition, float tileWidth = 1f, float tileHeight = 1f, float heightScale = 1f)
    {
        // Direct conversion - matching what Unity's tilemap does
        int x = Mathf.RoundToInt(worldPosition.x);
        
        // Subtract height to get base coordinate
        float baseY = worldPosition.y;
        int z = 0;
        
        // Determine Z coordinate based on height
        if (heightScale > 0)
        {
            z = Mathf.RoundToInt(baseY / heightScale);
            baseY -= z * heightScale;
        }
        
        int y = Mathf.RoundToInt(baseY);
        
        return new IsometricCoordinate(x, y, z);
    }

    // Get adjacent coordinate in a specific direction
    public IsometricCoordinate GetAdjacentCoordinate(Direction direction)
    {
        switch (direction)
        {
            // Grid-aligned directions
            case Direction.North: // Grid North
                return new IsometricCoordinate(x, y + 1, z);
            case Direction.South: // Grid South
                return new IsometricCoordinate(x, y - 1, z);
            case Direction.East:  // Grid East
                return new IsometricCoordinate(x + 1, y, z);
            case Direction.West:  // Grid West
                return new IsometricCoordinate(x - 1, y, z);
            
            // Isometric visual directions (diagonals on grid)
            case Direction.NorthEast: // Screen Up-Right
                return new IsometricCoordinate(x + 1, y + 1, z);
            case Direction.NorthWest: // Screen Up-Left
                return new IsometricCoordinate(x - 1, y + 1, z);
            case Direction.SouthEast: // Screen Down-Right
                return new IsometricCoordinate(x + 1, y - 1, z);
            case Direction.SouthWest: // Screen Down-Left
                return new IsometricCoordinate(x - 1, y - 1, z);
                
            default:
                return this;
        }
    }

    // Override ToString for debugging
    public override string ToString()
    {
        return $"Iso({x}, {y}, {z})";
    }

    // Override equality operators
    public override bool Equals(object obj)
    {
        if (!(obj is IsometricCoordinate))
            return false;

        var other = (IsometricCoordinate)obj;
        return x == other.x && y == other.y && z == other.z;
    }

    public override int GetHashCode()
    {
        return x.GetHashCode() ^ (y.GetHashCode() << 2) ^ (z.GetHashCode() >> 2);
    }

    public static bool operator ==(IsometricCoordinate a, IsometricCoordinate b)
    {
        return a.x == b.x && a.y == b.y && a.z == b.z;
    }

    public static bool operator !=(IsometricCoordinate a, IsometricCoordinate b)
    {
        return !(a == b);
    }
}

// Direction enum for movement
public enum Direction
{
    North,     // Grid North (+Y)
    South,     // Grid South (-Y)
    East,      // Grid East (+X)
    West,      // Grid West (-X)
    NorthEast, // Screen Up-Right (+X, +Y)
    NorthWest, // Screen Up-Left (-X, +Y)
    SouthEast, // Screen Down-Right (+X, -Y)
    SouthWest  // Screen Down-Left (-X, -Y)
}
