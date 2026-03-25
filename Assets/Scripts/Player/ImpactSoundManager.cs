using UnityEngine;
using FMODUnity; // Indispensable

public class ImpactSoundManagerFMOD : MonoBehaviour
{
    [Header("Scripts References")]
    [SerializeField] private GroundCheck groundCheck;
    [SerializeField] private WallDetection wallDetection;

    [Header("FMOD Settings")]
    [SerializeField] private EventReference collisionEvent;

    private bool wasGrounded;
    private bool wasWallDetected;

    private void Update()
    {
        // Sécurité : si les scripts ne sont pas assignés, on ne fait rien
        if (groundCheck == null || wallDetection == null) return;

        bool isGroundedNow = groundCheck.isGrounded;
        bool isWallDetectedNow = wallDetection.wallDetected;

        // Détection de l'atterrissage
        if (isGroundedNow && !wasGrounded)
        {
            PlayImpactSound();
        }
        
        // Détection de l'impact mur
        if (isWallDetectedNow && !wasWallDetected)
        {
            PlayImpactSound();
        }

        wasGrounded = isGroundedNow;
        wasWallDetected = isWallDetectedNow;
    }

    private void PlayImpactSound()
    {
        if (!collisionEvent.IsNull)
        {
            RuntimeManager.PlayOneShot(collisionEvent, transform.position);
            Debug.Log("Son d'impact FMOD joué !");
        }
    }
}