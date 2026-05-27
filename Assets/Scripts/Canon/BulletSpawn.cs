using UnityEngine;

public class BulletSpawn : MonoBehaviour
{   
    [Header("FMOD Sound")]
    [SerializeField] private float maxSoundDistance = 12f; 
    [SerializeField] private FMODUnity.EventReference shootEvent;

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
    
    private Transform playerTransform;
    
    private void Start()
    {
        if (respawnPoint == null)
        {
            GameObject foundRespawn = GameObject.FindWithTag("Respawn Point");
            if (foundRespawn != null) respawnPoint = foundRespawn.transform;
        }

        // --- MODIFICATION : On cherche le Player via son Tag ---
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    void Update()
    {
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

            if (!shootEvent.IsNull)
            {
                if (playerTransform != null)
                {
                    // Calcul de la distance réelle entre le canon et le JOUEUR
                    float distanceToPlayer = Vector3.Distance(bulletSpawn.position, playerTransform.position);

                    if (distanceToPlayer <= maxSoundDistance)
                    {
                        FMODUnity.RuntimeManager.PlayOneShot(shootEvent, bulletSpawn.transform.position);
                    }
                }
                else
                {
                    // Sécurité si le tag Player n'est pas trouvé
                    FMODUnity.RuntimeManager.PlayOneShot(shootEvent, bulletSpawn.transform.position);
                }
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