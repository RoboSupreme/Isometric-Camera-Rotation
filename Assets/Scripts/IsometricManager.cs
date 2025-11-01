using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central manager for isometric rendering and sorting.
/// Directly inspired by Project Isometric's design.
/// </summary>
public class IsometricManager : MonoBehaviour
{
    // Singleton instance
    private static IsometricManager _instance;
    public static IsometricManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<IsometricManager>();
                
                if (_instance == null)
                {
                    GameObject go = new GameObject("IsometricManager");
                    _instance = go.AddComponent<IsometricManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    [Header("Isometric Settings")]
    public float tileWidth = 1.0f;
    public float tileHeight = 0.5f;
    
    [Header("Rendering")]
    public Transform characterContainer;
    public Transform tileContainer;
    public Transform effectsContainer;
    
    // List of all active sorters
    private List<IsometricSorter> activeSorters = new List<IsometricSorter>();
    
    // Camera settings
    [Header("Camera")]
    public Camera mainCamera;
    public bool useCustomSort = true;
    
    private void Awake()
    {
        // Ensure singleton behavior
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Set up containers if not assigned
        if (characterContainer == null)
        {
            GameObject charContainer = new GameObject("CharacterContainer");
            charContainer.transform.SetParent(transform);
            characterContainer = charContainer.transform;
        }
        
        if (tileContainer == null)
        {
            GameObject tileContainerObj = new GameObject("TileContainer");
            tileContainerObj.transform.SetParent(transform);
            tileContainer = tileContainerObj.transform;
        }
        
        if (effectsContainer == null)
        {
            GameObject effectsContainerObj = new GameObject("EffectsContainer");
            effectsContainerObj.transform.SetParent(transform);
            effectsContainer = effectsContainerObj.transform;
        }
        
        // Get main camera if not assigned
        if (mainCamera == null)
            mainCamera = Camera.main;
            
        // Check projection and warn if not orthographic
        if (mainCamera != null && !mainCamera.orthographic)
        {
            Debug.LogWarning("IsometricManager: Camera is not set to Orthographic mode. This may cause rendering issues.");
        }
        
        // Set up custom sort mode
        if (useCustomSort)
        {
            Debug.Log("IsometricManager: Remember to set Transparency Sort Mode to Custom Axis in Project Settings > Graphics");
            Debug.Log("IsometricManager: Recommended sort axis: X=0, Y=1, Z=-0.26");
        }
    }
    
    private void LateUpdate()
    {
        // Update all sorters
        for (int i = 0; i < activeSorters.Count; i++)
        {
            if (activeSorters[i] != null && activeSorters[i].isActiveAndEnabled)
            {
                activeSorters[i].UpdateSortingOrder();
            }
        }
    }
    
    /// <summary>
    /// Register a sorter with the manager
    /// </summary>
    public void RegisterSorter(IsometricSorter sorter)
    {
        if (sorter != null && !activeSorters.Contains(sorter))
        {
            activeSorters.Add(sorter);
        }
    }
    
    /// <summary>
    /// Unregister a sorter from the manager
    /// </summary>
    public void UnregisterSorter(IsometricSorter sorter)
    {
        if (sorter != null && activeSorters.Contains(sorter))
        {
            activeSorters.Remove(sorter);
        }
    }
    
    /// <summary>
    /// Convert grid position to world position
    /// </summary>
    public Vector3 GridToWorldPosition(Vector3Int gridPos)
    {
        // Direct conversion for our system
        return new Vector3(gridPos.x, gridPos.y, 0);
    }
    
    /// <summary>
    /// Calculate sorting value using Project Isometric's formula
    /// </summary>
    public float CalculateSortValue(Vector3 position)
    {
        // Directly using Project Isometric's NE view formula
        return position.y - (position.x + position.z);
    }
    
    /// <summary>
    /// Place a new object in the isometric world with correct sorting
    /// </summary>
    public GameObject PlaceObjectAtPosition(GameObject prefab, Vector3Int gridPosition, Transform parent = null)
    {
        if (prefab == null) return null;
        
        // Create the new object
        GameObject newObject = Instantiate(prefab, GridToWorldPosition(gridPosition), Quaternion.identity);
        
        // Assign parent
        if (parent == null)
        {
            // Auto-assign parent based on tag
            if (newObject.CompareTag("Player"))
                newObject.transform.SetParent(characterContainer);
            else
                newObject.transform.SetParent(tileContainer);
        }
        else
        {
            newObject.transform.SetParent(parent);
        }
        
        // Add IsometricSorter if it doesn't have one
        IsometricSorter sorter = newObject.GetComponent<IsometricSorter>();
        if (sorter == null)
        {
            sorter = newObject.AddComponent<IsometricSorter>();
        }
        
        // Update sorting
        sorter.UpdateSortingOrder();
        
        return newObject;
    }
}
