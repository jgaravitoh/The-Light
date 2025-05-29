using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Carousel RenderText Entry", menuName = "UI/Carousel RenderText Entry", order =0)]
public class CarouselEntryRenderTexture:ScriptableObject
{
    //[field: SerializeField] public Sprite EntryGraphic { get; private set; }
    [field: SerializeField] public RenderTexture EntryGraphic { get; private set; }
    [field: SerializeField] public string Headline { get; private set; }
    [field: SerializeField, Multiline(10)] public string Description { get; private set; }

    //[Header("Interaction")]
    //[SerializeField] private string levelNameToLoad;

    public void Interact()
    {
        //SceneManager.LoadScene(levelNameToLoad);         //add code here for button interaction if needed.
    }
}
