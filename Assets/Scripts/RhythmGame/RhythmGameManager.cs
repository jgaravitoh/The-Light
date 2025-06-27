using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the core gameplay logic of the rhythm game, including spawning notes, handling input, and syncing with audio.
/// </summary>
public class RhythmGameManager : MonoBehaviour
{
    #region Singleton
    /// <summary>
    /// Singleton instance for global access.
    /// </summary>
    public static RhythmGameManager sharedInstanceRythmGameManager { get; private set; }
    #endregion

    #region Serialized Fields
    [Header("---- Audio Variables ----")]
    [SerializeField] private AudioSource song_audioSource; // Audio source that plays the song
    [SerializeField] private AudioClip TEST_audioclip;     // Temporary audio clip for testing

    [Header("---- Note Configuration Variables ----")]
    [SerializeField] private float note_speed = 5f;                // Speed of the falling notes
    [SerializeField] private List<GameObject> spawnerReferences;  // References to each lane's note spawner
    [SerializeField] public GameObject hitZoneReference;          // The area where notes should be hit
    [SerializeField] private float hitWindow = 0.15f;              // Time window allowed to hit a note
    #endregion

    #region Private Fields
    private float song_ElapsedTime = 0;                                // Time elapsed since the song started
    private RhythmMapCSVDataService rhythmMapCSVDataService;          // Service to load rhythm map data
    private RhythmMapTable rhythmMapTable;                            // Loaded rhythm map table
    private bool rhythmMap_IsLoaded;                                  // Indicates if the rhythm map is loaded
    private int rhythmMap_CurrentNoteIndex = 0;                       // Current index of the note to spawn
    private float rhythmMap_NoteOffset = 0;                           // Offset time to spawn notes ahead of time
    private List<NoteData> activeNoteDataList = new List<NoteData>(); // List of currently active notes
    #endregion

