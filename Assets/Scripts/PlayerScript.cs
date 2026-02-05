using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 6f;
    public float jumpForce = 7f;
    public float slowTimeScale = 0.1f;

    private Rigidbody rb;
    private bool isGrounded;
    private float moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        bool isMovingInput =
            Input.GetKey(KeyCode.LeftArrow) ||
            Input.GetKey(KeyCode.Q) ||
            Input.GetKey(KeyCode.RightArrow) ||
            Input.GetKey(KeyCode.D) ||
            Input.GetKey(KeyCode.Space);

        // ⏱️ Gestion du temps
        Time.timeScale = isMovingInput ? 1f : slowTimeScale;

        moveInput = 0f;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.Q))
            moveInput = -1f;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            moveInput = 1f;

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void FixedUpdate()
    {
        Vector3 velocity = rb.linearVelocity;
        rb.linearVelocity = new Vector3(moveInput * speed, velocity.y, 0f);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = false;
    }
}
