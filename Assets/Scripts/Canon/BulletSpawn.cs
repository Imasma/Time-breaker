using UnityEngine;

public class BulletSpawn : MonoBehaviour
{   
    [SerializeField] private float cooldown = 2f;
    private float t;
    [SerializeField] private GameObject bulletGO; // Ton prefab (Balle ou Plateforme)
    [SerializeField] private Transform bulletSpawn; 
    [SerializeField] private Transform respawnPoint; 
    
    [SerializeField] private float bulletSpeed = 20f;
    
    [Header("Platformes")]
    public float speed;
    
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
            SpawnBullet();
        }
    }

    private void SpawnBullet()
    {
        // 1. Création de l'objet (Balle, Plateforme, etc.)
        GameObject newBullet = Instantiate(bulletGO, bulletSpawn.position, bulletSpawn.rotation);
        
        // 2. MODIFICATION : On vérifie si l'objet est une "Balle tueuse" (BulletKill)
        // TryGetComponent permet d'éviter l'erreur si le script est absent
        if (newBullet.TryGetComponent<BulletKill>(out BulletKill bulletScript))
        {
            bulletScript.respawnPoint = this.respawnPoint;
        }
        else if (newBullet.TryGetComponent<ShootPlatforme>(out ShootPlatforme shootPlatformeScript))
        {
            shootPlatformeScript.speed = this.speed;
        }
        
        // 3. Gestion de la physique
        Rigidbody rb = newBullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = bulletSpawn.forward * bulletSpeed;
        }
        
        // 4. Destruction
    }
}