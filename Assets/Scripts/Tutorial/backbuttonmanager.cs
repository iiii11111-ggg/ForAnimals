using UnityEngine;

public class BackButtonManager : MonoBehaviour
{
    public GameObject optionUI; // �ɼ� UI GameObject �Ҵ�

    void Update()
    {
        // �ȵ���̵��� Back ��ư �Է� ���� (�Ǵ� �����Ϳ��� Escape)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // �ɼ� UI�� ��Ȱ��ȭ ���¸� Ȱ��ȭ
            if (!optionUI.activeSelf)
            {
                optionUI.SetActive(true);
            }
            else
            {
                // �ɼ� UI�� �̹� ���� ������ ����
                optionUI.SetActive(false);
            }
        }
    }
}

