using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class NoteSequenceJukebox : MonoBehaviour
{
    [Header("Fuentes")]
    public PitchDetector detector;
    public AudioSource musicSource;

    [Header("Confirmación de notas")]
    public float confirmCentsWindow = 15f;
    public float confirmHoldSeconds = 0.20f;
    public float dropoutGraceSeconds = 0.08f;
    public bool requireReleaseForSameNote = true;

    [Header("Descanso (silencio) para rearmar MISMA nota")]
    public float rearmSilenceSeconds = 0.08f;
    public float rearmAbsoluteFloor = 0.004f;

    [Header("Secuencia y reproducción")]
    public bool strictPrefixPruning = true;
    public bool lockoutWhilePlaying = true;
    [Range(0f, 1f)] public float musicVolume = 1f;

    [Header("Tolerancias de octava")]
    public bool acceptGlobalMinusOneOctave = true;
    public int globalAltOctaveOffsetSemitones = -12;
    public bool allowPerNoteMinusOneOctave = true;

    [Serializable]
    public class SongEntry
    {
        [Tooltip("Secuencia de 4 notas: \"E2 A2 D3 G3\"")]
        public string sequenceNames;
        public AudioClip songClip;
        public string songId = "Song";
    }

    [Header("Diccionario de secuencias → canción")]
    public List<SongEntry> songs = new List<SongEntry>();

    // ===== UI =====
    [Header("UI (arrastrar TMP_Text / chips)")]
    public TMP_Text seqText;
    public TMP_Text candidateText;
    public TMP_Text lastSongText;

    public bool useChips = false;
    public Transform seqChipsContainer;
    public GameObject seqChipPrefab;

    // ===== ACCIÓN POST-MATCH (opcional) =====
    [Header("Acción al detectar secuencia válida")]
    public bool deactivateOnMatch = false;             // opcional
    public GameObject targetToDeactivate;
    public float deactivateDelay = 0.05f;

    // ===== Bloqueo durante reproducción =====
    [Header("Bloqueo de detección durante reproducción")]
    public float postSongCooldownSeconds = 0.15f;
    private bool hardLockout = false;

    // ===== Indicadores de progreso =====
    [Header("Indicadores visuales de progreso (4 luces)")]
    [Tooltip("Lista de 4 GameObjects que se encienden según el progreso (1..4).")]
    public List<GameObject> progressLights = new List<GameObject>(4);

    [Tooltip("Tiempo que las 4 luces permanecen encendidas al completar la secuencia.")]
    public float lightsAutoOffSeconds = 1.0f;

    [Tooltip("GameObject especial que se activa al completar la secuencia y se desactiva al terminar la canción.")]
    public GameObject sequenceSuccessObject;

    // No permitir volver a encender luces hasta que termine la canción
    private bool lightsLocked = false;
    private Coroutine lightsOffRoutine;

    // ===== Internos =====
    private readonly List<int[]> sequencesMidi = new List<int[]>();
    private readonly List<SongEntry> sequencesMeta = new List<SongEntry>();

    // Estado de UNA nota
    private int candidateMidi = -1;
    private float timeInsideWindow = 0f;
    private float timeOutsideWindow = 0f;
    private int lastConfirmedMidi = -1;
    private bool armedForSameNote = true;

    // Prefijo confirmado (0..4)
    private readonly List<int> prefix = new List<int>(4);

    // Silencio acumulado para rearmar
    private float timeSilence = 0f;

    void Awake()
    {
        if (detector == null) { Debug.LogError("[Jukebox] Falta PitchDetector."); enabled = false; return; }
        if (musicSource == null) { Debug.LogError("[Jukebox] Falta AudioSource."); enabled = false; return; }

        sequencesMidi.Clear();
        sequencesMeta.Clear();

        foreach (var entry in songs)
        {
            if (string.IsNullOrWhiteSpace(entry.sequenceNames) || entry.songClip == null)
            {
                Debug.LogWarning("[Jukebox] Entrada ignorada: falta secuencia o clip.");
                continue;
            }
            if (!TryParseSequence(entry.sequenceNames, out var midiSeq))
            {
                Debug.LogWarning($"[Jukebox] No pude parsear: \"{entry.sequenceNames}\"");
                continue;
            }
            if (midiSeq.Length != 4)
            {
                Debug.LogWarning($"[Jukebox] La secuencia debe tener exactamente 4 notas: \"{entry.sequenceNames}\"");
                continue;
            }
            sequencesMidi.Add(midiSeq);
            sequencesMeta.Add(entry);
        }

        // Luces y UI inicial
        SetAllLights(false);
        if (sequenceSuccessObject) sequenceSuccessObject.SetActive(false);
        RefreshUISequence();
        SetText(candidateText, "—");
        SetText(lastSongText, "—");
    }

    void Update()
    {
        if (!detector.IsReady) return;

        bool playing = musicSource != null && musicSource.isPlaying;
        bool isLocked = lockoutWhilePlaying && (playing || hardLockout);

        if (isLocked)
        {
            ResetHoldStateWhileLocked();
            return;
        }

        float fInst = detector.CurrentFrequencyInstant;
        float level = detector.CurrentLevelRMS;
        bool voiced = detector.IsVoiced;

        UpdateSilenceRearm(level, voiced);

        if (fInst <= 0f)
        {
            UpdateHoldState(false, false, float.MaxValue, -1);
            SetText(candidateText, "—");
            return;
        }

        double midiF = 69.0 + 12.0 * Math.Log(fInst / detector.A4, 2.0);
        int midiNearest = (int)Math.Round(midiF);
        double fNearest = detector.MidiToFreq(midiNearest);
        float cents = (float)(1200.0 * Math.Log(fInst / fNearest, 2.0));
        float centsAbs = Mathf.Abs(cents);

        bool inside = centsAbs <= confirmCentsWindow;
        bool sameAsCandidate = (candidateMidi == midiNearest);

        UpdateHoldState(inside, sameAsCandidate, centsAbs, midiNearest);
        SetText(candidateText, midiNearest >= 0 ? MidiName(midiNearest) : "—");

        // Confirmación
        if (armedForSameNote && timeInsideWindow >= confirmHoldSeconds && candidateMidi >= 0)
        {
            if (requireReleaseForSameNote && candidateMidi == lastConfirmedMidi)
            {
                // armed=true implica que ya hubo silencio → permitimos
            }

            ConfirmNote(candidateMidi);
            lastConfirmedMidi = candidateMidi;
            armedForSameNote = false;
            timeSilence = 0f;
        }
    }

    // ====== Lockout helpers ======
    private void EnterLockout()
    {
        hardLockout = true;

        // Limpiar estados de detección (NO tocamos luces: ya están bloqueadas)
        candidateMidi = -1;
        timeInsideWindow = 0f;
        timeOutsideWindow = 0f;
        lastConfirmedMidi = -1;
        armedForSameNote = true;
        timeSilence = 0f;

        // Limpiar prefijo y UI
        prefix.Clear();
        RefreshUISequence();
        SetText(candidateText, "—");
    }

    private void ExitLockout()
    {
        hardLockout = false;

        // Desbloquear luces y apagar indicador especial
        lightsLocked = false;
        if (sequenceSuccessObject) sequenceSuccessObject.SetActive(false);
        SetAllLights(false);

        // Rearme de estados
        candidateMidi = -1;
        timeInsideWindow = 0f;
        timeOutsideWindow = 0f;
        lastConfirmedMidi = -1;
        armedForSameNote = true;
        timeSilence = 0f;

        SetText(candidateText, "—");
    }

    private void ResetHoldStateWhileLocked()
    {
        candidateMidi = -1;
        timeInsideWindow = 0f;
        timeOutsideWindow = 0f;
        SetText(candidateText, "—");
    }

    private IEnumerator WaitSongAndCooldown()
    {
        while (musicSource != null && musicSource.isPlaying)
            yield return null;

        if (postSongCooldownSeconds > 0f)
            yield return new WaitForSeconds(postSongCooldownSeconds);

        ExitLockout();
    }

    // ===== Rearme por silencio (descanso) =====
    private void UpdateSilenceRearm(float levelNow, bool voicedNow)
    {
        bool isSilent = (!voicedNow) || (levelNow <= rearmAbsoluteFloor);
        if (isSilent)
        {
            timeSilence += Time.deltaTime;
            if (timeSilence >= rearmSilenceSeconds)
                armedForSameNote = true;
        }
        else
        {
            timeSilence = 0f;
        }
    }

    // ===== Hold/confirmación básica de candidato =====
    private void UpdateHoldState(bool insideWindow, bool sameMidiAsCandidate, float centsAbs, int nearestMidi)
    {
        float dt = Mathf.Max(Time.deltaTime, 1f / 120f);

        if (!insideWindow)
        {
            timeOutsideWindow += dt;
            timeInsideWindow = 0f;

            if (timeOutsideWindow >= dropoutGraceSeconds)
                candidateMidi = -1;
            return;
        }

        if (sameMidiAsCandidate)
        {
            timeInsideWindow += dt;
            timeOutsideWindow = 0f;
        }
        else
        {
            candidateMidi = nearestMidi;
            timeInsideWindow = dt;
            timeOutsideWindow = 0f;
        }
    }

    private void ConfirmNote(int midi)
    {
        if (prefix.Count < 4) prefix.Add(midi);
        RefreshUISequence();      // también actualiza luces de progreso (si no están bloqueadas)

        // Prefijo válido (exacto, global -12, por-nota -12)
        if (strictPrefixPruning && !IsValidPrefixWithOffsets(prefix))
        {
            int last = prefix[prefix.Count - 1];
            prefix.Clear();
            if (IsValidStartWithOffsets(last)) prefix.Add(last);
            RefreshUISequence();
            return;
        }

        while (prefix.Count > 4) prefix.RemoveAt(0);

        if (prefix.Count == 4)
        {
            int appliedOffset;
            int idx = MatchExactWithOffsets(prefix, out appliedOffset);
            if (idx >= 0)
            {
                // === Éxito: encender 4 luces, bloquearlas y activar GO especial ===
                ForceAllLightsOnAndLock();
                if (sequenceSuccessObject) sequenceSuccessObject.SetActive(true);

                // Apagar luces tras X s (siguen bloqueadas hasta fin de canción)
                if (lightsOffRoutine != null) StopCoroutine(lightsOffRoutine);
                lightsOffRoutine = StartCoroutine(AutoOffLightsAfterDelay(lightsAutoOffSeconds));

                // Reproducir canción y activar lockout
                PlaySong(idx);

                // Limpiar prefijo para la siguiente vez (cuando salga del lockout)
                prefix.Clear();
                RefreshUISequence();

                if (deactivateOnMatch)
                {
                    //var target = (targetToDeactivate != null) ? targetToDeactivate : this.gameObject;
                    foreach (GameObject light in progressLights)
                    {
                        if (light != null) DeactivateAfterSeconds(deactivateDelay, light);
                    }
                }
            }
            else
            {
                if (strictPrefixPruning)
                {
                    int last = prefix[prefix.Count - 1];
                    prefix.Clear();
                    if (IsValidStartWithOffsets(last)) prefix.Add(last);
                    RefreshUISequence();
                }
                else
                {
                    prefix.RemoveAt(0);
                    RefreshUISequence();
                }
            }
        }
    }

    // ===== Luces =====
    private void RefreshProgressLights()
    {
        if (lightsLocked) return;
        SetLightsProgress(prefix.Count);
    }

    private void SetLightsProgress(int count)
    {
        for (int i = 0; i < progressLights.Count; i++)
            if (progressLights[i] != null)
                progressLights[i].SetActive(i < count);
    }

    private void SetAllLights(bool on)
    {
        for (int i = 0; i < progressLights.Count; i++)
            if (progressLights[i] != null)
                progressLights[i].SetActive(on);
    }

    private void ForceAllLightsOnAndLock()
    {
        lightsLocked = true;
        SetAllLights(true);
    }

    private IEnumerator AutoOffLightsAfterDelay(float seconds)
    {
        if (seconds > 0f) yield return new WaitForSeconds(seconds);
        // Siguen bloqueadas, solo apagamos
        SetAllLights(false);
    }

    // ===== Reproductor =====
    private void PlaySong(int index)
    {
        var meta = sequencesMeta[index];
        if (meta.songClip == null) return;

        musicSource.Stop();
        musicSource.clip = meta.songClip;
        musicSource.volume = musicVolume;
        musicSource.Play();

        SetText(lastSongText, string.IsNullOrEmpty(meta.songId) ? "(sin id)" : meta.songId);
        Debug.Log($"[Jukebox] Reproduciendo: {meta.songId} (#{index})");

        if (lockoutWhilePlaying)
        {
            EnterLockout();
            StartCoroutine(WaitSongAndCooldown());
        }
    }

    // ===== Desactivación diferida =====
    private IEnumerator DeactivateAfterSeconds(float seconds, GameObject target)
    {
        if (seconds > 0f) yield return new WaitForSeconds(seconds);
        if (target != null) target.SetActive(false);
    }

    // ===== Matching con offsets =====
    private bool IsValidStartWithOffsets(int midi)
    {
        for (int i = 0; i < sequencesMidi.Count; i++)
        {
            int baseNote = sequencesMidi[i][0];
            if (midi == baseNote) return true;
            if (acceptGlobalMinusOneOctave && midi == baseNote + globalAltOctaveOffsetSemitones) return true;
            if (allowPerNoteMinusOneOctave && midi == baseNote + globalAltOctaveOffsetSemitones) return true;
        }
        return false;
    }

    private bool IsValidPrefixWithOffsets(List<int> pref)
    {
        int k = pref.Count;
        for (int i = 0; i < sequencesMidi.Count; i++)
        {
            var seq = sequencesMidi[i];

            if (EqualsWithOffset(pref, seq, k, 0)) return true;
            if (acceptGlobalMinusOneOctave && EqualsWithOffset(pref, seq, k, globalAltOctaveOffsetSemitones)) return true;
            if (allowPerNoteMinusOneOctave && EqualsPerNoteAlt(pref, seq, k, globalAltOctaveOffsetSemitones)) return true;
        }
        return false;
    }

    private int MatchExactWithOffsets(List<int> pref, out int appliedOffset)
    {
        appliedOffset = 0;
        for (int i = 0; i < sequencesMidi.Count; i++)
        {
            var seq = sequencesMidi[i];

            if (pref.Count == 4 && EqualsWithOffset(pref, seq, 4, 0)) { appliedOffset = 0; return i; }
            if (acceptGlobalMinusOneOctave && pref.Count == 4 &&
                EqualsWithOffset(pref, seq, 4, globalAltOctaveOffsetSemitones)) { appliedOffset = globalAltOctaveOffsetSemitones; return i; }
            if (allowPerNoteMinusOneOctave && pref.Count == 4 &&
                EqualsPerNoteAlt(pref, seq, 4, globalAltOctaveOffsetSemitones)) { appliedOffset = int.MinValue; return i; }
        }
        return -1;
    }

    private bool EqualsWithOffset(List<int> pref, int[] seq, int k, int offsetSemitones)
    {
        for (int j = 0; j < k; j++)
            if (pref[j] != seq[j] + offsetSemitones) return false;
        return true;
    }

    private bool EqualsPerNoteAlt(List<int> pref, int[] seq, int k, int offsetAlt)
    {
        for (int j = 0; j < k; j++)
        {
            int a = pref[j];
            int b = seq[j];
            if (!(a == b || a == b + offsetAlt)) return false;
        }
        return true;
    }

    // ===== Parser nombres → MIDI =====
    private static readonly Dictionary<string, int> NOTE_TO_SEMITONE = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
    {
        {"C",0}, {"C#",1}, {"Db",1}, {"D",2}, {"D#",3}, {"Eb",3},
        {"E",4}, {"Fb",4}, {"E#",5}, {"F",5}, {"F#",6}, {"Gb",6},
        {"G",7}, {"G#",8}, {"Ab",8}, {"A",9}, {"A#",10}, {"Bb",10},
        {"B",11}, {"Cb",11}, {"B#",0}
    };

    private static bool TryParseNoteName(string token, out int midi)
    {
        midi = -1;
        token = token.Trim();
        if (string.IsNullOrEmpty(token)) return false;

        int pos = token.Length - 1;
        while (pos >= 0 && (char.IsDigit(token[pos]) || (token[pos] == '-' && pos == token.Length - 2)))
            pos--;

        string name = token.Substring(0, pos + 1);
        string octStr = token.Substring(pos + 1);

        if (!NOTE_TO_SEMITONE.TryGetValue(name, out int semi)) return false;
        if (!int.TryParse(octStr, out int oct)) return false;

        midi = (oct + 1) * 12 + semi; // C-1 = 0
        return true;
    }

    private bool TryParseSequence(string seqNames, out int[] midiSeq)
    {
        var parts = seqNames.Split(new[] { ' ', '\t', ',', ';', '/' }, StringSplitOptions.RemoveEmptyEntries);
        List<int> list = new List<int>(parts.Length);
        foreach (var p in parts)
        {
            if (TryParseNoteName(p, out int m)) list.Add(m);
            else { midiSeq = null; return false; }
        }
        midiSeq = list.ToArray();
        return true;
    }

    // ===== UI helpers =====
    private void RefreshUISequence()
    {
        if (seqText != null) seqText.text = BuildNoteListString(prefix);

        // Actualiza luces según progreso (si no están bloqueadas)
        RefreshProgressLights();

        if (useChips && seqChipsContainer != null && seqChipPrefab != null)
        {
            for (int i = seqChipsContainer.childCount - 1; i >= 0; i--)
                GameObject.Destroy(seqChipsContainer.GetChild(i).gameObject);

            for (int i = 0; i < prefix.Count; i++)
            {
                var go = GameObject.Instantiate(seqChipPrefab, seqChipsContainer);
                var tmp = go.GetComponentInChildren<TMP_Text>();
                if (tmp != null) tmp.text = MidiName(prefix[i]);
            }
        }
    }

    private string MidiName(int midi)
    {
        if (detector != null && midi >= 0) return detector.MidiToNoteName(midi);
        return midi.ToString();
    }

    private string BuildNoteListString(List<int> list)
    {
        if (list == null || list.Count == 0) return "—";
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < list.Count; i++)
        {
            if (i > 0) sb.Append(" · ");
            sb.Append(MidiName(list[i]));
        }
        return sb.ToString();
    }

    private void SetText(TMP_Text t, string value)
    {
        if (t != null) t.text = value;
    }
}