    #region Unity Methods
    private void Awake()
    {
        if (sharedInstanceRythmGameManager == null)
        {
            sharedInstanceRythmGameManager = this;
            //DontDestroyOnLoad(gameObject);
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

        // Subscribe to input events
        RhythmGameInputManager inputManager = FindAnyObjectByType<RhythmGameInputManager>();
        if (inputManager != null)
        {
            inputManager.OnLaneInput += TryHitNoteInLane;
        }
        else
        {
            Debug.LogWarning("RhythmGameInputManager not found in scene.");
        }
    }


    private void Update()
    {
        UpdateSongTime();
        TryLoadMapAndSong();
        HandleNoteSpawning();
        CheckMissedNotes();

        // check win
        if (rhythmMap_IsLoaded && rhythmMap_CurrentNoteIndex >= rhythmMapTable.hitTimeStamps.Length && RhythmGameLoopManager.Instance.current_misses < RhythmGameLoopManager.Instance.max_misses)
        {
            if (activeNoteDataList.Count == 0)
            {
                RhythmGameLoopManager.Instance.ShowWin();
            }
        }
    }
    #endregion

    #region Song & Map Loading
    /// <summary>
    /// Loads the song map and audio clip if not already loaded.
    /// </summary>
    private void TryLoadMapAndSong()
    {
        if (!rhythmMap_IsLoaded)
        {
            LoadSongMap("Song1");
            LoadSongClip();
        }
    }

    /// <summary>
    /// Loads the rhythm map from a CSV file.
    /// </summary>
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
        });
    }

    /// <summary>
    /// Assigns the test audio clip to the audio source.
    /// </summary>
    private void LoadSongClip()
    {
        song_audioSource.clip = TEST_audioclip;
    }

    /// <summary>
    /// Plays the audio clip.
    /// </summary>
    private void PlaySongClip()
    {
        if (song_audioSource.clip == null)
        {
            Debug.LogError("Could not play song clip. It was not loaded beforehand.");
            return;
        }

        song_audioSource.Play();
    }

    /// <summary>
    /// Updates the elapsed time of the song.
    /// </summary>
    private void UpdateSongTime()
    {
        if (song_audioSource.isPlaying)
        {
            song_ElapsedTime = song_audioSource.time;
        }
    }
    #endregion

    #region Note Spawning
    /// <summary>
    /// Calculates the time it takes for a note to travel from spawner to hit zone.
    /// </summary>
    private float CalculateNoteSpawnOffset()
    {
        float distance = Mathf.Abs(spawnerReferences[0].transform.position.z - hitZoneReference.transform.position.z);
        return distance / note_speed;
    }

    /// <summary>
    /// Determines when to spawn the next set of notes based on song progress.
    /// </summary>
    private void HandleNoteSpawning()
    {
        if (!rhythmMap_IsLoaded || rhythmMap_CurrentNoteIndex >= rhythmMapTable.hitTimeStamps.Length)
            return;

        float nextHitTime = rhythmMapTable.hitTimeStamps[rhythmMap_CurrentNoteIndex];

        if (rhythmMap_CurrentNoteIndex == 0)
        {
            if (nextHitTime - rhythmMap_NoteOffset < 0)
            {
                Invoke(nameof(PlaySongClip), rhythmMap_NoteOffset);
                SpawnNoteRow();
            }
            else
            {
                PlaySongClip();
            }
        }
        else if (!song_audioSource.isPlaying)
        {
            if (Time.time + rhythmMap_NoteOffset > nextHitTime + rhythmMap_NoteOffset)
            {
                SpawnNoteRow();
            }
        }
        else if (song_ElapsedTime + rhythmMap_NoteOffset > nextHitTime)
        {
            SpawnNoteRow();
        }
    }

    /// <summary>
    /// Spawns a single note and tracks its data.
    /// </summary>
    private void SpawnNote(Vector3 position, int lane, float hitTime)
    {
        GameObject note = RhythmGameNotePool.sharedInstanceRhythmGameNotePool.RequestNote();
        var visual = note.GetComponent<RythmGameNote>();
        visual.Spawn(position, lane);

        activeNoteDataList.Add(new NoteData()
        {
            hitTime = hitTime,
            lane = lane,
            visualNote = note.transform,
            wasHit = false
        });
    }

    /// <summary>
    /// Spawns a row of notes across all active lanes.
    /// </summary>
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

    /// <summary>
    /// Returns the current note speed.
    /// </summary>
    public float Note_GetSpeed() => note_speed;

    /// <summary>
    /// Updates the note speed.
    /// </summary>
    public void Note_SetSpeed(float newSpeed) => note_speed = newSpeed;
    #endregion

    #region Input & Hit Detection
    /// <summary>
    /// Attempts to hit the closest note in a given lane based on song time.
    /// </summary>
    public void TryHitNoteInLane(int lane)
    {
        float currentTime = song_ElapsedTime;
        NoteData closest = null;
        float smallestDiff = float.MaxValue;

        foreach (var note in activeNoteDataList)
        {
            if (note.lane != lane || note.wasHit) continue;

            float diff = Mathf.Abs(note.hitTime - currentTime);
            if (diff < hitWindow && diff < smallestDiff)
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

    /// <summary>
    /// Deactivates and removes notes that were not hit within the allowed time window.
    /// </summary>
    private void CheckMissedNotes()
    {
        float currentTime = song_ElapsedTime;

        for (int i = activeNoteDataList.Count - 1; i >= 0; i--)
        {
            NoteData note = activeNoteDataList[i];
            if (note.wasHit) continue;

            if (currentTime - note.hitTime > hitWindow)
            {
                Debug.Log($"MISS (too late) on lane {note.lane}");
                note.visualNote.gameObject.SetActive(false);
                activeNoteDataList.RemoveAt(i);
                RhythmGameLoopManager.Instance.AddMissCounter();
            }
        }
    }
    #endregion

    public void PauseAudio()
    {
        song_audioSource.Pause();
    }
    public void UnpauseAudio()
    {
        song_audioSource.UnPause();
    }
    public void StopAudio()
    {
        song_audioSource.Stop();
    }
}

/// <summary>
/// Stores data for a spawned note, used for hit detection and lifecycle control.
/// </summary>
public class NoteData
{
    public int lane;                 // The lane the note belongs to
    public float hitTime;            // The expected hit time
    public Transform visualNote;     // Reference to the note's visual object
    public bool wasHit;              // Whether the note has been successfully hit
}