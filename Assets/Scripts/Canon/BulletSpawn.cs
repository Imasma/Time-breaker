using UnityEngine;

public class BulletSpawn : MonoBehaviour
{   
    [Header("Paramètres de Tir")]
    [SerializeField] private bool shootBullet = true; // Le Toggle
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
        }
        else
        {
            objectToSpawn = platformPrefab;
            // On garde la plateforme plate (X et Z à 0)
            spawnRotation = Quaternion.Euler(0, bulletSpawn.eulerAngles.y, 0);
        }

        GameObject newObj = Instantiate(objectToSpawn, bulletSpawn.position, spawnRotation);
    
        // CONFIGURATION DE LA PLATEFORME
        if (newObj.TryGetComponent<ShootPlatforme>(out ShootPlatforme platformScript))
        {
            platformScript.speed = this.platformSpeed;
            // ON ENVOIE LA DIRECTION RÉELLE DU CANON ICI :
            platformScript.moveDirection = bulletSpawn.forward; 
        }
    
        if (newObj.TryGetComponent<BulletKill>(out BulletKill bulletScript))
        {
            bulletScript.respawnPoint = this.respawnPoint;
        }

        // On peut désactiver la vélocité Rigidbody pour les plateformes 
        // car le script ShootPlatforme gère maintenant tout le mouvement.
        Rigidbody rb = newObj.GetComponent<Rigidbody>();
        if (rb != null && shootBullet)
        {
            rb.linearVelocity = bulletSpawn.forward * bulletSpeed;
        }
    
    }
}