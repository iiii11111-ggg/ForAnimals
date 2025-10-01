using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadManager : MonoBehaviour
{
    public void LoadGameData(int slotIndex)
    {
        GameManager.Instance.LoadGameSlot(slotIndex, "InGame");
    }
}