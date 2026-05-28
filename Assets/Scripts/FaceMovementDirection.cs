using UnityEngine;

public class FaceMovementDirection : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 10f;
    public float movementThreshold = 0.1f;

    [Header("References")]
    public Transform avatar;

    [Header("Model Offset")]
    public float modelYOffset = 180f;

    private Rigidbody rb;
    private bool hasMovedOnce = false;
    private Vector3 lastValidDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (avatar == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
                avatar = player.transform;
        }

        transform.rotation = Quaternion.identity * Quaternion.Euler(0f, modelYOffset, 0f);
    }

    void Update()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputZ = Input.GetAxisRaw("Vertical");
        Vector3 inputDirection = new Vector3(inputX, 0f, inputZ).normalized;
        float inputMagnitude = inputDirection.magnitude;

        Quaternion targetRotation;

        if (inputMagnitude > movementThreshold)
        {
            hasMovedOnce = true;
            lastValidDirection = inputDirection;
            targetRotation = Quaternion.LookRotation(lastValidDirection);
        }
        else
        {
            if (!hasMovedOnce) return;

            if (avatar != null)
            {
                Vector3 dirToAvatar = avatar.position - transform.position;
                dirToAvatar.y = 0f;

                if (dirToAvatar != Vector3.zero)
                {
                    float angle = Vector3.SignedAngle(
                        lastValidDirection,
                        dirToAvatar.normalized,
                        Vector3.up
                    );
                    angle = Mathf.Clamp(angle, -89f, 89f);
                    targetRotation = Quaternion.LookRotation(lastValidDirection)
                                     * Quaternion.Euler(0f, angle, 0f);
                }
                else
                {
                    targetRotation = Quaternion.LookRotation(lastValidDirection);
                }
            }
            else
            {
                targetRotation = Quaternion.LookRotation(lastValidDirection);
            }
        }

        targetRotation *= Quaternion.Euler(0f, modelYOffset, 0f);

        Quaternion newRotation = Quaternion.Lerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );

        // ✅ CLAMP FINAL — on vérifie l'angle par rapport à lastValidDirection
        // après le lerp, avant d'appliquer la rotation
        Vector3 newForward = newRotation * Vector3.forward;
        Vector3 referenceForward = Quaternion.LookRotation(lastValidDirection)
                                   * Quaternion.Euler(0f, modelYOffset, 0f)
                                   * Vector3.forward;

        float finalAngle = Vector3.SignedAngle(referenceForward, newForward, Vector3.up);

        if (Mathf.Abs(finalAngle) > 89f)
        {
            // ✅ On force la rotation à rester dans les limites
            float clampedAngle = Mathf.Clamp(finalAngle, -89f, 89f);
            newRotation = Quaternion.LookRotation(lastValidDirection)
                          * Quaternion.Euler(0f, modelYOffset, 0f)
                          * Quaternion.Euler(0f, clampedAngle, 0f);
        }

        transform.rotation = newRotation;
    }
}