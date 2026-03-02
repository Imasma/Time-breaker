using UnityEngine;

public class BulletSpawn : MonoBehaviour
{   
    [SerializeField] private float cooldown = 2f;
    private float t;
    [SerializeField] private GameObject bulletGO;
    [SerializeField] private Transform bulletSpawn; 
    [SerializeField] private Transform respawnPoint; 
    
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float bulletLife = 2f;
    
    private void Start()
    {
        
        // On cherche l'objet qui possède le tag exact
        GameObject foundRespawn = GameObject.FindWithTag("Respawn Point");

        if (foundRespawn != null)
        { 
            respawnPoint = foundRespawn.transform;
        }

        
    }
    void Update()
    {
        t -= Time.deltaTime;
        if (t <= 0f)
        { 
            t = cooldown;
            SpawnBullet();
        }
    }

    private void SpawnBullet()
    {
        // 1. On crée la balle
        GameObject newBullet = Instantiate(bulletGO, bulletSpawn.transform.position, bulletSpawn.transform.rotation);
        
        // récupère le script BulletKill sur la balle qu'on vient de créer
        BulletKill bulletScript = newBullet.GetComponent<BulletKill>();
        
        //assigne le respawn point
        bulletScript.respawnPoint = this.respawnPoint;
        
        // gère la physique
        Rigidbody rb = newBullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = bulletSpawn.transform.forward * bulletSpeed;
        }
        
        Destroy(newBullet, bulletLife); 
    }
}