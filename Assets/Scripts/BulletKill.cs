using UnityEngine;

public class BulletKill : MonoBehaviour
{
    public Transform respawnPoint;
    public Cheat Cheat;
    private void Awake()
    {
        // On trouve l'objet avec le tag "Player", puis on récupère son script Cheat
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Cheat = player.GetComponent<Cheat>();
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && Cheat.godMode == false)
        {
            other.transform.position = respawnPoint.position;
        
            Rigidbody rb = other.GetComponent<Rigidbody>();
            
            rb.linearVelocity = Vector3.zero;
            
        }

        if (other.CompareTag("Clone") || other.gameObject.layer == LayerMask.NameToLayer("Terrain"))
        {
            Destroy(gameObject);
        }
    }
}