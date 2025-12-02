using UnityEngine;

public class FreeLookController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float rotationSpeed = 3f;

    private float rotationX = 0f;
    private float rotationY = 0f;

    void Update()
    {
        // --- 1. 회전 (마우스) ---
        if (Input.GetMouseButton(1)) // 마우스 오른쪽 버튼을 누른 상태에서만 회전
        {
            rotationX += Input.GetAxis("Mouse X") * rotationSpeed;
            rotationY -= Input.GetAxis("Mouse Y") * rotationSpeed;
            rotationY = Mathf.Clamp(rotationY, -90f, 90f); // Y축 회전 제한

            transform.rotation = Quaternion.Euler(rotationY, rotationX, 0f);
        }

        // --- 2. 이동 (키보드) ---
        Vector3 inputMove = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) inputMove += Vector3.forward;
        if (Input.GetKey(KeyCode.S)) inputMove += Vector3.back;
        if (Input.GetKey(KeyCode.A)) inputMove += Vector3.left;
        if (Input.GetKey(KeyCode.D)) inputMove += Vector3.right;
        if (Input.GetKey(KeyCode.Q)) inputMove += Vector3.down;
        if (Input.GetKey(KeyCode.E)) inputMove += Vector3.up;

        transform.Translate(inputMove.normalized * moveSpeed * Time.deltaTime);
    }
}