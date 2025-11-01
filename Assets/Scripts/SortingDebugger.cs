using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(IsometricSorter))]
public class SortingDebugger : MonoBehaviour
{
    private IsometricSorter sorter;
    private SpriteRenderer spriteRenderer;
    
    [Header("Debug Settings")]
    public bool showSortingInfo = true;
    public Color textColor = Color.yellow;
    
    void Start()
    {
        sorter = GetComponent<IsometricSorter>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    void OnGUI()
    {
        if (showSortingInfo && sorter != null && spriteRenderer != null)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
            
            // Convert to GUI coordinates
            screenPos.y = Screen.height - screenPos.y;
            
            GUIStyle style = new GUIStyle();
            style.normal.textColor = textColor;
            style.fontSize = 12;
            style.fontStyle = FontStyle.Bold;
            style.alignment = TextAnchor.UpperCenter;
            
            // Display position and sorting order
            GUI.Label(
                new Rect(screenPos.x - 100, screenPos.y - 40, 200, 20),
                $"Pos: ({transform.position.x:F1}, {transform.position.y:F1}, {transform.position.z:F1})",
                style
            );
            
            GUI.Label(
                new Rect(screenPos.x - 100, screenPos.y - 20, 200, 20),
                $"Sort Order: {spriteRenderer.sortingOrder}",
                style
            );
        }
    }
}
