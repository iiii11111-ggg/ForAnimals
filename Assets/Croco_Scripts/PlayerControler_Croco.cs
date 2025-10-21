using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController_Croco : MonoBehaviour
{
    public float moveSpeed = 6f;
    Rigidbody rb;
    Vector3 moveInput;

    public Animator an;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // 입력 (WASD 또는 화살표)
        float h = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right
        float v = Input.GetAxisRaw("Vertical");    // W/S or Up/Down
        moveInput = new Vector3(h, 0f, v).normalized;
    }

    void FixedUpdate()
    {
        Vector3 velocity = moveInput * moveSpeed;
        rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);

        // 바라보는 방향
        // moveInput의 크기가 0.001f보다 크면(입력이 있으면) 캐릭터가 이동 중으로 간주
        bool isMoving = moveInput.sqrMagnitude > 0.001f;

        if (isMoving)
        {
            transform.forward = Vector3.Slerp(transform.forward, moveInput, 0.2f);
        }

        // 💡 애니메이터 로직: isWalking bool 변수 설정 💡
        if (an != null)
        {
            an.SetBool("isRunning", isMoving);
        }
    }
}