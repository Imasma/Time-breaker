using UnityEngine;

public class ImagesRemanentes : MonoBehaviour
{
    [Header("Réglages de l'effet")]
    [SerializeField] private GameObject ghostPrefab;
    [SerializeField] private float spawnFrequency = 0.1f;
    [SerializeField] private float ghostLifetime = 0.5f;
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Réglages du Buffer")]
    [SerializeField] private float slowBufferDuration = 0.2f; // Temps à attendre avant de spawn
    private float currentBufferTimer = 0f;

    [Header("Activation")]
    public bool isEnabled = true;

    private float spawnTimer;
    private bool isMoving; 

    void Update()
    {
        if (!isEnabled || playerMovement == null) return;

        float inputX = Input.GetAxisRaw("Horizontal");
        float inputZ = Input.GetAxisRaw("Vertical");
        isMoving = (Mathf.Abs(inputX) > 0.1f || Mathf.Abs(inputZ) > 0.1f);
        
        bool isConditionsMet = playerMovement.IsSlowModeActive() && !isMoving;

        if (isConditionsMet)
        {
            // On fait grimper le buffer tant qu'on est immobile en slow
            currentBufferTimer += Time.unscaledDeltaTime;

            // Si on a dépassé le temps de buffer, on peut commencer à spawn
            if (currentBufferTimer >= slowBufferDuration)
            {
                spawnTimer += Time.unscaledDeltaTime;

                if (spawnTimer >= spawnFrequency)
                {
                    SpawnGhost();
                    spawnTimer = 0;
                }
            }
        }
        else
        {
            // Dès qu'on bouge ou que le slow s'arrête, on reset tout
            currentBufferTimer = 0f;
            spawnTimer = 0f;
        }
    }

    void SpawnGhost()
    {
        GameObject currentGhost = Instantiate(ghostPrefab, transform.position, transform.rotation);
        Destroy(currentGhost, ghostLifetime);
    }
}