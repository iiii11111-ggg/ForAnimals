using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public Rigidbody rb;

    public float moveSpeed = 8f;
    public float turnSpeed = 15f;

    public Animator an;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        an = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        an.SetBool("isJumping", false);
        Movement();
    }

    public void Movement() 
    {
        float xInput = Input.GetAxis("Horizontal");
        float zInput = Input.GetAxis("Vertical");


        Vector3 moveDirection = new Vector3(xInput, 0f, zInput);


        if (moveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);

            an.SetBool("isRunning", true);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
        }
        else 
        {
            an.SetBool("isRunning", false);
        }

            // --- 5. �̵� �ӵ� ���� ---
            float xSpeed = xInput * moveSpeed;
        float zSpeed = zInput * moveSpeed;
        rb.linearVelocity = new Vector3(xSpeed, rb.linearVelocity.y, zSpeed);
    }

}
