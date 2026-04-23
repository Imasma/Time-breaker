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
    private bool isMoving; 

    [Header("Slope Slide")]
    [SerializeField] private float slopeLimit = 45f;
    [SerializeField] private float slideSpeed = 10f;
    private Vector3 hitNormal;
    
    [Header("WallJump")]
    [SerializeField] private Vector3 wallJumpPower = new Vector3(6f, 12f, 0f);

    [Header("Detection")]
    [SerializeField] private GroundCheck groundCheck; 
    [SerializeField] private WallDetection wallCheck; 
    
    [Header("WallSlide")] 
    [SerializeField] private float wallSlideMaxSpeed = 2f; 
    
    [Header("Slow System (Ephémère avec Durée)")]
    [Range(0.1f, 1f)] [SerializeField] private float slowTimeScale = 0.25f;
    [Range(0.1f, 1f)] [SerializeField] private float playerSpeedPercentage = 0.2f; 
    [SerializeField] private float recoveryDuration = 2.0f; // Temps en secondes pour revenir à 1.0

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
            if (groundCheck.isGrounded) Jump();
            else if (wallCheck.wallDetected && !groundCheck.isGrounded) WallJump();
        }
        
        ApplyTimeSlow();
        Debug.Log(Time.timeScale);
    }

    void FixedUpdate()
    {
        CheckSlope();
        HandleMovement();
        ApplyGravity();

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
        bool shouldBeSlow = isSlowModeActive && !isGhostActive && !isMoving;

        if (shouldBeSlow)
        {
            if (!wasActuallySlow)
            {
                Time.timeScale = slowTimeScale;
                wasActuallySlow = true;
            }

            // CALCUL DE LA VITESSE DE RÉCUPÉRATION
            // Vitesse = Distance (1.0 - slowScale) / Temps voulu
            float step = (1f - slowTimeScale) / recoveryDuration;
            
            Time.timeScale = Mathf.MoveTowards(Time.timeScale, 1f, Time.unscaledDeltaTime * step);
        }
        else
        {
            Time.timeScale = 1f;
            wasActuallySlow = false;
        }

        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }

    // --- LE RESTE DU SCRIPT RESTE IDENTIQUE ---

    void HandleMovement()
    {
        Vector3 inputDirection = (transform.right * inputX + transform.forward * inputZ).normalized;
        Vector3 currentVelocity = rb.linearVelocity;
        Vector3 currentHorizontal = new Vector3(currentVelocity.x, 0, currentVelocity.z);
        
        float speedMult = GetDynamicSpeedMultiplier();
        float currentTopSpeed = topSpeed * speedMult;
        float currentAccelRate;

        if (!isMoving) currentAccelRate = deceleration * speedMult;
        else if (Vector3.Dot(currentHorizontal.normalized, inputDirection) < -0.1f) currentAccelRate = turnFriction * speedMult;
        else currentAccelRate = acceleration * speedMult;

        Vector3 targetHorizontal = inputDirection * currentTopSpeed;
        Vector3 newHorizontal = Vector3.MoveTowards(currentHorizontal, targetHorizontal, currentAccelRate * Time.fixedDeltaTime);

        if (IsSliding())
        {
            Vector3 slopeDirection = Vector3.ProjectOnPlane(Vector3.down, hitNormal).normalized;
            rb.linearVelocity = new Vector3(slopeDirection.x * slideSpeed * speedMult, rb.linearVelocity.y, slopeDirection.z * slideSpeed * speedMult);
        }
        else
        {
            rb.linearVelocity = new Vector3(newHorizontal.x, currentVelocity.y, newHorizontal.z);
        }
    }
    
    void ApplyGravity()
    {
        float speedMult = GetDynamicSpeedMultiplier();
        Vector3 customGravity = Physics.gravity * gravityMultiplier;
        customGravity *= (speedMult * speedMult) * slowGravityBoost;

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
                rb.AddForce(customGravity, ForceMode.Acceleration);
            }
        }
    }

    private void Jump()
    {
        float speedMult = GetDynamicSpeedMultiplier();
        float finalJumpSpeed = jumpForce * speedMult * slowJumpBoost;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, finalJumpSpeed, rb.linearVelocity.z);
    }
    
    private void WallJump()
    {
        rb.linearVelocity = Vector3.zero;
        float speedMult = GetDynamicSpeedMultiplier();
        float finalWallJumpY = wallJumpPower.y * speedMult * slowJumpBoost;
        rb.AddForce(new Vector3(0, finalWallJumpY, 0), ForceMode.Impulse);
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