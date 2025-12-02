using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro 필수

public class SaveSlotUI : MonoBehaviour
{
    [Header("설정")]
    public int slotIndex = 1;

    [Header("UI 연결")]
    public Button loadButton;
    public TextMeshProUGUI slotText;
    public Button deleteButton;

    [Header("팝업 연결")]
    public GameObject deleteConfirmPopup;
    public Button popupYesButton;
    public Button popupNoButton;

    // 📌 [추가] 원래 폰트 크기를 기억할 변수
    private float defaultFontSize;

    private void Awake()
    {
        // 게임 시작 시, 인스펙터에 설정된 폰트 크기를 '기본값'으로 저장해둡니다.
        if (slotText != null)
        {
            defaultFontSize = slotText.fontSize;
        }
    }

    private void Start()
    {
        UpdateSlotUI();

        // 삭제 버튼 이벤트 자동 연결
        if (deleteButton != null)
        {
            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(OnClickDeleteButton);
        }
    }

    public void UpdateSlotUI()
    {
        string sceneName;
        bool hasSave = SaveManager.Instance.CheckSaveFileExists(slotIndex, out sceneName);

        if (hasSave)
        {
            // ✅ 저장 데이터가 있을 때
            slotText.text = sceneName;

            // 📌 [추가] 폰트 크기를 기본값보다 20% 키움 (1.2배)
            slotText.fontSize = defaultFontSize * 1.2f;

            // 텍스트 색상도 조금 더 진하게 하고 싶다면 아래 주석 해제 (선택사항)
            // slotText.color = Color.white; 

            if (deleteButton != null)
                deleteButton.gameObject.SetActive(true);

            loadButton.onClick.RemoveAllListeners();
            loadButton.onClick.AddListener(() => GameManager.Instance.LoadGameSlot(slotIndex));
        }
        else
        {
            // ✅ 저장 데이터가 없을 때 (Empty)
            slotText.text = "Empty";

            // 📌 [추가] 폰트 크기를 다시 원래대로 복구
            slotText.fontSize = defaultFontSize;

            // 색상을 조금 어둡게 하고 싶다면 아래 주석 해제 (선택사항)
            // slotText.color = Color.gray;

            if (deleteButton != null)
                deleteButton.gameObject.SetActive(false);

            loadButton.onClick.RemoveAllListeners();
            loadButton.onClick.AddListener(() => GameManager.Instance.LoadGameSlot(slotIndex));
        }
    }

    public void OnClickDeleteButton()
    {
        if (deleteConfirmPopup != null)
        {
            deleteConfirmPopup.SetActive(true);

            if (popupYesButton != null)
            {
                popupYesButton.onClick.RemoveAllListeners();
                popupYesButton.onClick.AddListener(ConfirmDelete);
            }

            if (popupNoButton != null)
            {
                popupNoButton.onClick.RemoveAllListeners();
                popupNoButton.onClick.AddListener(() => deleteConfirmPopup.SetActive(false));
            }
        }
    }

    private void ConfirmDelete()
    {
        SaveManager.Instance.DeleteSaveData(slotIndex);
        UpdateSlotUI(); // UI 갱신 (폰트 크기도 같이 줄어듦)
        deleteConfirmPopup.SetActive(false);
    }
}