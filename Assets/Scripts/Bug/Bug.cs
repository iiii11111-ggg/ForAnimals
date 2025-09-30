using UnityEngine;

public class Bug : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    private Camera cam;

    void Start()
    {
        direction = Random.insideUnitCircle.normalized; // ������ �ʱ� ���� ����
        speed = Random.Range(0.5f, 1.5f);
        cam = Camera.main;

        // �ʱ� ���⿡ ���� ������Ʈ ȸ��
        UpdateRotation();
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        // �̵� ������ ����� ������ ������Ʈ ȸ�� ������Ʈ
        UpdateRotation();

        KeepInsideScreen();
    }

    void UpdateRotation()
    {
        // �̵� ����(direction)�� ���� X�� ȸ�� ������ ����մϴ�.
        // Atan2(y, x)�� x��(+)�� 0���� �������� �ð� �ݴ� �������� �����ϴ� ����(-180 ~ 180)�� ��ȯ�մϴ�.
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // �־��� ���ؿ� ���� X�� ȸ���� �����մϴ�:
        // direction�� �Ʒ� (0, -1)�� �� angle�� -90��, X���� 0������ ��.
        // direction�� ������ (1, 0)�� �� angle�� 0��, X���� 90������ ��.
        // ���� angle ���� 90���� ���ϸ� �� ���踦 �����մϴ�.
        float targetXRotation = angle + 90f;

        // X�� ȸ���� �����ϰ�, Y��� Z�� ȸ���� ������ ������ �����մϴ�.
        // ��ũ�������� Y: -90, Z: 39.183 �̾���, ���� Z�� 45�� �����ϼ����� �ݿ��մϴ�.
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