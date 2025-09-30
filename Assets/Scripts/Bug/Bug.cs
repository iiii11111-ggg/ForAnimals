using UnityEngine;

public class Bug : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    private Camera cam;

    void Start()
    {
        direction = Random.insideUnitCircle.normalized; // 랜덤한 초기 방향 설정
        speed = Random.Range(0.5f, 1.5f);
        cam = Camera.main;

        // 초기 방향에 맞춰 오브젝트 회전
        UpdateRotation();
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        // 이동 방향이 변경될 때마다 오브젝트 회전 업데이트
        UpdateRotation();

        KeepInsideScreen();
    }

    void UpdateRotation()
    {
        // 이동 방향(direction)에 따라 X축 회전 각도를 계산합니다.
        // Atan2(y, x)는 x축(+)을 0도로 기준으로 시계 반대 방향으로 증가하는 각도(-180 ~ 180)를 반환합니다.
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 주어진 기준에 맞춰 X축 회전을 보정합니다:
        // direction이 아래 (0, -1)일 때 angle은 -90도, X축은 0도여야 함.
        // direction이 오른쪽 (1, 0)일 때 angle은 0도, X축은 90도여야 함.
        // 따라서 angle 값에 90도를 더하면 이 관계를 만족합니다.
        float targetXRotation = angle + 90f;

        // X축 회전만 변경하고, Y축과 Z축 회전은 고정된 값으로 설정합니다.
        // 스크린샷에서 Y: -90, Z: 39.183 이었고, 이제 Z는 45로 고정하셨으니 반영합니다.
        transform.rotation = Quaternion.Euler(targetXRotation, -90f, 45f);
    }

    void KeepInsideScreen()
    {
        Vector3 pos = transform.position;

        Vector3 viewportPos = cam.WorldToViewportPoint(pos);

        if (viewportPos.x <= 0f || viewportPos.x >= 1f)
        {
            direction.x = -direction.x;
            pos.x = Mathf.Clamp(pos.x, cam.ViewportToWorldPoint(new Vector3(0.05f, 0, 0)).x,
                                         cam.ViewportToWorldPoint(new Vector3(0.95f, 0, 0)).x);
            transform.position = pos;
            UpdateRotation();
        }

        if (viewportPos.y <= 0f || viewportPos.y >= 1f)
        {
            direction.y = -direction.y;
            pos.y = Mathf.Clamp(pos.y, cam.ViewportToWorldPoint(new Vector3(0, 0.05f, 0)).y,
                                         cam.ViewportToWorldPoint(new Vector3(0, 0.95f, 0)).y);
            transform.position = pos;
            UpdateRotation();
        }
    }

    void OnMouseDown()
    {
        BugManager.Instance.AddScore(1);
        Destroy(gameObject);
    }
}