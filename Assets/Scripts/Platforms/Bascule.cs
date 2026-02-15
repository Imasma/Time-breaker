using UnityEngine;

public class Bascule : MonoBehaviour
{
    [Header("Réglages de la Bascule")]
    [SerializeField] private float maxRotationAngle = 20f; // angle maximum d'inclinaison
    [SerializeField] private float basculeSpeed = 5f;      // Vitesse à laquelle elle bascule
    [SerializeField] private float restoreSpeed = 2f;     // Vitesse à laquelle elle revient

    private Quaternion targetRotation;
    private bool isPlayerOnTop;
    private Transform playerTransform;
    
    
    
    void Update()
    {
        if (isPlayerOnTop && playerTransform != null)
        {
            // calcule la position relative du joueur sur l'axe X (de -0.5 à 0.5)
            // divise par la taille de la plateforme pour que l'inclinaison soit proportionnelle
            float localX = transform.InverseTransformPoint(playerTransform.position).x;
            float platformWidth = 1f;

            // On définit l'angle cible sur l'axe Z
            // Si localX est positif (droite), la rotation Z doit être négative pour pencher vers le bas
            float tiltAmount = -localX * maxRotationAngle;
            targetRotation = Quaternion.Euler(0, 0, tiltAmount);
        }
        else
        {
            // Si le joueur n'est plus là, on revient à la rotation initiale (à plat)
            targetRotation = Quaternion.identity;
        }

        // On applique la rotation de manière fluide
        float currentSpeed = isPlayerOnTop ? basculeSpeed : restoreSpeed;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * currentSpeed);
    }

    // Détection du joueur
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerOnTop = true;
            playerTransform = collision.transform;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerOnTop = false;
        }
    }
}