using UnityEngine;
using UnityEngine.UI;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public Slider slider;
    void Start()
    {
        slider.value = 0;
    }

    // Update is called once per frame
    void Update()
    {
        slider.value += (0.01f*Time.deltaTime);
    }
}
