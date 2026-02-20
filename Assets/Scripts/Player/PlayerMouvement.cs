using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 6f;
    [SerializeField] private float gravityMultiplier = 2f;
    [SerializeField] private float jumpForce = 7f;
    
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
    public InputAction toggleSlowAction; // Touche pour activer le mode slow
    [SerializeField] private float slowTimeScale = 0.1f; // Slow de l'environnement
    [Range(0.1f, 1f)]
    [Tooltip("Définit la vitesse globale du joueur en slow-mo (ex: 0.6 = le joueur fait tout à 60% de sa vitesse normale)")]
    [SerializeField] private float playerSpeedPercentage = 0.6f; 
    
    [Header("Slow Motion Physics Tweaks")]
    [Tooltip("Ajustement manuel de la hauteur du saut en Slow-Mo (1 = hauteur mathématiquement identique au mode normal)")]
    [SerializeField] private float slowJumpBoost = 1f; 
    [Tooltip("Ajustement manuel de la lourdeur de la chute en Slow-Mo (1 = chute mathématiquement identique au mode normal)")]
    [SerializeField] private float slowGravityBoost = 1f; 
    
    private bool isSlowModeActive = false; // Si vrai, on est en mode Slow. Si faux, mode Clone.
    [HideInInspector] public bool isGhostActive = false; // Géré par ModesGestion

    private Rigidbody rb;
    private float inputX;
    private float inputZ;

    private void OnEnable()
    {
        toggleSlowAction.Enable();
        toggleSlowAction.performed += _ => ToggleSlowMode();
    }

    private void OnDisable()
    {
        toggleSlowAction.Disable();
        toggleSlowAction.performed -= _ => ToggleSlowMode();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        // On DÉSACTIVE la gravité de base de Unity pour la gérer nous-mêmes de façon 100% précise
        rb.useGravity = false; 
    }

    void Update()
    {
        // 1. Détection des entrées
        inputX = Input.GetAxisRaw("Horizontal");
        inputZ = Input.GetAxisRaw("Vertical");
        bool jumpPress = Input.GetButtonDown("Jump");

        if (jumpPress)
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

    private void ToggleSlowMode()
    {
        // On ne change de mode que si un fantôme n'est pas déjà déployé
        if (!isGhostActive)
        {
            isSlowModeActive = !isSlowModeActive;
        }
    }

    // Utilisé par ModesGestion pour forcer le retour en mode Clone
    public void SetSlowMode(bool active)
    {
        isSlowModeActive = active;
    }

    private void Jump()
    {
        float finalJumpSpeed = jumpForce;

        if (Time.timeScale < 1f)
        {
            /* MATHS DU SAUT :
               Pour que le saut garde sa hauteur mais paraisse plus lent, on utilise le ratio de vitesse souhaité.
               Ratio = (VitesseJoueurVoulue / VitesseMondeActuelle).
               On multiplie la force par ce ratio pour que l'impulsion initiale compense exactement le slow du moteur.
            */
            float speedCompensation = playerSpeedPercentage / Time.timeScale;
            finalJumpSpeed *= speedCompensation;
            
            // Tweak : appliquer ton multiplicateur personnel depuis l'inspecteur
            finalJumpSpeed *= slowJumpBoost;
        }

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, finalJumpSpeed, rb.linearVelocity.z);
    }
    
    private void WallJump()
    {
        rb.linearVelocity = Vector3.zero;
        float finalWallJumpY = wallJumpPower.y;

        if (Time.timeScale < 1f)
        {
            // Pareil pour le saut contre le mur : harmonisation avec le pourcentage de vitesse du joueur
            float speedCompensation = playerSpeedPercentage / Time.timeScale;
            finalWallJumpY *= speedCompensation;
            finalWallJumpY *= slowJumpBoost;
        }

        rb.AddForce(new Vector3(0, finalWallJumpY, 0), ForceMode.Impulse);
    }
    
    void HandleMovement()
    {
        Vector3 moveDirection = (transform.right * inputX + transform.forward * inputZ).normalized;
        float finalSpeed = speed;

        // Si le temps est ralenti, on applique la compensation pour que le joueur soit "moins slow"
        if (Time.timeScale < 1f) 
        {
            // Calcul : On multiplie par (Pourcentage voulu / TimeScale actuel)
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
        // Gravité personnalisée
        Vector3 customGravity = Physics.gravity * gravityMultiplier;

        if (Time.timeScale < 1f)
        {
            /* MATHS DE LA GRAVITÉ :
               La gravité est une accélération (m/s²). Puisque le temps (dt) intervient deux fois dans 
               le calcul de la position (une fois pour la vitesse, une fois pour le déplacement),
               on doit appliquer le ratio de compensation AU CARRÉ.
               Cela permet au joueur de tomber à la même hauteur, mais avec une vitesse synchronisée 
               sur son 'playerSpeedPercentage'.
            */
            float speedCompensation = playerSpeedPercentage / Time.timeScale;
            customGravity *= (speedCompensation * speedCompensation);
            
            // Tweak : appliquer ton multiplicateur personnel pour ajuster le "poids" ressenti
            customGravity *= slowGravityBoost;
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
                // Application manuelle de notre gravité modifiée puisque rb.useGravity est False
                rb.AddForce(customGravity, ForceMode.Acceleration);
            }
        }
    }

    void ApplyTimeSlow()
    {
        // On applique le slow seulement si le mode est actif ET qu'aucun fantôme n'est déployé
        if (isSlowModeActive && !isGhostActive) 
        {
            Time.timeScale = slowTimeScale;
        }
        else  
        {
            Time.timeScale = 1f;
        }

        // Ajustement indispensable du delta physique (FixedUpdate) pour garder une simulation fluide et stable
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
}