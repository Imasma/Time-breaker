using UnityEngine;

public class BulletKill : MonoBehaviour
{
    public Transform respawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
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