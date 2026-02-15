using UnityEngine;

public class BulletSpawn : MonoBehaviour
{   
    [SerializeField] private float cooldown = 2f;
    private float t;
    [SerializeField] private GameObject bulletGO;
    
    [SerializeField] private Transform respawnPoint; 
    
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float bulletLife = 2f;
    
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
        GameObject newBullet = Instantiate(bulletGO, transform.position, transform.rotation);
        
        // récupère le script BulletKill sur la balle qu'on vient de créer
        BulletKill bulletScript = newBullet.GetComponent<BulletKill>();
        
        //assigne le respawn point
        bulletScript.respawnPoint = this.respawnPoint;
        
        // gère la physique
        Rigidbody rb = newBullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = transform.forward * bulletSpeed;
        }
        
        Destroy(newBullet, 2f); 
    }
}