using UnityEngine;
using System.Collections;

public class SolidClone : MonoBehaviour
{
    private ModesGestion manager;

    void Start()
    {
        // On cherche le joueur par son Tag
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player != null)
        {
            manager = player.GetComponent<ModesGestion>();
        }
        else
        {
            Debug.LogError("SolidClone : Aucun objet avec le tag 'Player' n'a été trouvé !");
        }
    }

    public void StartLifeTimer(float delay)
    {
        Debug.Log("SolidClone : Timer lancé pour " + delay + " secondes.");
        StartCoroutine(LifeRoutine(delay));
    }

    private IEnumerator LifeRoutine(float delay)
    {
        // On utilise le temps réel pour être sûr que le slow-mo n'arrête pas le chrono
        yield return new WaitForSecondsRealtime(delay);

        Debug.Log("SolidClone : Temps écoulé, destruction en cours...");

        if (manager != null)
        {
            manager.RemoveCloneFromList(this.gameObject);
        }

        Destroy(gameObject);
    }
}