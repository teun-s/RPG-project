using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target; // The player to follow
    [SerializeField] private Vector2 boundarySize = new Vector2(4f, 2f); // Boundary size in world units

    private Camera cam;
    private Vector3 offset;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("CameraFollow script must be attached to a Camera!");
            return;
        }

        if (target == null)
        {
            Debug.LogError("Target not assigned in CameraFollow!");
            return;
        }

        // Initialize the offset (typically zero for a 2D game, but can be adjusted)
        offset = transform.position - target.position;
    }

    void LateUpdate()
    {
        if (target == null || cam == null) return;

        // Current camera position
        Vector3 cameraPosition = transform.position;

        // Player position in world space
        Vector3 playerPosition = target.position + offset;

        // Calculate the boundary
        float boundaryX = boundarySize.x * 0.5f; // Half the boundary width
        float boundaryY = boundarySize.y * 0.5f; // Half the boundary height

        // Check if the player has exited the boundary
        float deltaX = 0f;
        float deltaY = 0f;

        if (playerPosition.x > cameraPosition.x + boundaryX)
        {
            deltaX = playerPosition.x - (cameraPosition.x + boundaryX);
        }
        else if (playerPosition.x < cameraPosition.x - boundaryX)
        {
            deltaX = playerPosition.x - (cameraPosition.x - boundaryX);
        }

        if (playerPosition.y > cameraPosition.y + boundaryY)
        {
            deltaY = playerPosition.y - (cameraPosition.y + boundaryY);
        }
        else if (playerPosition.y < cameraPosition.y - boundaryY)
        {
            deltaY = playerPosition.y - (cameraPosition.y - boundaryY);
        }

        // Move the camera by the delta to bring the player back within the boundary
        cameraPosition.x += deltaX;
        cameraPosition.y += deltaY;

        // Update the camera position (keep Z unchanged for 2D)
        transform.position = new Vector3(cameraPosition.x, cameraPosition.y, transform.position.z);
    }
}
