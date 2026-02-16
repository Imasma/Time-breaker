using UnityEngine;

public class BasculeStay : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private Transform platform;

    private Vector3 originalScale;
    private Quaternion originalRotation;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(playerTag))
        {
            // 1. Sauvegarde et Parentage
            originalScale = other.transform.localScale;
            originalRotation = other.transform.localRotation;
            other.transform.SetParent(platform, true);

            // 2. ON FORCE LE GROUNDED
            // On cherche le GroundCheck (souvent sur un enfant "GroundCheck" ou sur le Player)
            GroundCheck gc = other.GetComponentInChildren<GroundCheck>();
            if (gc != null) gc.isOnBascule = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag(playerTag))
        {
            // 1. Libération et remise à l'échelle
            other.transform.SetParent(null, true);
            other.transform.localScale = originalScale;
            other.transform.localRotation = originalRotation;

            // 2. ON RETIRE LE FORCE GROUNDED
            GroundCheck gc = other.GetComponentInChildren<GroundCheck>();
            if (gc != null) gc.isOnBascule = false;
        }    
    }
}