using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;        // Le Player
    public Vector3 offset = new Vector3(0f, 2f, -10f); // Décalage caméra
    public float smoothTime = 0.2f;  // Plus petit = plus réactif

    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}
