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

        // Désactivation de TOUS les colliders (Sphere Collider, etc.) pour être immatériel
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
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

        // Suivi du chemin enregistré
        if (index < pathReference.Count)
        {
            transform.position = pathReference[index];
            index++;
        }
    }
}