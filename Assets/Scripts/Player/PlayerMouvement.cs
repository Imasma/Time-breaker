using UnityEngine;
using UnityEngine.InputSystem; 

public class PlayerMovement : MonoBehaviour
{
    [Header("Input System")]
    public InputAction jumpAction; 

    [Header("Movement (Sonic Momentum)")]
    [SerializeField] private float topSpeed = 12f;      // Vitesse maximale
    [SerializeField] private float acceleration = 15f;  // Vitesse à laquelle on atteint le Top Speed
    [SerializeField] private float deceleration = 25f;  // Vitesse de freinage quand on lâche le stick
    [SerializeField] private float turnFriction = 40f;  // Force de freinage quand on fait un demi-tour brusque (Skid)
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
    
    [Header("Slow System (Mode Slow)")]
    [SerializeField] private float slowTimeScale = 0.1f;
    [Range(0.1f, 1f)]
    [SerializeField] private float playerSpeedPercentage = 0.6f; 
    
    [Header("Slow Motion Physics Tweaks")]
    [SerializeField] private float slowJumpBoost = 1f;
    [SerializeField] private float slowGravityBoost = 1f;

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
    }

    void FixedUpdate()
    {
        CheckSlope();
        HandleMovement();
        ApplyGravity();
    }

    public void SetSlowMode(bool active) => isSlowModeActive = active;

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
        // 1. Déterminer la direction voulue par le joueur
        Vector3 inputDirection = (transform.right * inputX + transform.forward * inputZ).normalized;
        
        // 2. Séparer la vélocité horizontale actuelle (on ignore Y pour ne pas perturber les sauts/gravité)
        Vector3 currentVelocity = rb.linearVelocity;
        Vector3 currentHorizontal = new Vector3(currentVelocity.x, 0, currentVelocity.z);
        
        // 3. Calcul de la vitesse maximale ciblée (avec prise en compte du SlowMo)
        float currentTopSpeed = topSpeed;
        if (Time.timeScale < 1f) 
            currentTopSpeed *= (playerSpeedPercentage / slowTimeScale);

        Vector3 targetHorizontal = inputDirection * currentTopSpeed;

        // 4. Déterminer quel taux d'accélération/décélération utiliser
        float currentAccelRate;

        if (!isMoving) 
        {
            // Le joueur lâche le stick : Freinage naturel
            currentAccelRate = deceleration; 
        } 
        else if (Vector3.Dot(currentHorizontal.normalized, inputDirection) < -0.1f) 
        {
            // Le joueur veut aller dans le sens inverse de son élan : Freinage brusque (Skid)
            currentAccelRate = turnFriction; 
        } 
        else 
        {
            // Le joueur accélère normalement
            currentAccelRate = acceleration; 
        }

        // Ajustement du taux d'accélération pour le mode ralenti
        if (Time.timeScale < 1f) 
            currentAccelRate *= (playerSpeedPercentage / slowTimeScale);

        // 5. Appliquer l'inertie mathématiquement (MoveTowards rapproche doucement la vitesse actuelle de la cible)
        Vector3 newHorizontal = Vector3.MoveTowards(currentHorizontal, targetHorizontal, currentAccelRate * Time.fixedDeltaTime);

        // 6. Assigner la vélocité finale au Rigidbody
        if (IsSliding())
        {
            Vector3 slopeDirection = Vector3.ProjectOnPlane(Vector3.down, hitNormal).normalized;
            rb.linearVelocity = new Vector3(slopeDirection.x * slideSpeed, rb.linearVelocity.y, slopeDirection.z * slideSpeed);
        }
        else
        {
            rb.linearVelocity = new Vector3(newHorizontal.x, currentVelocity.y, newHorizontal.z);
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
        bool shouldBeSlow = isSlowModeActive && !isGhostActive && !isMoving;

        if (shouldBeSlow != wasActuallySlow)
        {
            float ratio = playerSpeedPercentage / slowTimeScale;

            if (shouldBeSlow)
            {
                Time.timeScale = slowTimeScale;
                rb.linearVelocity *= ratio;
            }
            else
            {
                Time.timeScale = 1f;
                rb.linearVelocity /= ratio;
            }

            Time.fixedDeltaTime = 0.02f * Time.timeScale;
            wasActuallySlow = shouldBeSlow;
        }

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

    private bool IsSliding() => groundCheck.isGrounded && Vector3.Angle(Vector3.up, hitNormal) > slopeLimit;
    
    public bool IsSlowModeActive() => wasActuallySlow;
}