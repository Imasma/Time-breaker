using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 6f;
    [SerializeField] private float gravityMultiplier = 2f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float slowTimeScale = 0.1f;

    [Header("Detection")]
    [SerializeField] private GroundCheck groundCheck; // Référence groundCheck
    [SerializeField] private WallDetection wallCheck; // Référence wallCheck
    
    private Rigidbody rb;
    private bool moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        
        // On utilise la gravité manuelle via gravityMultiplier
        rb.useGravity = true; 
    }

    void Update()
    {
        // 1. Détection des entrées (Horizontal et Vertical)
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        bool jumpPress = Input.GetButton("Jump");

        // Déclenchement du TimeSlow si aucune touche n'est pressée
        moveInput = (Mathf.Abs(x) > 0.1f || Mathf.Abs(z) > 0.1f || jumpPress);

        // Saut (le booléen isGrounded est maintenant lu directement depuis le script GroundCheck)
        if (Input.GetButtonDown("Jump"))
        {
            if (groundCheck.isGrounded || wallCheck.wallDetected)
            {
                Jump();
            }
        }

        ApplyTimeSlow();
    }

    void FixedUpdate()
    {
        HandleMovement();
        ApplyGravity();
    }

    private void Jump()
    {
        // Reset velocity Y pour un saut propre
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
    
    void HandleMovement()
    {
        // Lecture des axes (lissage automatique pour le déplacement)
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Calcul du vecteur souhaité à partir des axes locaux
        Vector3 moveDirection = transform.right * x + transform.forward * z;
        
        // Clamp pour éviter d'aller plus vite en diagonale (Strafe Boost)
        Vector3 desiredMove = Vector3.ClampMagnitude(moveDirection, 1f) * speed;

        // On conserve la vélocité Y actuelle (gravité/saut)
        float currentY = rb.linearVelocity.y;

        // Application de la vélocité (X et Z calculés, Y préservé)
        rb.linearVelocity = new Vector3(desiredMove.x, currentY, desiredMove.z);
    }
    
    void ApplyGravity()
    {
        // Si on n'est pas au sol, on applique le surplus de gravité
        if (!groundCheck.isGrounded)
        {
            rb.AddForce(Physics.gravity * (gravityMultiplier - 1f), ForceMode.Acceleration);
        }
    }

    void ApplyTimeSlow()
    {
        // Logique "Superhot"
        Time.timeScale = moveInput ? 1f : slowTimeScale;
        
        // Synchronisation de la physique pour éviter les saccades
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }
}