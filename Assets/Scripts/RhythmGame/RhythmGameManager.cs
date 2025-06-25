using System.Collections.Generic;
using UnityEngine;

public class RhythmGameManager : MonoBehaviour
{
    public static RhythmGameManager sharedInstanceRythmGameManager { get; private set; }

    [Header("---- Audio Variables ----")]
    [SerializeField] private AudioSource song_audioSource;
    [SerializeField] private AudioClip TEST_audioclip;

    [Header("---- Note Configuration Variables ----")]
    [SerializeField] private float note_speed = 5f;
    [SerializeField] private List<GameObject> spawnerReferences;
    [SerializeField] public GameObject hitZoneReference;
    [SerializeField] private float hitWindow = 0.15f;

    private float song_ElapsedTime = 0;
    private float scheduledSongStartTime = -1f;
    private bool songStarted = false;

    private RhythmMapCSVDataService rhythmMapCSVDataService;
    private RhythmMapTable rhythmMapTable;
    private bool rhythmMap_IsLoaded;
    private int rhythmMap_CurrentNoteIndex = 0;
    private float rhythmMap_NoteOffset = 0;
    private List<NoteData> activeNoteDataList = new();

    private void Awake()
    {
        if (sharedInstanceRythmGameManager == null)
        {
            sharedInstanceRythmGameManager = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        rhythmMapCSVDataService = new RhythmMapCSVDataService();
        rhythmMap_NoteOffset = CalculateNoteSpawnOffset();

        RhythmGameInputManager inputManager = FindAnyObjectByType<RhythmGameInputManager>();
        if (inputManager != null)
        {
            inputManager.OnLaneInput += TryHitNoteInLane;
        }
        else
        {
            Debug.LogWarning("RhythmGameInputManager not found in scene.");
        }

        LoadSongMap("Song1");
        LoadSongClip();
    }

    private void Update()
    {
        if (songStarted)
        {
            song_ElapsedTime = song_audioSource.time;
        }

        HandleNoteSpawning();
        CheckMissedNotes();
    }

    private void LoadSongMap(string songName)
    {
        string path = $"Assets/CSV Files/RhythmGame/{songName}.csv";
        rhythmMapCSVDataService.LoadDataAsync<RhythmMapTable>(path, false, (varTable) =>
        {
            if (varTable == null)
            {
                Debug.LogError("Could not load beat map.");
                return;
            }

            rhythmMapTable = varTable;
            rhythmMap_IsLoaded = true;

            // Schedule song start time after loading map
            scheduledSongStartTime = Time.time + rhythmMap_NoteOffset;
        });
    }

    private void LoadSongClip()
    {
        song_audioSource.clip = TEST_audioclip;
    }

    private float CalculateNoteSpawnOffset()
    {
        float distance = Mathf.Abs(spawnerReferences[0].transform.position.z - hitZoneReference.transform.position.z);
        return distance / note_speed;
    }

    private void HandleNoteSpawning()
    {
        if (!rhythmMap_IsLoaded || rhythmMap_CurrentNoteIndex >= rhythmMapTable.hitTimeStamps.Length)
            return;

        float nextHitTime = rhythmMapTable.hitTimeStamps[rhythmMap_CurrentNoteIndex];
        float spawnTime = scheduledSongStartTime + nextHitTime - rhythmMap_NoteOffset;

        if (Time.time >= spawnTime)
        {
            SpawnNoteRow();
        }

        if (!songStarted && Time.time >= scheduledSongStartTime)
        {
            song_audioSource.Play();
            songStarted = true;
        }
    }

    private void SpawnNoteRow()
    {
        float hitTime = rhythmMapTable.hitTimeStamps[rhythmMap_CurrentNoteIndex];

        for (int i = 0; i < spawnerReferences.Count; i++)
        {
            if (rhythmMapTable.hitDurations[rhythmMap_CurrentNoteIndex, i] >= 0)
            {
                SpawnNote(spawnerReferences[i].transform.position, i, hitTime);
            }
        }

        rhythmMap_CurrentNoteIndex++;
    }

    private void SpawnNote(Vector3 position, int lane, float hitTime)
    {
        GameObject note = RhythmGameNotePool.sharedInstanceRhythmGameNotePool.RequestNote();
        var visual = note.GetComponent<RythmGameNote>();
        visual.Spawn(position, lane);

        float absoluteHitTime = scheduledSongStartTime + hitTime;

        activeNoteDataList.Add(new NoteData()
        {
            hitTime = hitTime,
            lane = lane,
            visualNote = note.transform,
            wasHit = false,
            absoluteHitTime = absoluteHitTime
        });
    }

    public void TryHitNoteInLane(int lane)
    {
        NoteData closest = null;
        float smallestDiff = float.MaxValue;
        float currentTime = Time.time;

        foreach (var note in activeNoteDataList)
        {
            if (note.lane != lane || note.wasHit) continue;

            float diff = Mathf.Abs(currentTime - note.absoluteHitTime);
            if (diff <= hitWindow && diff < smallestDiff)
            {
                smallestDiff = diff;
                closest = note;
            }
        }

        if (closest != null)
        {
            Debug.Log($"HIT! lane {closest.lane}");
            closest.wasHit = true;
            closest.visualNote.gameObject.SetActive(false);
            activeNoteDataList.Remove(closest);
        }
        else
        {
            Debug.Log($"MISS! lane {lane}");
        }
    }

    private void CheckMissedNotes()
    {
        float currentTime = Time.time;

        for (int i = activeNoteDataList.Count - 1; i >= 0; i--)
        {
            NoteData note = activeNoteDataList[i];
            if (note.wasHit) continue;

            if (currentTime - note.absoluteHitTime > hitWindow)
            {
                Debug.Log($"MISS (too late) on lane {note.lane}");
                note.visualNote.gameObject.SetActive(false);
                activeNoteDataList.RemoveAt(i);
            }
        }
    }

    public float Note_GetSpeed() => note_speed;
    public void Note_SetSpeed(float newSpeed) => note_speed = newSpeed;
}

public class NoteData
{
    public int lane;
    public float hitTime;
    public Transform visualNote;
    public bool wasHit;
    public float absoluteHitTime;
}
