using UnityEngine;

public class FaceMovementDirection : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 10f;
    public float movementThreshold = 0.1f;

    [Header("References")]
    public Transform avatar;

    private Rigidbody rb;
    private bool hasMovedOnce = false;
    private Vector3 lastValidDirection;

    // ✅ Rotation de base — face caméra
    private Quaternion baseRotation;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (avatar == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
                avatar = player.transform;
        }

        // ✅ On mémorise la rotation initiale comme référence absolue
        baseRotation = transform.rotation;
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
            lastValidDirection = -inputDirection;
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

        Quaternion newRotation = Quaternion.Lerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );

        // ✅ Clamp par rapport à la rotation de base (face caméra)
        // On extrait l'angle Y de la nouvelle rotation
        float yAngle = newRotation.eulerAngles.y;

        // Convertir en -180/+180
        if (yAngle > 180f) yAngle -= 360f;

        // ✅ Bloquer entre -90° et +90° — jamais dos à la caméra
        yAngle = Mathf.Clamp(yAngle, -80f, 80f);

        transform.rotation = Quaternion.Euler(0f, yAngle, 0f);
    }
}