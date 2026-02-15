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

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.useGravity = true; 
        
        // CONSEIL : Dans l'inspecteur, règle "Interpolate" sur "None" 
        // pour éviter les catapultages lors des changements de TimeScale.
        
        currentSlowEnergy = maxSlowEnergy; 
    }

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        bool jumpPress = Input.GetButtonDown("Jump");

        moveInput = (Mathf.Abs(x) > 0.1f || Mathf.Abs(z) > 0.1f);

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
        // Utilisation d'un Vector3 pour la force (X pour s'écarter, Y pour monter)
        rb.AddForce(new Vector3(0, wallJumpPower.y, 0), ForceMode.Impulse);
    }
    
    void HandleMovement()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 moveDirection = transform.right * x + transform.forward * z;
        float finalSpeed = speed;

        if (Time.timeScale < 1f) finalSpeed *= slowPlayerBoost;

        Vector3 targetVelocity = Vector3.ClampMagnitude(moveDirection, 1f) * finalSpeed;
        
        // CALCUL DE LA FORCE : Au lieu de forcer rb.linearVelocity, 
        // on calcule la différence pour atteindre la vitesse cible.
        Vector3 velocityChange = (targetVelocity - new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z));
        rb.AddForce(new Vector3(velocityChange.x, 0, velocityChange.z), ForceMode.VelocityChange);
    }
    
    void ApplyGravity()
    {
        if (!groundCheck.isGrounded)
        {
            if (wallCheck.wallDetected && rb.linearVelocity.y < 0)
            {
                // CORRECTION WALL SLIDE : Vitesse constante vers le bas
                float targetSlideVelocity = -wallSlideMaxSpeed;
                
                // On applique une force pour atteindre cette vitesse de glisse
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
        if (moveInput && currentSlowEnergy > 0f) //si on bouge et qu'on à l'énergie
        {
            Time.timeScale = slowTimeScale;
            currentSlowEnergy -= drainSpeed * Time.unscaledDeltaTime;
        }
        else if (!moveInput ) // si on bouge pas
        {
            Time.timeScale = 1f;
            currentSlowEnergy += refillSpeed * Time.unscaledDeltaTime;
        }
        else if (currentSlowEnergy == 0f) //enlève le slow si y a plus d'énergie
        {
            Time.timeScale = 1f;
        }

        currentSlowEnergy = Mathf.Clamp(currentSlowEnergy, 0f, maxSlowEnergy);
        
        // On lisse le changement de fixedDeltaTime
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }
}