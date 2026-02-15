using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;        
    public Vector3 offset = new Vector3(0f, 2f, -10f); 
    public float smoothTime = 0.2f;  

    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition = target.position + offset;

        // même quand le monde est au ralenti.
        transform.position = Vector3.SmoothDamp(
            transform.position, 
            targetPosition, 
            ref velocity, 
            smoothTime, 
            Mathf.Infinity, 
            Time.unscaledDeltaTime
        );
    }
}