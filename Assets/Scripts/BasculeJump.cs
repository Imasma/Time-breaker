using UnityEngine;

public class BasculeJump : MonoBehaviour
{
    public GameObject basculeGO;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        rb.MoveRotation(basculeGO.transform.rotation);
    }
}