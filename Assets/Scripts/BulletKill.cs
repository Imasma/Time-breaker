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
    }
}