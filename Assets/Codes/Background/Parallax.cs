using UnityEngine;

public class Parallax : MonoBehaviour
{
    [Header("Parallax Settings")]
    public float parallaxEffect = 0.5f;
    public bool infiniteScrolling = true;
    private Transform cameraTransform;
    private Vector3 lastCameraPosition;
    private float textureUnitSize;
    
    void Start()
    {
        cameraTransform = Camera.main.transform;
        lastCameraPosition = cameraTransform.position;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if(sr != null)
        {
            textureUnitSize = sr.bounds.size.x;
        }    
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;
        transform.position += new Vector3(deltaMovement.x * parallaxEffect, 0, 0);

        lastCameraPosition = cameraTransform.position;

        if (infiniteScrolling)
        {
            float deltaX = cameraTransform.position.x - transform.position.x;
            if(Mathf.Abs(deltaX) >= textureUnitSize)
            {
                float offsetPosition = deltaX > 0 ? textureUnitSize : -textureUnitSize;
                transform.position = new Vector3(
                    transform.position.x + offsetPosition,
                    transform.position.y,
                    transform.position.z
                );
            }
        }
    }
}
