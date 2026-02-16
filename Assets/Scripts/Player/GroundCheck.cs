using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    [SerializeField] private float groundRayLength = 0.2f; // Longueur sous la sphère
    [SerializeField] private float groundRayOffSet = 0.1f; // Marge par rapport aux bords
    
    private SphereCollider sphere;
    public bool isGrounded; // Changé en public pour y accéder depuis le PlayerMovement
    [HideInInspector] public bool isOnBascule; // Nouvelle variable pour forcer le sol
    

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
        // On utilise X pour la largeur et Y pour la hauteur du rayon
        float radiusX = sphere.radius * transform.lossyScale.x; 
        float radiusY = sphere.radius * transform.lossyScale.y; 
        
        // La distance part du centre, traverse le rayon Y et s'étend un peu plus
        float totalDist = radiusY + groundRayLength;

        // On définit 3 positions sur l'axe X (Horizontal)
        float[] xOffsets = {
            -radiusX + groundRayOffSet, // Gauche
            0f,                         // Centre
            radiusX - groundRayOffSet   // Droite
        };
        
        foreach (float xOffset in xOffsets)
        {
            // On décale l'origine sur l'axe X (gauche/droite)
            Vector3 rayOrigin = center + Vector3.right * xOffset;

            // On check vers le BAS (Vector3.down)
            if (CheckGround(rayOrigin, Vector3.down, totalDist)) isGrounded = true;
        }

        Debug.Log("Grounded: " + isGrounded);
    }

    private bool CheckGround(Vector3 origin, Vector3 direction, float distance)
    {
        RaycastHit hit;
        // Debug en rouge pour le sol
        Debug.DrawRay(origin, direction * distance, Color.red);

        if (Physics.Raycast(origin, direction, out hit, distance))
        {
            if (hit.collider.CompareTag("Ground"))
            {
                Debug.DrawRay(hit.point, hit.normal, Color.yellow);
                return true;
            }
        }
        return false;
    }
}