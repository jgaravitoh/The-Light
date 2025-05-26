using UnityEngine;
using UnityEngine.EventSystems;

public class ScreenMouse : MonoBehaviour
{
    public RectTransform panelRect; // Assign your Panel's RectTransform in the Inspector
    private RectTransform imageRect;
    private Canvas canvas;
    private bool mouseVisibility = true;
    void Start()
    {
        imageRect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    void Update()
    {
        Vector2 localPoint;
        Vector2 mouseScreenPos = Input.mousePosition;

        // Convert screen point to local point in panel
        RectTransformUtility.ScreenPointToLocalPointInRectangle(panelRect, mouseScreenPos, canvas.worldCamera, out localPoint);

        // Clamp to panel bounds
        Vector2 clampedPosition = ClampToPanel(localPoint);
        imageRect.localPosition = clampedPosition;
        //imageRect.localPosition = localPoint;

        if (Input.GetKeyDown(KeyCode.Escape)) { mouseVisibility = !mouseVisibility;  Cursor.visible = mouseVisibility; };
    }

    Vector2 ClampToPanel(Vector2 position)
    {
        // Calculate min and max bounds for the image inside the panel
        Vector2 minBounds = panelRect.rect.min + (imageRect.rect.size * imageRect.pivot);
        Vector2 maxBounds = panelRect.rect.max - (imageRect.rect.size * (Vector2.one - imageRect.pivot));

        float clampedX = Mathf.Clamp(position.x, minBounds.x, maxBounds.x);
        float clampedY = Mathf.Clamp(position.y, minBounds.y, maxBounds.y);

        return new Vector2(clampedX, clampedY);
    }
}
