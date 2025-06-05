using UnityEngine;

public class SimpleShowHidePanel : MonoBehaviour
{
    public GameObject appPanel;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void TurnOn()
    {
        appPanel.SetActive(true);
    }
    public void TurnOff()
    {
        appPanel.SetActive(false);
    }
}
