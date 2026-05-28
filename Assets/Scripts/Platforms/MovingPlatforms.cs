using UnityEngine;

public class MovingPlatforms : MonoBehaviour
{
    [Header("Positions")]
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private GameObject GroundGO; // Le GameObject qui a le Collider et le Rigidbody

    [Header("Parameters")] 
    [SerializeField] private float speed = 2f;

    private Rigidbody rb;

    void Start()
    {
        if (GroundGO != null)
        {
            // On récupère le Rigidbody sur l'objet qui doit bouger
            rb = GroundGO.GetComponent<Rigidbody>();
            
            if (rb == null)
            {
                // On l'ajoute automatiquement s'il n'existe pas
                rb = GroundGO.AddComponent<Rigidbody>();
            }

            // Configuration cruciale pour une plateforme physique
            rb.isKinematic = true; 
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
        else
        {
            Debug.LogError("Veuillez assigner GroundGO dans l'inspecteur !", this);
        }
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            MovePlatform();
        }
    }

    private void MovePlatform()
    {
        // On utilise Time.fixedTime au lieu de Time.time car on est dans le FixedUpdate
        float t = Mathf.PingPong(Time.fixedTime * speed, 1f);
        
        // Calcul de la position cible
        Vector3 targetPosition = Vector3.Lerp(startPoint.position, endPoint.position, t);
        
        // Déplacement physique fluide
        rb.MovePosition(targetPosition);
    }
    
    private void OnDrawGizmos()
    {
        if (startPoint != null && endPoint != null && GroundGO != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(startPoint.position, endPoint.position);
            Gizmos.DrawWireCube(startPoint.position, GroundGO.transform.localScale);
            Gizmos.DrawWireCube(endPoint.position, GroundGO.transform.localScale);
        }
    }
}