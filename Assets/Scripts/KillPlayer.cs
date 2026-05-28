using UnityEngine;

public class KillPlayer : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private FMODUnity.EventReference deathSound;
    private FMOD.Studio.EventInstance deathSoundInstance; // Stocke le son en cours

    [Tooltip("Sera rempli automatiquement au Start si un objet a le tag 'Respawn Point'")]
    public Transform respawnPoint;
    public Cheat Cheat;

    private void Awake()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Cheat = player.GetComponent<Cheat>();
        }
    }

    private void Start()
    {
        GameObject foundRespawn = GameObject.FindWithTag("Respawn Point");
        if (foundRespawn != null) 
        { 
            respawnPoint = foundRespawn.transform;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && respawnPoint != null && Cheat.godMode == false)
        {

            
            // Si le son est déjà en train de jouer, on le coupe immédiatement
            if (deathSoundInstance.isValid())
            {
                deathSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                deathSoundInstance.release(); // Libère la mémoire du son stoppé
            }

                // On crée une nouvelle instance et on la joue
                deathSoundInstance = FMODUnity.RuntimeManager.CreateInstance(deathSound);
                deathSoundInstance.start();
            

            // --- TÉLÉPORTATION ---
            other.transform.position = respawnPoint.position;
            
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
            }
        }
    }

    // Nettoyage de la mémoire si l'objet est détruit pendant le jeu
    private void OnDestroy()
    {
        if (deathSoundInstance.isValid())
        {
            deathSoundInstance.release();
        }
    }
}