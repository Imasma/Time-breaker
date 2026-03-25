using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;        
    [Header("Offsets")]
    public Vector3 normalOffset = new Vector3(0f, 2f, -10f); 
    public Vector3 slowModeOffset = new Vector3(0f, 3f, -15f); 
    
    [Header("Settings")]
    public float smoothTime = 0.2f; // On remet smoothTime, plus naturel pour une caméra (ex: 0.15f à 0.3f)
    public float offsetTransitionSpeed = 5f; 
    [Tooltip("Délai (en secondes de jeu) avant que la caméra ne commence à changer de perspective")]
    public float timer = 1f; 

    private Vector3 currentOffset;
    private PlayerMovement playerScript;
    private Vector3 velocity = Vector3.zero; // Requis par le SmoothDamp

    private bool currentCameraIsSlow = false; 
    private float currentWaitTime = 0f;       

    void Start()
    {
        currentOffset = normalOffset;
        playerScript = target.GetComponent<PlayerMovement>();
        
    }

    void LateUpdate()
    {
        // --- LIAISON AVEC LE TIME MANAGER ---
        float currentCustomTimeScale = (TimeManager.Instance != null) ? TimeManager.Instance.customTimeScale : 1f;
        float customDeltaTime = Time.deltaTime * currentCustomTimeScale;

        // 1. État réel du joueur
        bool playerIsSlow = (playerScript != null && playerScript.IsSlowModeActive());

        // 2. Gestion du délai avec Custom Time
        if (playerIsSlow != currentCameraIsSlow)
        {
            currentWaitTime += customDeltaTime;

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
        currentOffset = Vector3.Lerp(currentOffset, targetOffset, customDeltaTime * offsetTransitionSpeed);

        // 5. Mouvement de la caméra avec SmoothDamp + CustomTime !
        Vector3 targetPosition = target.position + currentOffset;
        
        // On passe customDeltaTime à la fin pour qu'il ignore le Time.timeScale de Unity !
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime, Mathf.Infinity, customDeltaTime);
    }
}