using UnityEngine;
using System.Collections.Generic;

public class Ghost : MonoBehaviour
{
    private List<Vector3> pathReference;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true; 

        // On désactive les collisions pour ne pas pousser le joueur
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders) col.enabled = false;
    }

    public void SetPathReference(List<Vector3> recordedPath)
    {
        pathReference = recordedPath;
    }

    void FixedUpdate()
    {
        // Si la liste existe et n'est pas vide
        if (pathReference != null && pathReference.Count > 0)
        {
            // Le fantôme se place TOUJOURS sur le point le plus vieux (index 0)
            // Comme ModesGestion supprime le 0 à chaque frame, le 0 suivant devient la nouvelle cible
            transform.position = pathReference[0];

            // Optionnel : Pour que le fantôme regarde vers le point suivant (plus naturel)
            if (pathReference.Count > 1)
            {
                transform.LookAt(pathReference[1]);
            }
        }
    }
}