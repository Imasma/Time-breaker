using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class BreakablePlatform : MonoBehaviour
{
    [Header("Références")]
    [Tooltip("Cube qui doit disparaître.")]
    [SerializeField] private GameObject platformGO;

    [Tooltip("Glissez ici l'objet Particle System.")]
    [SerializeField] private ParticleSystem breakParticles;

    [Header("Réglages")]
    [SerializeField] private float breakDelay = 0.2f;
    [SerializeField] private float respawnDelay = 3.0f;
    [SerializeField] private string playerTag = "Player";

    private bool isBroken = false;

    // Note : OnCollisionEnter fonctionne sur le parent si l'enfant a le Collider 
    // et qu'aucun des deux n'a de Rigidbody, ou si le Rigidbody est sur le parent.
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(playerTag) && !isBroken)
        {
            StartCoroutine(BreakSequence());
        }
    }

    private IEnumerator BreakSequence()
    {
        isBroken = true;

        // 1. Attente avant la cassure
        yield return new WaitForSeconds(breakDelay);

        // 2. Particules
        if (breakParticles != null)
        {
            breakParticles.Play();
        }

        // 3. On désactive l'objet référencé (le cube enfant)
        if (platformGO != null)
        {
            platformGO.SetActive(false);
        }

        // 4. Temps de recharge
        yield return new WaitForSeconds(respawnDelay);

        // 5. On réactive le cube enfant
        if (platformGO != null)
        {
            platformGO.SetActive(true);
        }
        
        isBroken = false; 
    }
}