using UnityEngine;
using System.Collections.Generic;

public class Ghost : MonoBehaviour
{
    private List<Vector3> pathReference;
    private int index = 0;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            // On désactive la physique standard pour éviter les conflits pendant le suivi strict
            rb.isKinematic = true; 
        }
    }

    // On reçoit la référence de la liste du joueur
    public void SetPathReference(List<Vector3> recordedPath)
    {
        pathReference = recordedPath;
        index = 0; // On commence à lire la liste depuis le début
    }

    void FixedUpdate()
    {
        if (pathReference == null) return;

        // Tant que le fantôme a des positions en retard par rapport au joueur
        if (index < pathReference.Count)
        {
            transform.position = pathReference[index];
            index++;
        }
        // Si index == pathReference.Count, le clone a rattrapé le joueur. 
        // Il attendra ici que le joueur ajoute une nouvelle position à la frame suivante.
    }
}