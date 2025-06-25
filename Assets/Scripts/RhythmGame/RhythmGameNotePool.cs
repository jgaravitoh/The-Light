// ===============================
// RhythmGameNotePool.cs
// ===============================

using System.Collections.Generic;
using UnityEngine;

public class RhythmGameNotePool : MonoBehaviour
{
    [SerializeField] private GameObject notePrefab;
    [SerializeField] private int poolSize = 42;
    private List<GameObject> noteList = new();

    public static RhythmGameNotePool sharedInstanceRhythmGameNotePool { get; private set; }

    private void Awake()
    {
        if (sharedInstanceRhythmGameNotePool == null) { sharedInstanceRhythmGameNotePool = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    private void Start()
    {
        AddNotesToPool(poolSize);
    }

    private void AddNotesToPool(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            GameObject note = Instantiate(notePrefab);
            note.SetActive(false);
            noteList.Add(note);
            note.transform.parent = transform;
        }
    }

    public GameObject RequestNote()
    {
        foreach (var note in noteList)
        {
            if (!note.activeSelf)
            {
                note.SetActive(true);
                return note;
            }
        }

        AddNotesToPool(1);
        GameObject newNote = noteList[^1];
        newNote.SetActive(true);
        return newNote;
    }
}