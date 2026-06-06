using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 1f, -10f);

    [Header("Look Ahead")]
    [SerializeField] private float lookAheadDistance = 3f;
    [SerializeField] private float lookAheadSmooth = 3f;

    [Header("Camera Bounds")]
    [SerializeField] private float minX = 0f;
    [SerializeField] private float maxX = 160f;
    [SerializeField] private float minY = 0f;
    [SerializeField] private float maxY = 10f;

    private float currentLookAhead = 0f;
    private float lastPlayerX;

    private void Start()
    {
        if (player != null)
            lastPlayerX = player.position.x;
    }

    private void LateUpdate()
    {
        if (player == null) return;

        float moveDirection = player.position.x - lastPlayerX;
        lastPlayerX = player.position.x;

        float targetLookAhead = 0f;
        if (moveDirection > 0.01f)
            targetLookAhead = lookAheadDistance;
        else if (moveDirection < -0.01f)
            targetLookAhead = -lookAheadDistance;

        currentLookAhead = Mathf.Lerp(currentLookAhead, targetLookAhead, lookAheadSmooth * Time.deltaTime);

        Vector3 targetPosition = player.position + offset;
        targetPosition.x += currentLookAhead;

        targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
        targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);

        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
    }
}
