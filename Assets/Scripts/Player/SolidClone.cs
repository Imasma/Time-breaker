using UnityEngine;
using UnityEngine.Playables;
using System.Collections;

public class SolidClone : MonoBehaviour
{
    [Header("Animation")]
    public PlayableDirector disappearanceTimeline; // Glissez votre Timeline ici
    
    private ModesGestion manager;

    void Start()
    {
        // On récupère le gestionnaire pour pouvoir se retirer de sa liste à la fin
        manager = GameObject.FindGameObjectWithTag("Player").GetComponent<ModesGestion>();
    }

    public void StartLifeTimer(float delay)
    {
        StartCoroutine(LifeRoutine(delay));
    }

    private IEnumerator LifeRoutine(float delay)
    {
        // Attendre 7 secondes
        yield return new WaitForSeconds(delay);

        // Lancer la Timeline de disparition
        if (disappearanceTimeline != null)
        {
            disappearanceTimeline.Play();
            // Attendre la fin de la timeline avant de détruire l'objet
            yield return new WaitForSeconds((float)disappearanceTimeline.duration);
        }

        // Prévenir le manager et détruire
        if (manager != null) manager.RemoveCloneFromList(this.gameObject);
        Destroy(gameObject);
    }
}