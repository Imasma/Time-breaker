using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 6f;
    [SerializeField] private float gravityMultiplier = 2f;
    [SerializeField] private float jumpForce = 7f;
    private bool isMoving;

    [Header("Slope Slide")]
    [SerializeField] private float slopeLimit = 45f;    // Angle max avant de glisser
    [SerializeField] private float slideSpeed = 10f;   // Vitesse de la glissade
    private Vector3 hitNormal;                         // Direction de la pente
    
    [Header("WallJump")]
    [SerializeField] private Vector3 wallJumpPower = new Vector3(6f, 12f, 0f);

    [Header("Detection")]
    [SerializeField] private GroundCheck groundCheck; 
    [SerializeField] private WallDetection wallCheck;
    
    [Header("WallSlide")] 
    [SerializeField] private float wallSlideMaxSpeed = 2f;
    
    [Header("Slow System (Mode Slow)")]
    [SerializeField] private float slowTimeScale = 0.1f; // Slow de l'environnement
    [Range(0.1f, 1f)]
    [Tooltip("Définit la vitesse globale du joueur en slow-mo (ex: 0.6 = le joueur fait tout à 60% de sa vitesse normale)")]
    [SerializeField] private float playerSpeedPercentage = 0.6f;
    [SerializeField] private float slowApplyCD = 0.5f; // Délai de stabilisation
    
    [Header("Slow Motion Physics Tweaks")]
    [Tooltip("Ajustement manuel de la hauteur du saut en Slow-Mo (1 = hauteur mathématiquement identique au mode normal)")]
    [SerializeField] private float slowJumpBoost = 1f;
    [Tooltip("Ajustement manuel de la lourdeur de la chute en Slow-Mo (1 = chute mathématiquement identique au mode normal)")]
    [SerializeField] private float slowGravityBoost = 1f;

    private bool isSlowModeActive = true; // Par défaut True (Mode Orange au départ)
    private bool wasActuallySlow = false; // Suivi de l'état réel du TimeScale pour la compensation
    [HideInInspector] public bool isGhostActive = false; // Géré par ModesGestion

    private Rigidbody rb;
    private float inputX;
    private float inputZ;

    // Timer interne pour gérer le cooldown de transition
    private float slowTransitionTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        // On DÉSACTIVE la gravité de base de Unity pour la gérer nous-mêmes
        rb.useGravity = false;
    }

    void Update()
    {
        // 1. Détection des entrées
        inputX = Input.GetAxisRaw("Horizontal");
        inputZ = Input.GetAxisRaw("Vertical");

        // Détection du mouvement sur les deux axes
        isMoving = (Mathf.Abs(inputX) > 0.1f || Mathf.Abs(inputZ) > 0.1f);

        bool jumpPress = Input.GetButtonDown("Jump");
        if (jumpPress)
        {
            if (groundCheck.isGrounded) Jump();
            else if (wallCheck.wallDetected && !groundCheck.isGrounded) WallJump();
        }
        
        // Gère le TimeScale et ajuste la vélocité si le temps change
        ApplyTimeSlow();
    }

    void FixedUpdate()
    {
        CheckSlope();
        HandleMovement();
        ApplyGravity();
    }

    public void SetSlowMode(bool active)
    {
        isSlowModeActive = active;
    }

    private void Jump()
    {
        float finalJumpSpeed = jumpForce;
        if (Time.timeScale < 1f)
        {
            float speedCompensation = playerSpeedPercentage / Time.timeScale;
            finalJumpSpeed *= speedCompensation * slowJumpBoost;
        }

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, finalJumpSpeed, rb.linearVelocity.z);
    }
    
    private void WallJump()
    {
        rb.linearVelocity = Vector3.zero;
        float finalWallJumpY = wallJumpPower.y;

        if (Time.timeScale < 1f)
        {
            float speedCompensation = playerSpeedPercentage / Time.timeScale;
            finalWallJumpY *= speedCompensation * slowJumpBoost;
        }

        rb.AddForce(new Vector3(0, finalWallJumpY, 0), ForceMode.Impulse);
    }
    
    void HandleMovement()
    {
        Vector3 moveDirection = (transform.right * inputX + transform.forward * inputZ).normalized;
        float finalSpeed = speed;

        if (Time.timeScale < 1f) 
        {
            finalSpeed *= (playerSpeedPercentage / slowTimeScale);
        }

        if (IsSliding())
        {
            Vector3 slopeDirection = Vector3.ProjectOnPlane(Vector3.down, hitNormal).normalized;
            rb.linearVelocity = new Vector3(slopeDirection.x * slideSpeed, rb.linearVelocity.y, slopeDirection.z * slideSpeed);
        }
        else
        {
            rb.linearVelocity = new Vector3(moveDirection.x * finalSpeed, rb.linearVelocity.y, moveDirection.z * finalSpeed);
        }
    }
    
    void ApplyGravity()
    {
        Vector3 customGravity = Physics.gravity * gravityMultiplier;
        if (Time.timeScale < 1f)
        {
            float speedCompensation = playerSpeedPercentage / Time.timeScale;
            customGravity *= (speedCompensation * speedCompensation) * slowGravityBoost;
        }

        if (!groundCheck.isGrounded)
        {
            if (wallCheck.wallDetected && rb.linearVelocity.y < 0)
            {
                float targetSlideVelocity = -wallSlideMaxSpeed;
                float slideVelocityChange = targetSlideVelocity - rb.linearVelocity.y;
                rb.AddForce(Vector3.up * slideVelocityChange, ForceMode.VelocityChange);
            }
            else
            {
                rb.AddForce(customGravity, ForceMode.Acceleration);
            }
        }
    }

    void ApplyTimeSlow()
    {
        // État souhaité : lent si mode orange actif, pas de fantôme et joueur immobile 
        bool targetSlowState = isSlowModeActive && !isGhostActive && !isMoving;

        // Si l'état souhaité diffère de l'état actuel
        if (targetSlowState != wasActuallySlow)
        {
            // Utilisation de unscaledDeltaTime pour un cooldown constant même en slow-mo 
            slowTransitionTimer += Time.unscaledDeltaTime;

            // On n'applique le changement que si l'état est stable pendant slowApplyCD 
            if (slowTransitionTimer >= slowApplyCD)
            {
                float ratio = playerSpeedPercentage / slowTimeScale;

                if (targetSlowState)
                {
                    Time.timeScale = slowTimeScale;
                    rb.linearVelocity *= ratio; // Compensation de vélocité pour éviter de tomber 
                }
                else
                {
                    Time.timeScale = 1f;
                    rb.linearVelocity /= ratio; // Évite la propulsion brusque à la reprise 
                }

                Time.fixedDeltaTime = 0.02f * Time.timeScale;
                wasActuallySlow = targetSlowState;
                slowTransitionTimer = 0f;
            }
        }
        else
        {
            // Si le mouvement reprend avant la fin du cooldown, on annule le changement
            slowTransitionTimer = 0f;
        }

        // Maintien des valeurs de temps
        Time.timeScale = wasActuallySlow ? slowTimeScale : 1f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }
    
    private void CheckSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1.5f))
            hitNormal = hit.normal;
        else
            hitNormal = Vector3.up;
    }

    private bool IsSliding()
    {
        return groundCheck.isGrounded && Vector3.Angle(Vector3.up, hitNormal) > slopeLimit;
    }
    
    public bool IsSlowModeActive()
    {
        return wasActuallySlow;
    }
}