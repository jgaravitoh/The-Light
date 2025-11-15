using UnityEngine;

public class StartMinigameAfterTutorial : MonoBehaviour
{
    private void OnDisable()
    {
        ArticleSystemManager.Instance.NotifyTutorialEnded();
    }
}
