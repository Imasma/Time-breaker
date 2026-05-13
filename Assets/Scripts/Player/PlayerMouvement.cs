using UnityEngine;
using UnityEngine.InputSystem; 

public class PlayerMovement : MonoBehaviour
{
    [Header("Input System")]
    public InputAction jumpAction; 

    [Header("Movement (Sonic Momentum)")]
    [SerializeField] private float topSpeed = 12f;      
    [SerializeField] private float acceleration = 15f;  
    [SerializeField] private float deceleration = 25f;  
    [SerializeField] private float turnFriction = 40f;  
    [SerializeField] private float gravityMultiplier = 2f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float secondJumpForce = 4f;
    [SerializeField] private float fastFallMultiplier = 3f;
    private bool isMoving; 

    [Header("Slope Slide")]
    [SerializeField] private float slopeLimit = 45f;
    [SerializeField] private float slideSpeed = 10f;
    private Vector3 hitNormal;
    
    [Header("WallJump & DoubleJump")]
    [SerializeField] private Vector3 wallJumpPower = new Vector3(6f, 12f, 0f);
    private bool canDoubleJump;

    [Header("Detection")]
    [SerializeField] private GroundCheck groundCheck; 
    [SerializeField] private WallDetection wallCheck; 
    
    [Header("WallSlide")] 
    [SerializeField] private float wallSlideMaxSpeed = 2f; 
    
    [Header("Slow System (Ephémère avec Durée)")]
    [Range(0.1f, 1f)] [SerializeField] private float slowTimeScale = 0.25f;
    [Range(0.1f, 1f)] [SerializeField] private float playerSpeedPercentage = 0.2f; 

    [Header("Slow Motion Physics Tweaks")]
    [SerializeField] private float slowJumpBoost = 1.1f;
    [SerializeField] private float slowGravityBoost = 1f;

    [Header("Platforms")]
    [SerializeField] private string movingPlatformTag = "MovingPlatform";
    private MovingPlatformStay currentPlatform;

    private bool isSlowModeActive = true; 
    private bool wasActuallySlow = false;
    [HideInInspector] public bool isGhostActive = false;

    private Rigidbody rb;
    private float inputX;
    private float inputZ;

    private void OnEnable() => jumpAction.Enable();
    private void OnDisable() => jumpAction.Disable();

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

        isMoving = (Mathf.Abs(inputX) > 0.1f || Mathf.Abs(inputZ) > 0.1f);

        if (jumpAction.triggered)
        {
            if (groundCheck.isGrounded) 
            {
                Jump(jumpForce);
            }
            else if (wallCheck.wallDetected) 
            {
                WallJump();
            }
            else if (canDoubleJump) 
            {
                Jump(secondJumpForce);
                canDoubleJump = false;
            }
        }
        
