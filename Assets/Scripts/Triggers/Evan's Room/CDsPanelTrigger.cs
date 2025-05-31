using UnityEngine;

public class CDsPanelTrigger : MonoBehaviour
{
    [SerializeField] private GameObject CdCarouselPanel;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            CdCarouselPanel.SetActive(true);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            CdCarouselPanel.SetActive(false);
        }
    }
}
