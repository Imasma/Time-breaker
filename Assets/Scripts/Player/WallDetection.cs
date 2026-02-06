using System;
using UnityEngine;

public class WallDetection : MonoBehaviour
{
    [SerializeField] private float wallRayLength = 0.5f;
    [SerializeField] private float wallRayOffSet = 0.1f;
    
    private SphereCollider sphere;
    
    public bool wallDetected = false;


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

            // On check à Gauche et à Droite pour chaque hauteur
            if (CheckWall(rayOrigin, Vector3.left, totalDist)) wallDetected = true;
            if (CheckWall(rayOrigin, Vector3.right, totalDist)) wallDetected = true;
        }

        Debug.Log(wallDetected);
    }

    private bool CheckWall(Vector3 origin, Vector3 direction, float distance)
    {
        RaycastHit hit;
        // Debug visuel pour voir les rayons dans la scène (Bleu)
        Debug.DrawRay(origin, direction * distance, Color.blue);

        if (Physics.Raycast(origin, direction, out hit, distance))
        {
            if (hit.collider.CompareTag("Wall"))
            {
                // Debug visuel si un mur est touché (Jaune)
                Debug.DrawRay(hit.point, hit.normal, Color.yellow);
                return true;
            }
        }
        return false;
    }
}