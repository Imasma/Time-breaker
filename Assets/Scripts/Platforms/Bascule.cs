using UnityEngine;

public class Bascule : MonoBehaviour
{
    [Header("Réglages de la Bascule")]
    [SerializeField] private float maxRotationAngle = 20f; // angle maximum d'inclinaison
    [SerializeField] private float basculeSpeed = 5f;      // Vitesse à laquelle elle bascule
    [SerializeField] private float restoreSpeed = 2f;     // Vitesse à laquelle elle revient

    private Quaternion targetRotation;
    private bool isPlayerTouching; // Renommé pour plus de clarté (dessus ou dessous)
    private Transform playerTransform;
    
    void Update()
    {
        if (isPlayerTouching && playerTransform != null)
        {
            // Calcule la position relative (Locale) du joueur par rapport à la plateforme
            Vector3 localPlayerPos = transform.InverseTransformPoint(playerTransform.position);
            
            float localX = localPlayerPos.x;
            float localY = localPlayerPos.y;

            // On définit l'angle cible sur l'axe Z
            // Par défaut : localX positif (droite) -> tiltAmount négatif (penche à droite)
            float tiltAmount = -localX * maxRotationAngle;

            // INVERSION : Si le joueur est en dessous du centre de la plateforme
            if (localY < 0)
            {
                tiltAmount = -tiltAmount; // On inverse l'inclinaison
            }

            targetRotation = Quaternion.Euler(0, 0, tiltAmount);
        }
        else
        {
            // Si le joueur n'est plus là, on revient à la rotation initiale (à plat)
            targetRotation = Quaternion.identity;
        }

        // On applique la rotation de manière fluide
        float currentSpeed = isPlayerTouching ? basculeSpeed : restoreSpeed;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * currentSpeed);
    }

    // Détection du joueur (Collision pour le dessus ou le dessous)
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerTouching = true;
            playerTransform = collision.transform;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerTouching = false;
        }
    }
}