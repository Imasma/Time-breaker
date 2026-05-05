using UnityEngine;

public class BulletSpawn : MonoBehaviour
{   
    [Header("FMOD Sound")]
    [SerializeField] private FMODUnity.EventReference shootEvent; // Glisse ton event FMOD ici

    [Header("Paramètres de Tir")]
    [SerializeField] private bool shootBullet = true; 
    [SerializeField] private float cooldown = 2f;
    private float t;

    [Header("Prefabs")]
    [SerializeField] private GameObject bulletPrefab; 
    [SerializeField] private GameObject platformPrefab;
    
    [Header("Configuration")]
    [SerializeField] private Transform bulletSpawn; 
    [SerializeField] private Transform respawnPoint; 
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float platformSpeed = 5f;
    
    private void Start()
    {
        if (respawnPoint == null)
        {
            GameObject foundRespawn = GameObject.FindWithTag("Respawn Point");
            if (foundRespawn != null) respawnPoint = foundRespawn.transform;
        }
    }

    void Update()
    {
        // On met à jour le paramètre global de temps FMOD ici pour être sûr
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("GameTimeScale", Time.timeScale);

        t -= Time.deltaTime;
        if (t <= 0f)
        { 
            t = cooldown;
            SpawnObject();
        }
    }

    private void SpawnObject()
    {
        GameObject objectToSpawn;
        Quaternion spawnRotation;

        if (shootBullet)
        {
            objectToSpawn = bulletPrefab;
            spawnRotation = bulletSpawn.rotation;

            // --- JOUER LE SON SPATIALISÉ ---
            // On joue le son attaché au "bulletSpawn" pour qu'il vienne du canon
            if (!shootEvent.IsNull)
            {
                //FMODUnity.RuntimeManager.PlayOneShot(shootEvent, bulletSpawn.transform.position);
                
            }
        }
        else
        {
            objectToSpawn = platformPrefab;
            spawnRotation = Quaternion.Euler(0, bulletSpawn.eulerAngles.y, 0);
        }

        GameObject newObj = Instantiate(objectToSpawn, bulletSpawn.position, spawnRotation);
    
        if (newObj.TryGetComponent<ShootPlatforme>(out ShootPlatforme platformScript))
        {
            platformScript.speed = this.platformSpeed;
            platformScript.moveDirection = bulletSpawn.forward; 
        }
    
        if (newObj.TryGetComponent<BulletKill>(out BulletKill bulletScript))
        {
            bulletScript.respawnPoint = this.respawnPoint;
        }

        Rigidbody rb = newObj.GetComponent<Rigidbody>();
        if (rb != null && shootBullet)
        {
            rb.linearVelocity = bulletSpawn.forward * bulletSpeed;
        }
    }
}