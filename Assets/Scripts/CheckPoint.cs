using System;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    public GameObject respawnPoint;
    private bool checkpointUsed = false;

    private void Start()
    {
        respawnPoint = GameObject.FindWithTag("Respawn Point");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !checkpointUsed)
        {
            respawnPoint.transform.position = gameObject.transform.position;
            checkpointUsed = !checkpointUsed;
            GetComponent<Renderer>().material.EnableKeyword("_EMISSION");        
        }
    }    

}
