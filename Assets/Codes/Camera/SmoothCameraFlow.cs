using UnityEngine;

public class Cameramovement : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform target;
    public float smoothTime = 0.3f;
    public Vector3 offset = new Vector3(0, 0, -10);
    public Vector3 velocity = Vector3.zero;
    void FixedUpdate()
    {
        if (target == null)
        {
            Debug.LogWarning("Camera target is not assigned!");
        }

        Vector3 targetPosition = target.position + offset;
        targetPosition.z = transform.position.z;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothTime
        );
    }
}
