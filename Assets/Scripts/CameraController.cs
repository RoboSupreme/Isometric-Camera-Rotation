using UnityEngine;

// Basic stub for camera controller
public class CameraController : MonoBehaviour
{
    private Vector3 originalLocalPosition;
    
    void Start()
    {
        // Store original local position relative to parent
        originalLocalPosition = transform.localPosition;
    }
    
    // Reset the camera to its original position relative to parent
    public void ResetPosition()
    {
        transform.localPosition = originalLocalPosition;
    }
}
