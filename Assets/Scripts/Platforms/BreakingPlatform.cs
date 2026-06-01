using System.Collections;
using UnityEngine;

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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(playerTag) && !isBroken)
        {
            // ✅ Lance les particules dès que le joueur touche la plateforme
            if (breakParticles != null)
            {
                breakParticles.gameObject.SetActive(true);
                breakParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                breakParticles.Play();
            }

            StartCoroutine(BreakSequence());
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        // ✅ Stop les particules si le joueur quitte avant la cassure
        if (collision.gameObject.CompareTag(playerTag) && !isBroken)
        {
            if (breakParticles != null)
                breakParticles.Stop();
        }
    }

    private IEnumerator BreakSequence()
    {
        isBroken = true;

        // ✅ Pendant le breakDelay le joueur voit les particules
        yield return new WaitForSeconds(breakDelay);

        // ✅ Stop les particules puis casse la plateforme
        if (breakParticles != null)
            breakParticles.Stop();

        if (platformGO != null)
            platformGO.SetActive(false);

        yield return new WaitForSeconds(respawnDelay);

        if (platformGO != null)
            platformGO.SetActive(true);

        isBroken = false;
    }
}