// ===============================
// RythmGameNote.cs
// ===============================

using UnityEngine;

public class RythmGameNote : MonoBehaviour
{
    private float note_speed;
    private int note_lane;
    [SerializeField] private bool note_directionForwardBool = true;
    private int note_direction;

    private void Start()
    {
        note_speed = RhythmGameManager.sharedInstanceRythmGameManager.Note_GetSpeed();
        note_direction = note_directionForwardBool ? 1 : -1;
    }

    private void FixedUpdate()
    {
        MoveNote();
    }

    private void MoveNote()
    {
        transform.Translate(Vector3.forward * note_speed * Time.deltaTime * note_direction);
    }

    public void Spawn(Vector3 position, int lane)
    {
        transform.position = position;
        note_lane = lane;
    }

    public int GetLane() => note_lane;
}