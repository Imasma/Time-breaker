using System;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    [SerializeField] private FMODUnity.EventReference setSpawn;
    public GameObject respawnPoint;
    private bool checkpointUsed = false;

    [Header("Animation Settings")]
    public float rotateDuration = 3f;
    public float levitateHeight = 1f;
    public float levitateSpeed = 2f;
    public float rotateSpeed = 180f;

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private bool isAnimating = false;
    private float animationTimer = 0f;

    private void Start()
    {
        respawnPoint = GameObject.FindWithTag("Respawn Point");
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    private void Update()
    {
        if (!isAnimating) return;

        animationTimer += Time.deltaTime;

        // ✅ Rotation continue pendant l'animation
        transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f);

        // ✅ Lévitation avec un sinus
        float newY = initialPosition.y + Mathf.Sin(animationTimer * levitateSpeed) * levitateHeight;
        transform.position = new Vector3(initialPosition.x, newY, initialPosition.z);

        // ✅ Fin de l'animation — retour à la position initiale
        if (animationTimer >= rotateDuration)
        {
            isAnimating = false;
            animationTimer = 0f;
            transform.position = initialPosition;
            transform.rotation = initialRotation;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !checkpointUsed)
        {
            respawnPoint.transform.position = gameObject.transform.position;
            checkpointUsed = !checkpointUsed;
            GetComponent<Renderer>().material.EnableKeyword("_EMISSION");

            FMODUnity.RuntimeManager.PlayOneShot(setSpawn, transform.position);

            // ✅ Lance l'animation
            isAnimating = true;
            animationTimer = 0f;
        }
    }
}