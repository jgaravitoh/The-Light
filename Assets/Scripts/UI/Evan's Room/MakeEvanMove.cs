using UnityEngine;

public class MakeEvanMove : MonoBehaviour
{
    [SerializeField] public Vector3 direction;
    [SerializeField] public float duration;
    [SerializeField] private Transform playerTransform;

    private void Awake()
    {
        if (playerTransform == null)
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
    }
    public void MoveEvan()
    {
        MovePlayerByUnits.Move(playerTransform,direction,duration);
        PlayerMovement.sharedInstancePlayerMovement.allowMovement = true;
    }
}
