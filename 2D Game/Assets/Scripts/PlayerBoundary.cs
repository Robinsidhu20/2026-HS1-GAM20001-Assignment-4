using UnityEngine;

public class PlayerBoundary : MonoBehaviour
{
    [SerializeField] private float leftBound = 0f;
    [SerializeField] private float rightBound = 172f;
    [SerializeField] private float bottomBound = -5f;

    private void Update()
    {
        Vector3 pos = transform.position;

        pos.x = Mathf.Clamp(pos.x, leftBound, rightBound);

        if (pos.y < bottomBound)
        {
            pos.y = 2f;
            pos.x = 2f;
        }

        transform.position = pos;
    }
}
