using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;        
    [Header("Offsets")]
    public Vector3 normalOffset = new Vector3(0f, 2f, -10f); 
    public Vector3 slowModeOffset = new Vector3(0f, 3f, -15f); // Un peu plus haut et loin par exemple
    
    [Header("Settings")]
    public float smoothTime = 0.2f;  
    public float offsetTransitionSpeed = 5f; // Vitesse de transition de l'offset
    [Tooltip("Délai (en secondes) avant que la caméra ne commence à changer de perspective")]
    public float timer = 1f; // Je l'ai passé en public pour l'inspecteur

    private Vector3 currentOffset;
    private Vector3 velocity = Vector3.zero;
    private PlayerMovement playerScript;

    // --- NOUVELLES VARIABLES POUR LE DÉLAI ---
    private bool currentCameraIsSlow = false; // L'état actuel que la caméra applique
    private float currentWaitTime = 0f;       // Le chronomètre en cours

    void Start()
    {
        currentOffset = normalOffset;
        // On récupère le script de mouvement sur la cible pour connaître l'état du slow
        if (target != null)
        {
            playerScript = target.GetComponent<PlayerMovement>();
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 1. On regarde quel est l'état REEL du joueur
        bool playerIsSlow = (playerScript != null && playerScript.IsSlowModeActive());

        // 2. GESTION DU DÉLAI
        // Si le joueur est dans un état différent de celui de la caméra, on lance le chrono
        if (playerIsSlow != currentCameraIsSlow)
        {
            // On utilise unscaledDeltaTime pour que le chrono de 1 seconde dure vraiment 1 seconde, 
            // même si le jeu est au ralenti !
            currentWaitTime += Time.unscaledDeltaTime;

            // Si le timer est écoulé, la caméra valide le changement d'état
            if (currentWaitTime >= timer)
            {
                currentCameraIsSlow = playerIsSlow;
                currentWaitTime = 0f; // On réinitialise le chrono pour la prochaine fois
            }
        }
        else
        {
            // Si le joueur a changé d'avis avant la fin du timer (ex: il appuie sur Bas puis relâche), on annule
            currentWaitTime = 0f;
        }

        // 3. On détermine l'offset en fonction de l'état VALIDÉ de la caméra
        Vector3 targetOffset = currentCameraIsSlow ? slowModeOffset : normalOffset;

        // 4. Transition fluide de l'offset actuel vers l'offset cible
        // Utilise unscaledDeltaTime pour que la caméra change d'offset même au ralenti
        currentOffset = Vector3.Lerp(currentOffset, targetOffset, Time.unscaledDeltaTime * offsetTransitionSpeed);

        Vector3 targetPosition = target.position + currentOffset;

        // 5. Mouvement de la caméra
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