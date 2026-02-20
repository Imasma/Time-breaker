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

    private Vector3 currentOffset;
    private Vector3 velocity = Vector3.zero;
    private PlayerMovement playerScript;

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

        // On détermine quel offset utiliser selon l'état du joueur
        Vector3 targetOffset = (playerScript != null && playerScript.IsSlowModeActive()) 
            ? slowModeOffset 
            : normalOffset;

        // Transition fluide de l'offset actuel vers l'offset cible
        // Utilise unscaledDeltaTime pour que la caméra change d'offset même au ralenti
        currentOffset = Vector3.Lerp(currentOffset, targetOffset, Time.unscaledDeltaTime * offsetTransitionSpeed);

        Vector3 targetPosition = target.position + currentOffset;

        // même quand le monde est au ralenti.
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