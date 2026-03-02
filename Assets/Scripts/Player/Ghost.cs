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
            rb.isKinematic = true; 
        }
    }

    public void SetPathReference(List<Vector3> recordedPath)
    {
        pathReference = recordedPath;
        index = 0; 
    }

    void FixedUpdate()
    {
        if (pathReference == null) return;

        // Le fantôme parcourt la liste des positions enregistrées par le joueur
        if (index < pathReference.Count)
        {
            transform.position = pathReference[index];
            index++;
        }
        // Si l'index rattrape la fin de la liste, il attend que le joueur bouge 
        // pour que ModesGestion.cs ajoute de nouvelles positions dans FixedUpdate.
    }
}