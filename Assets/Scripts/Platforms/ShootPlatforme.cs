using System;
using UnityEngine;

public class ShootPlatforme : MonoBehaviour
{
    public float speed;
    private void FixedUpdate()
    {
        float speed = 5f;
        transform.position += transform.forward * speed * Time.fixedDeltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Terrain"))
        {
            Destroy(gameObject);
        }
    }
}
