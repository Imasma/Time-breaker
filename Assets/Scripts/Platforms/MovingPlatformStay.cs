using UnityEngine;

public class MovingPlatformStay : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    
    private Rigidbody playerRb;
    private Vector3 previousPlatformPosition;

    // On utilise FixedUpdate car on manipule la physique (Rigidbody)
    private void FixedUpdate()
    {
        
        // 1. On calcule la distance parcourue par la plateforme depuis la frame précédente
        Vector3 platformMovement = transform.position - previousPlatformPosition;
            
        // 2. On "téléporte" fluidement le joueur de cette même distance
        playerRb.MovePosition(playerRb.position + platformMovement);
        
        // On met à jour l'ancienne position pour la prochaine frame
        previousPlatformPosition = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            // On récupère le Rigidbody du joueur au lieu de le parenter
            playerRb = other.GetComponent<Rigidbody>();
            
            // On initialise la position pour éviter un bond géant à la première frame
            previousPlatformPosition = transform.position; 
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            // Le joueur quitte la plateforme, on arrête de le suivre
            playerRb = null;
        }    
    }
}
