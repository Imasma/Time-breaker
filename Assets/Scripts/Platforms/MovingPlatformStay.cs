using UnityEngine;

public class MovingPlatformStay : MonoBehaviour
{
    // On rend le mouvement public pour que le joueur puisse le lire
    public Vector3 PlatformMovement { get; private set; }
    
    private Vector3 previousPosition;

    private void Start()
    {
        previousPosition = transform.position;
    }

    private void FixedUpdate()
    {
        // On calcule la distance parcourue à cette frame
        PlatformMovement = transform.position - previousPosition;
        previousPosition = transform.position;
    }
}