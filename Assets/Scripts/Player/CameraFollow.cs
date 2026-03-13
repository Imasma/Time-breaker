using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;        
    [Header("Offsets")]
    public Vector3 normalOffset = new Vector3(0f, 2f, -10f); 
    public Vector3 slowModeOffset = new Vector3(0f, 3f, -15f); 
    
    [Header("Settings")]
    public float followSpeed = 10f; // Remplace le smoothTime (plus la valeur est haute, plus c'est rapide)
    public float offsetTransitionSpeed = 5f; 
    [Tooltip("Délai (en secondes) avant que la caméra ne commence à changer de perspective")]
    public float timer = 1f; 

    private Vector3 currentOffset;
    private PlayerMovement playerScript;

    private bool currentCameraIsSlow = false; 
    private float currentWaitTime = 0f;       

    void Start()
    {
        currentOffset = normalOffset;
        if (target != null)
        {
            playerScript = target.GetComponent<PlayerMovement>();
        }
    }

    // LATE UPDATE : Toujours utiliser ça pour la caméra !
    void LateUpdate()
    {
        if (target == null) return;

        // 1. État réel du joueur
        bool playerIsSlow = (playerScript != null && playerScript.IsSlowModeActive());

        // 2. Gestion du délai
        // On utilise Time.deltaTime standard, car on ne modifie plus le Time.timeScale de Unity
        if (playerIsSlow != currentCameraIsSlow)
        {
            currentWaitTime += Time.deltaTime;

            if (currentWaitTime >= timer)
            {
                currentCameraIsSlow = playerIsSlow;
                currentWaitTime = 0f; 
            }
        }
        else
        {
            currentWaitTime = 0f;
        }

        // 3. Offset cible
        Vector3 targetOffset = currentCameraIsSlow ? slowModeOffset : normalOffset;

        // 4. Transition de l'offset 
        currentOffset = Vector3.Lerp(currentOffset, targetOffset, Time.deltaTime * offsetTransitionSpeed);

        // 5. Mouvement de la caméra
        // On utilise Lerp, c'est plus robuste si on utilise time scale de unity.
        Vector3 targetPosition = target.position + currentOffset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);
    }
}