using UnityEngine;

public class AutoSave : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) 
        {
            SaveManager.Instance.SaveGameData(transform.position);
        } 
    }
}
