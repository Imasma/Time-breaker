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
    [SerializeField] private float playerSpeedPercentage = 0.6f; 
    [SerializeField] private float slowApplyCD = 0.5f; 
    
    [Header("Slow Motion Physics Tweaks")]
    [SerializeField] private float slowJumpBoost = 1f;
    [SerializeField] private float slowGravityBoost = 1f;

    private bool isSlowModeActive = true; 
    private bool wasActuallySlow = false;
    [HideInInspector] public bool isGhostActive = false;

    private Rigidbody rb;
    private float inputX;
    private float inputZ;

    // Timer pour gérer le cooldown et éviter les micro-freezes
    private float slowTransitionTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.useGravity = false;
    }

    void Update()
    {
        inputX = Input.GetAxisRaw("Horizontal");
        inputZ = Input.GetAxisRaw("Vertical");

        // On vérifie si le joueur appuie sur une touche de mouvement
        isMoving = (Mathf.Abs(inputX) > 0.1f || Mathf.Abs(inputZ) > 0.1f);

        if (Input.GetButtonDown("Jump"))
        {
            if (groundCheck.isGrounded) Jump();
            else if (wallCheck.wallDetected && !groundCheck.isGrounded) WallJump();
        }
        
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
        // On veut du slow si : Mode Orange ET Pas de fantôme ET Immobile
        bool shouldBeSlow = isSlowModeActive && !isGhostActive && !isMoving;

        if (shouldBeSlow != wasActuallySlow)
        {
            // On compte le temps réel écoulé (indépendant du TimeScale)
            slowTransitionTimer += Time.unscaledDeltaTime;

            // Si le changement d'état (immobile ou en mouvement) est maintenu assez longtemps
            if (slowTransitionTimer >= slowApplyCD)
            {
                float ratio = playerSpeedPercentage / slowTimeScale;

                if (shouldBeSlow)
                {
                    Time.timeScale = slowTimeScale;
                    rb.linearVelocity *= ratio; // Évite la chute brusque
                }
                else
                {
                    Time.timeScale = 1f;
                    rb.linearVelocity /= ratio; // Évite l'effet fusée
                }

                Time.fixedDeltaTime = 0.02f * Time.timeScale;
                wasActuallySlow = shouldBeSlow;
                slowTransitionTimer = 0f;
            }
        }
        else
        {
            // Si le joueur recommence à bouger avant la fin du cooldown, on reset le timer
            slowTransitionTimer = 0f;
        }

        // Sécurité pour forcer le TimeScale actuel
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