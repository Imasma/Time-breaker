using System;
using UnityEngine;

public class WallDetection : MonoBehaviour
{
    [SerializeField] private float wallRayLength = 0.5f;
    [SerializeField] private float wallRayOffSet = 0.1f;
    
    private SphereCollider sphere;
    
    public bool wallDetected = false;
    public float wallDirection = 0f; // 1 pour mur à droite, -1 pour mur à gauche

    private void Start()
    {
        sphere = GetComponent<SphereCollider>(); 
    }

    private void Update()
    {
        RayWallDetection();
    }

    private void RayWallDetection()
    {
        wallDetected = false; 

        Vector3 center = transform.position + sphere.center;
        float radius = sphere.radius * transform.lossyScale.y; 
        float totalDist = radius + wallRayLength;

        float[] yOffsets = {
            radius - wallRayOffSet,
            0f,
            -radius + wallRayOffSet
        };
        
        foreach (float yOffset in yOffsets)
        {
            Vector3 rayOrigin = center + Vector3.up * yOffset;

            // On check à Gauche
            if (CheckWall(rayOrigin, Vector3.left, totalDist)) 
            {
                wallDetected = true;
                wallDirection = -1f; // Mur à gauche
            }
            // On check à Droite
            if (CheckWall(rayOrigin, Vector3.right, totalDist)) 
            {
                wallDetected = true;
                wallDirection = 1f; // Mur à droite
            }
        }
    }

    private bool CheckWall(Vector3 origin, Vector3 direction, float distance)
    {
        RaycastHit hit;
        Debug.DrawRay(origin, direction * distance, Color.blue);

        if (Physics.Raycast(origin, direction, out hit, distance))
        {
            // MODIFICATION : On vérifie si c'est un Mur OU un Clone
            if (hit.collider.CompareTag("Wall") || hit.collider.CompareTag("Clone"))
            {
                Debug.DrawRay(hit.point, hit.normal, Color.yellow);
                return true;
            }
        }
        return false;
    }
}