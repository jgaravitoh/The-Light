using System.Collections;
using UnityEngine;

public class MovePlayerByUnits : MonoBehaviour
{
    public static MovePlayerByUnits sharedInstanceMovePlayerByUnits { get; private set; }

    private void Awake()
    {
        // Singleton setup
        if (sharedInstanceMovePlayerByUnits != null && sharedInstanceMovePlayerByUnits != this)
        {
            Destroy(gameObject);
        }
        else
        {
            sharedInstanceMovePlayerByUnits = this;
        }

        // Optional: Keep this across scenes
        // DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Call this to move a Transform by a certain amount over time.
    /// </summary>
    /// <param name="target">The Transform to move.</param>
    /// <param name="moveAmount">How much to move (in world units).</param>
    /// <param name="duration">How long the move should take.</param>
    public static void Move(Transform target, Vector3 moveAmount, float duration)
    {
        if (sharedInstanceMovePlayerByUnits != null)
        {
            sharedInstanceMovePlayerByUnits.StartCoroutine(sharedInstanceMovePlayerByUnits.MoveOverTime(target, moveAmount, duration));
        }
        else
        {
            Debug.LogWarning("MoveObjectByUnits instance not found in scene!");
        }
    }

    private IEnumerator MoveOverTime(Transform target, Vector3 moveAmount, float duration)
    {
        Vector3 startPosition = target.position;
        Vector3 targetPosition = startPosition + moveAmount;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            target.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        target.position = targetPosition; // Snap to end
    }
}
