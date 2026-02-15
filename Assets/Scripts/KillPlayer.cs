using UnityEngine;

public class KillPlayer : MonoBehaviour
{
    [Tooltip("Sera rempli automatiquement au Start si un objet a le tag 'Respawn Point'")]
    public Transform respawnPoint;

    private void Start()
    {
        
        // On cherche l'objet qui possède le tag exact
        GameObject foundRespawn = GameObject.FindWithTag("Respawn Point");

        if (foundRespawn != null)
        { 
            respawnPoint = foundRespawn.transform;
        }

        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && respawnPoint != null)
        {
            // Téléportation
            other.transform.position = respawnPoint.position;
            
            // On reset la physique pour éviter que le joueur ne garde son élan
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
            }
        }
    }
}