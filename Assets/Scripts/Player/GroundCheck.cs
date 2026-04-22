using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    [SerializeField] private float groundRayLength = 0.2f; 
    [SerializeField] private float groundRayOffSet = 0.1f; 
    
    private SphereCollider sphere;
    public bool isGrounded; 
    [HideInInspector] public bool isOnBascule; 
    

    private void Start()
    {
        sphere = GetComponent<SphereCollider>(); 
    }

    private void Update()
    {
        RayGroundDetection();
        if (isOnBascule) isGrounded = true;
    }

    private void RayGroundDetection()
    {
        isGrounded = false; 

        Vector3 center = transform.position + sphere.center;
        float radiusX = sphere.radius * transform.lossyScale.x; 
        float radiusY = sphere.radius * transform.lossyScale.y; 
        
        float totalDist = radiusY + groundRayLength;

        float[] xOffsets = {
            -radiusX + groundRayOffSet, 
            0f,                         
            radiusX - groundRayOffSet   
        };
        
        foreach (float xOffset in xOffsets)
        {
            Vector3 rayOrigin = center + Vector3.right * xOffset;
            if (CheckGround(rayOrigin, Vector3.down, totalDist)) isGrounded = true;
        }

        Debug.Log("Grounded: " + isGrounded);
    }

    private bool CheckGround(Vector3 origin, Vector3 direction, float distance)
    {
        RaycastHit hit;
        Debug.DrawRay(origin, direction * distance, Color.red);

        // On ajoute 'Physics.DefaultRaycastLayers' (pour toucher tous les layers) 
        // et 'QueryTriggerInteraction.Ignore' pour traverser les Triggers
        if (Physics.Raycast(origin, direction, out hit, distance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.CompareTag("Ground") || hit.collider.CompareTag("Clone"))
            {
                Debug.DrawRay(hit.point, hit.normal, Color.yellow);
                return true;
            }
        }
        return false;
    }
}