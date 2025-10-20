using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadManager : MonoBehaviour
{
    private GameObject optionOn;
    public void LoadGameData(int slotIndex)
    {
        optionOn = OptionManager.instance.optionUi;
        optionOn.SetActive(true);
        GameManager.Instance.LoadGameSlot(slotIndex);
    }
}