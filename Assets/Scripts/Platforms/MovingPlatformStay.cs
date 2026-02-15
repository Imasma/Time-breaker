using System;
using UnityEngine;

public class MovingPlatformStay : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private Transform platform;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(playerTag))
        {
            //on met le joueur en parent
            other.gameObject.transform.parent = platform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag(playerTag))
        {
            //on met le joueur en parent
            other.gameObject.transform.parent = null;
        }    }
}
