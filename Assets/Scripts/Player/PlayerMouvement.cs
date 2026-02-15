using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 6f;
    [SerializeField] private float gravityMultiplier = 2f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float slowTimeScale = 0.1f;
    
    [Header("WallJump")]
    [SerializeField] private Vector3 wallJumpPower = new Vector3(6f, 12f, 0f);

    [Header("Detection")]
    [SerializeField] private GroundCheck groundCheck; 
    [SerializeField] private WallDetection wallCheck; 
    
    [Header("WallSlide")] 
    // On définit une vitesse cible (ex: -2f) plutôt qu'un multiplicateur complexe
    [SerializeField] private float wallSlideMaxSpeed = 2f; 
    
    [Header("Slow System")]
    [SerializeField] private float maxSlowEnergy = 5f;
    [SerializeField] private float drainSpeed = 1f;
    [SerializeField] private float refillSpeed = 2f;
    [SerializeField] private float slowPlayerBoost = 1.5f; 
    
    public float currentSlowEnergy;

    private Rigidbody rb;
    private bool moveInput;
    private float inputX;
    private float inputZ;
    private bool isJumpingInput; // Nouvelle variable pour détecter si Espace est maintenu

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.useGravity = true; 
        
        currentSlowEnergy = maxSlowEnergy; 
    }

    void Update()
    {
        // 1. Détection des entrées
        inputX = Input.GetAxisRaw("Horizontal");
        inputZ = Input.GetAxisRaw("Vertical");
        bool jumpPress = Input.GetButtonDown("Jump");
        
        // On vérifie si la touche saut (Espace) est maintenue enfoncée
        isJumpingInput = Input.GetButton("Jump");

        // On garde moveInput uniquement pour le calcul de la direction dans HandleMovement
        moveInput = (Mathf.Abs(inputX) > 0.1f || Mathf.Abs(inputZ) > 0.1f);

        if (jumpPress)
        {
            if (groundCheck.isGrounded) Jump();
            else if (wallCheck.wallDetected && !groundCheck.isGrounded) WallJump();
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
        float finalJumpSpeed = jumpForce;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, finalJumpSpeed, rb.linearVelocity.z);
    }
    
    private void WallJump()
    {
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(new Vector3(0, wallJumpPower.y, 0), ForceMode.Impulse);
    }
    
    void HandleMovement()
    {
        // On calcule la direction de mouvement
        Vector3 moveDirection = (transform.right * inputX + transform.forward * inputZ).normalized;
        
        float finalSpeed = speed;

        // On applique le boost si le slow est activé
        if (Time.timeScale < 1f) finalSpeed *= slowPlayerBoost;

        // Application directe de la vitesse (basé sur ta fonction) : 
        // On définit la vélocité X et Z sans toucher au Y (gravité/saut)
        rb.linearVelocity = new Vector3(moveDirection.x * finalSpeed, rb.linearVelocity.y, moveDirection.z * finalSpeed);
    }
    
    void ApplyGravity()
    {
        if (!groundCheck.isGrounded)
        {
            if (wallCheck.wallDetected && rb.linearVelocity.y < 0)
            {
                //Vitesse vers le bas
                float targetSlideVelocity = -wallSlideMaxSpeed;
                
                // force pour atteindre cette vitesse de glisse
                float slideVelocityChange = targetSlideVelocity - rb.linearVelocity.y;
                rb.AddForce(Vector3.up * slideVelocityChange, ForceMode.VelocityChange);
            }
            else
            {
                rb.AddForce(Physics.gravity * (gravityMultiplier - 1f), ForceMode.Acceleration);
            }
        }
    }

    void ApplyTimeSlow()
    {
        // On vérifie si le personnage a de l'inertie
        bool hasInertia = rb.linearVelocity.magnitude > 0.1f;  // 0.1f pour ignorer les micro-vibrations parce que c'est chiant

        // Si le joueur bouge, a de l'énergie ET ne maintient pas Espace
        if (hasInertia && currentSlowEnergy > 0f && !isJumpingInput) 
        {
            Time.timeScale = slowTimeScale;
            
            // vide la barre 
           // currentSlowEnergy -= drainSpeed * Time.unscaledDeltaTime;
        }
        else 
        {
            // Si le joueur est à l'arrêt, n'a plus d'énergie OU maintient Espace
            Time.timeScale = 1f;
            
            // On remplit la barre
            currentSlowEnergy += refillSpeed * Time.unscaledDeltaTime;
        }

        // enlève le slow de force si l'énergie atteint zéro (sécurité)
        if (currentSlowEnergy <= 0.01f) 
        {
            Time.timeScale = 1f;
        }
        // pour ne pas dépasser 0 ou le max
        currentSlowEnergy = Mathf.Clamp(currentSlowEnergy, 0f, maxSlowEnergy);
        
        // On lisse le changement de fixedDeltaTime
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }
}