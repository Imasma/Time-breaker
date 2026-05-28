using UnityEngine;

public class BulletKill : MonoBehaviour
{
    [SerializeField] private FMODUnity.EventReference deathSound;
    private FMOD.Studio.EventInstance deathSoundInstance;

    public Transform respawnPoint;
    public Cheat Cheat;

    private void Awake()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) Cheat = player.GetComponent<Cheat>();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && Cheat.godMode == false)
        {
            // --- LOGIQUE SON ---
                deathSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                deathSoundInstance.release();
            
            deathSoundInstance = FMODUnity.RuntimeManager.CreateInstance(deathSound);
            deathSoundInstance.start();

            // --- TÉLÉPORTATION ---
            other.transform.position = respawnPoint.position;
            Rigidbody rb = other.GetComponent<Rigidbody>();
            rb.linearVelocity = Vector3.zero;
        }

        if (other.CompareTag("Clone") || other.gameObject.layer == LayerMask.NameToLayer("Terrain"))
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (deathSoundInstance.isValid()) deathSoundInstance.release();
    }
}