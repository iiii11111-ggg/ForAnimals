using UnityEditor;
using UnityEngine;

public class SaveDataEditor : MonoBehaviour
{
    [MenuItem("Tools/Save Data/Clear Save Data")]
    public static void ClearSaveData()
    {
        // 씬에 있는 SaveManager를 찾아서 데이터 삭제 함수를 호출합니다.
        // 이 방법은 씬에 SaveManager가 있어야만 작동합니다.
        SaveManager manager = FindObjectOfType<SaveManager>();
        if (manager != null)
        {
            manager.ClearDestroyedObjectHistory();
            Debug.Log("세이브 데이터가 성공적으로 삭제되었습니다.");
        }
        else
        {
            // 만약 PlayerPrefs만 쓴다면 아래 코드로 직접 삭제할 수도 있습니다.
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.LogWarning("씬에 SaveManager가 없어서 PlayerPrefs를 직접 삭제했습니다.");
        }
    }
}
