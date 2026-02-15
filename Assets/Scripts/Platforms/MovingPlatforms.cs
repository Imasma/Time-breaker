using UnityEngine;

public class MovingPlatforms : MonoBehaviour
{
    [Header("Positions")]
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;

    [Header("Parameters")] 
    [SerializeField] private float speed = 2f;

    void Update()
    {
        MovePlatform();
    }

    private void MovePlatform()
    {
        // On calcule un facteur qui varie entre 0 et 1 selon le temps et la vitesse
        float t = Mathf.PingPong(Time.time * speed, 1f);
        
        transform.position = Vector3.Lerp(startPoint.position, endPoint.position, t);
    }
    
    private void OnDrawGizmos()
    {
        if (startPoint != null && endPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(startPoint.position, endPoint.position);
            Gizmos.DrawWireCube(startPoint.position, transform.localScale);
            Gizmos.DrawWireCube(endPoint.position, transform.localScale);
        }
    }
}