        ApplyTimeSlow();
    }

    void FixedUpdate()
    {
        CheckSlope();
        HandleMovement();
        ApplyGravity();

        if (groundCheck.isGrounded || wallCheck.wallDetected)
        {
            canDoubleJump = true;
        }

        if (currentPlatform != null)
        {
            rb.MovePosition(rb.position + currentPlatform.PlatformMovement);
        }
    }

    private float GetDynamicSpeedMultiplier()
    {
        if (slowTimeScale >= 0.99f) return 1f;
        float targetRatio = playerSpeedPercentage / slowTimeScale;
        float t = (1f - Time.timeScale) / (1f - slowTimeScale);
        return Mathf.Lerp(1f, targetRatio, Mathf.Clamp01(t));
    }

    void ApplyTimeSlow()
    {
        // On vérifie si on doit être au ralenti
        bool shouldBeSlow = isSlowModeActive && !isGhostActive && !isMoving;

        if (shouldBeSlow)
        {
            // Le temps reste bloqué sur ta valeur slowTimeScale (ex: 0.25)
            Time.timeScale = slowTimeScale;
            wasActuallySlow = true;
        }
        else
        {
            // Dès qu'on bouge ou que le mode est off, le temps redevient normal (1.0)
            Time.timeScale = 1f;
            wasActuallySlow = false;
        }

        // Toujours mettre à jour le fixedDeltaTime pour garder une physique fluide
        Time.fixedDeltaTime = Mathf.Max(0.005f, 0.02f * Time.timeScale);
    }

    void HandleMovement()
    {
        Vector3 inputDirection = (transform.right * inputX + transform.forward * inputZ).normalized;
        Vector3 currentVelocity = rb.linearVelocity;
        Vector3 currentHorizontal = new Vector3(currentVelocity.x, 0, currentVelocity.z);
    
        float speedMult = GetDynamicSpeedMultiplier();
        float targetMaxSpeed = topSpeed * speedMult;
        float currentAccelRate;

        if (!isMoving) 
        {
            // FIX : Si on est en l'air, on n'applique PAS de décélération horizontale
            // On garde l'élan (Momentum)
            currentAccelRate = groundCheck.isGrounded ? (deceleration * speedMult) : 0f; 
        }
        else if (Vector3.Dot(currentHorizontal.normalized, inputDirection) < -0.1f) 
        {
            currentAccelRate = turnFriction * speedMult;
        }
        else 
        {
            currentAccelRate = acceleration * speedMult;
        }

        Vector3 targetHorizontal = inputDirection * targetMaxSpeed;
        Vector3 newHorizontal;

        // Si on lâche l'input en l'air, on garde la vélocité actuelle intacte
        if (!isMoving && !groundCheck.isGrounded)
        {
            newHorizontal = currentHorizontal;
        }
        else if (isMoving && currentHorizontal.magnitude > targetMaxSpeed + 0.1f)
        {
            float frictionSouple = deceleration * 0.1f;
            newHorizontal = Vector3.MoveTowards(currentHorizontal, targetHorizontal, frictionSouple * Time.fixedDeltaTime);
        }
        else
        {
            newHorizontal = Vector3.MoveTowards(currentHorizontal, targetHorizontal, currentAccelRate * Time.fixedDeltaTime);
        }

        // Application finale (reste inchangé)
        if (IsSliding())
        {
            Vector3 slopeDirection = Vector3.ProjectOnPlane(Vector3.down, hitNormal).normalized;
            // FIX : On modifie X et Z, mais on garde rb.linearVelocity.y en temps RÉEL
            rb.linearVelocity = new Vector3(
                slopeDirection.x * slideSpeed * speedMult, 
                rb.linearVelocity.y, 
                slopeDirection.z * slideSpeed * speedMult
            );
        }
        else
        {
            // FIX CRITIQUE : Ne pas utiliser 'currentVelocity.y' stocké au début.
            // On injecte directement la vélocité horizontale calculée tout en laissant
            // l'axe Y vivre sa vie (saut/gravité).
            rb.linearVelocity = new Vector3(newHorizontal.x, rb.linearVelocity.y, newHorizontal.z);
        }
    }
    
    void ApplyGravity()
    {
        float speedMult = GetDynamicSpeedMultiplier();
        float currentGravityMultiplier = gravityMultiplier;

        if (!groundCheck.isGrounded && inputZ < -0.1f)
        {
            currentGravityMultiplier *= fastFallMultiplier;
        }

        Vector3 customGravity = Physics.gravity * currentGravityMultiplier;
    
        // FIX : On retire le (speedMult * speedMult). 
        // On garde juste slowGravityBoost si tu veux ajuster manuellement le feeling.
        customGravity *= slowGravityBoost; 

        if (!groundCheck.isGrounded)
        {
            if (wallCheck.wallDetected && rb.linearVelocity.y < 0)
            {
                float targetSlideVelocity = -wallSlideMaxSpeed * speedMult;
                float slideVelocityChange = targetSlideVelocity - rb.linearVelocity.y;
                rb.AddForce(Vector3.up * slideVelocityChange, ForceMode.VelocityChange);
            }
            else
            {
                // Acceleration est déjà scalé par le temps via le moteur physique
                rb.AddForce(customGravity, ForceMode.Acceleration);
            }
        }
    }

    private void Jump(float force)
    {
        float finalJumpSpeed = force;

        if (wasActuallySlow)
        {
            finalJumpSpeed = force * slowJumpBoost;
        }
        else 
        {
            finalJumpSpeed = force;
        }

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, finalJumpSpeed, rb.linearVelocity.z);
    }
    
    private void WallJump()
    {
        rb.linearVelocity = Vector3.zero;
    
        // On retire speedMult ici aussi
        float finalWallJumpY = wallJumpPower.y * slowJumpBoost;
        rb.AddForce(new Vector3(0, finalWallJumpY, 0), ForceMode.Impulse);
    
        canDoubleJump = true;
    }

    public void SetSlowMode(bool active) => isSlowModeActive = active;
    public bool IsSlowModeActive() => wasActuallySlow;

    private void CheckSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1.5f))
            hitNormal = hit.normal;
        else
            hitNormal = Vector3.up;
    }

    private bool IsSliding() => groundCheck.isGrounded && Vector3.Angle(Vector3.up, hitNormal) > slopeLimit;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(movingPlatformTag))
            currentPlatform = other.GetComponent<MovingPlatformStay>();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(movingPlatformTag))
            currentPlatform = null;
    }
}   