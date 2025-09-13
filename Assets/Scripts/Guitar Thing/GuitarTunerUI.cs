/*=====================================================================
 GuitarTunerUI.cs (Opción A recomendada)
 - Usa CurrentFrequencyInstant para decidir nota/cents (evita “barrer” notas)
 - Mantiene suavizado SOLO en el slider; resetea si hay salto grande
=====================================================================*/

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

public class GuitarTunerUI : MonoBehaviour
{
    [Header("Referencias")]
    public PitchDetector detector;

    [Header("UI base")]
    public GameObject panelStandardE;

    [Header("Slider y textos")]
    public Slider centsSlider;
    public TMP_Text txtNote;
    public TMP_Text txtLiveFreq;

    [Header("Botones de cuerdas - E estándar (6ª..1ª)")]
    public Button[] buttonsStandardE;

    [Header("Estilo de botones")]
    public Color activeColor = Color.green;
    public Color inactiveColor = Color.gray;
    public Sprite activeSprite;
    public Sprite inactiveSprite;
    public Color activeTextColor = Color.black;
    public Color inactiveTextColor = Color.white;

    [Header("Colores precisión (txtNote y slider)")]
    public Color colorOK = new Color(0.2f, 0.8f, 0.2f);
    public Color colorNear = new Color(1f, 0.85f, 0.2f);
    public Color colorFar = new Color(0.95f, 0.25f, 0.25f);
    public Color colorSilent = Color.gray;

    [Header("Slider parts (asigna en el Inspector)")]
    public Image sliderFill;
    public Image sliderBackground;

    [Header("Umbrales de cents")]
    public float umbralVerde = 5f;
    public float umbralAmarillo = 15f;

    [Header("Estabilidad extra (UI)")]
    [Range(0.01f, 1f)] public float centsSmoothing = 0.25f; // EMA
    public float harmonicHysteresisCents = 25f;
    public float deadbandCents = 1f;
    public float maxCentsStepPerFrame = 8f;
    public float clampCents = 50f;

    // Estado
    private int targetMidi = -1;
    private string targetLabel = "—";
    private int selectedIndex = -1;
    private float smoothedCents = 0f;
    private int lastHarmonicK = 0;

    // Presets
    private readonly Dictionary<string, int[]> presets = new Dictionary<string, int[]>();
    private readonly Dictionary<string, string[]> presetLabels = new Dictionary<string, string[]>();

    void Awake()
    {
        // E estándar
        presets["E estándar"] = new int[] { 40, 45, 50, 55, 59, 64 };         // E2,A2,D3,G3,B3,E4
        presetLabels["E estándar"] = new string[] { "E2", "A2", "D3", "G3", "B3", "E4" };
    }

    void Start()
    {
        OnChangeTuning(0);

        if (centsSlider != null)
        {
            centsSlider.minValue = -clampCents;
            centsSlider.maxValue = +clampCents;
            centsSlider.wholeNumbers = false;
            centsSlider.value = 0;
        }

        HookButtons(buttonsStandardE);
        ApplyButtonLabelsForAll();

        if (txtNote) txtNote.text = "—";
        SetPrecisionColor(colorSilent, true);
    }

    void Update()
    {
        if (detector == null || !detector.IsReady) return;

        // Usa frecuencia instantánea para decidir nota/cents (evita barrer intermedias)
        float fInst = detector.CurrentFrequencyInstant;
        float fSmooth = detector.CurrentFrequency; // opcional para mostrar en Hz “suave”

        if (txtLiveFreq) txtLiveFreq.text = (fInst > 0f) ? $"{fInst:0.0} Hz" : "—";

        if (fInst <= 0f)
        {
            if (centsSlider) centsSlider.value = 0;
            if (txtNote)
            {
                txtNote.text = (targetMidi >= 0) ? $"{targetLabel} → —" : "—";
                SetPrecisionColor(colorSilent, true);
            }
            return;
        }

        // Helper: de frecuencia a midi y cents respecto a nota más cercana
        Func<double, (int midiNearest, double cents)> MidiAndCentsFromFreq = (freq) =>
        {
            double midi = 69.0 + 12.0 * Math.Log(freq / detector.A4, 2.0);
            int midiNearest = (int)Math.Round(midi);
            double fNearest = detector.MidiToFreq(midiNearest);
            double cents = 1200.0 * Math.Log(freq / fNearest, 2.0);
            return (midiNearest, cents);
        };

        // --- MODO OBJETIVO ---
        if (targetMidi >= 0)
        {
            double fTarget = detector.MidiToFreq(targetMidi);

            // Selección de armónico con histéresis (usando fInst)
            int bestK = lastHarmonicK; bool first = true; double bestCents = 0;
            for (int k = -2; k <= 2; k++)
            {
                double fCand = fInst * Math.Pow(2.0, k);
                if (fCand <= 1e-6) continue;
                double cents = 1200.0 * Math.Log(fCand / fTarget, 2.0);
                double score = Math.Abs(cents);
                if (first) { bestK = k; bestCents = cents; first = false; }
                else if (score + 1.0 < Math.Abs(bestCents) - harmonicHysteresisCents) { bestK = k; bestCents = cents; }
            }
            // Mantener armónico previo si no mejora suficiente
            double fStay = fInst * Math.Pow(2.0, lastHarmonicK);
            double centsStay = 1200.0 * Math.Log(fStay / fTarget, 2.0);
            if (Math.Abs(bestCents) + 1.0 < Math.Abs(centsStay) - harmonicHysteresisCents || Math.Abs(centsStay) > 120.0)
                lastHarmonicK = bestK;
            else { bestK = lastHarmonicK; bestCents = centsStay; }

            // Reseteo del suavizado del slider si hubo salto grande
            double fAligned = fInst * Math.Pow(2.0, bestK);
            double midiAligned = 69.0 + 12.0 * Math.Log(fAligned / detector.A4, 2.0);
            if (Math.Abs(midiAligned - detector.CurrentMidi) >= detector.jumpSemitoneReset)
                smoothedCents = 0f;

            // Suavizado + deadband + slew SOLO para el slider
            float targetCents = (float)bestCents;
            if (Mathf.Abs(targetCents) < deadbandCents) targetCents = 0f;
            float ema = Mathf.Lerp(smoothedCents, targetCents, Mathf.Clamp01(centsSmoothing));
            float desiredDelta = ema - smoothedCents;
            float maxStep = Mathf.Max(0.01f, maxCentsStepPerFrame);
            if (Mathf.Abs(desiredDelta) > maxStep) smoothedCents += Mathf.Sign(desiredDelta) * maxStep;
            else smoothedCents = ema;

            if (centsSlider) centsSlider.value = Mathf.Clamp(smoothedCents, -clampCents, clampCents);

            // Nota en vivo (a partir de fInst alineada al armónico)
            var (midiNearest, _) = MidiAndCentsFromFreq(fAligned);
            string liveNote = detector.MidiToNoteName(midiNearest);

            if (txtNote)
            {
                txtNote.text = $"{targetLabel} → {liveNote} ({smoothedCents:+0;-0;0} cents)";
                ApplyPrecisionColor(Mathf.Abs(smoothedCents));
            }
            return;
        }

        // --- MODO CROMÁTICO ---
        var (midiChrom, centsChrom) = MidiAndCentsFromFreq(fInst); // usa instantánea
        string liveNoteChrom = detector.MidiToNoteName(midiChrom);

        // Resetea suavizado si saltó muchas notas
        if (Mathf.Abs(midiChrom - detector.CurrentMidi) >= detector.jumpSemitoneReset)
            smoothedCents = 0f;

        float targetCentsChrom = (float)centsChrom;
        if (Mathf.Abs(targetCentsChrom) < deadbandCents) targetCentsChrom = 0f;
        float emaChrom = Mathf.Lerp(smoothedCents, targetCentsChrom, Mathf.Clamp01(centsSmoothing));
        float deltaChrom = emaChrom - smoothedCents;
        float maxStepChrom = Mathf.Max(0.01f, maxCentsStepPerFrame);
        if (Mathf.Abs(deltaChrom) > maxStepChrom) smoothedCents += Mathf.Sign(deltaChrom) * maxStepChrom;
        else smoothedCents = emaChrom;

        if (centsSlider) centsSlider.value = Mathf.Clamp(smoothedCents, -clampCents, clampCents);

        if (txtNote)
        {
            txtNote.text = $"{liveNoteChrom} ({smoothedCents:+0;-0;0} cents)";
            ApplyPrecisionColor(Mathf.Abs(smoothedCents));
        }
    }

    // === LÓGICA DE AFINACIÓN ===

    public void OnChangeTuning(int optionIndex)
    {
        string key = IndexToPresetName(optionIndex);
        if (panelStandardE) panelStandardE.SetActive(key == "E estándar");

        ApplyButtonLabelsFor(key);

        targetMidi = -1;
        targetLabel = "—";
        selectedIndex = -1;
        smoothedCents = 0f;
        lastHarmonicK = 0;
        if (centsSlider) centsSlider.value = 0;
        if (txtNote) txtNote.text = "—";
        UpdateButtonStates();
        SetPrecisionColor(colorSilent, true);
    }

    public void OnSelectStringButton(int stringIndex)
    {
        string key = CurrentPresetName();
        var arr = presets[key];
        var labels = presetLabels[key];
        if (stringIndex < 0 || stringIndex >= arr.Length) return;

        targetMidi = arr[stringIndex];
        targetLabel = labels[stringIndex];
        selectedIndex = stringIndex;

        smoothedCents = 0f;
        lastHarmonicK = 0;

        if (txtNote) txtNote.text = $"{targetLabel} → —";
        if (centsSlider) centsSlider.value = 0;

        UpdateButtonStates();
    }

    // === UTILIDADES ===

    void HookButtons(Button[] btns)
    {
        if (btns == null) return;
        for (int i = 0; i < btns.Length; i++)
        {
            if (btns[i] == null) continue;
            int idx = i;
            btns[i].onClick.RemoveAllListeners();
            btns[i].onClick.AddListener(() => OnSelectStringButton(idx));
        }
    }

    void ApplyButtonLabelsForAll()
    {
        ApplyButtonLabels(buttonsStandardE, presetLabels["E estándar"]);
    }

    void ApplyButtonLabelsFor(string presetName)
    {
        if (presetName == "E estándar")
            ApplyButtonLabels(buttonsStandardE, presetLabels[presetName]);
    }

    void ApplyButtonLabels(Button[] btns, string[] labels)
    {
        if (btns == null || labels == null) return;
        for (int i = 0; i < btns.Length && i < labels.Length; i++)
        {
            if (btns[i] == null) continue;
            var tmp = btns[i].GetComponentInChildren<TMP_Text>();
            if (tmp != null) tmp.text = labels[i];
            else
            {
                var legacy = btns[i].GetComponentInChildren<UnityEngine.UI.Text>();
                if (legacy != null) legacy.text = labels[i];
            }
        }
    }

    void UpdateButtonStates()
    {
        SetButtonsVisual(buttonsStandardE, CurrentPresetName() == "E estándar");
    }

    void SetButtonsVisual(Button[] btns, bool isActiveGroup)
    {
        if (btns == null) return;
        for (int i = 0; i < btns.Length; i++)
        {
            if (btns[i] == null) continue;
            var img = btns[i].GetComponent<Image>();
            bool isSelected = isActiveGroup && (i == selectedIndex);

            if (img != null)
            {
                if (activeSprite != null && inactiveSprite != null)
                    img.sprite = isSelected ? activeSprite : inactiveSprite;

                img.color = isSelected ? activeColor : inactiveColor;
            }

            var tmp = btns[i].GetComponentInChildren<TMP_Text>();
            if (tmp) tmp.color = isSelected ? activeTextColor : inactiveTextColor;
        }
    }

    void ApplyPrecisionColor(float absCents)
    {
        if (absCents <= umbralVerde) SetPrecisionColor(colorOK, false);
        else if (absCents <= umbralAmarillo) SetPrecisionColor(colorNear, false);
        else SetPrecisionColor(colorFar, false);
    }

    void SetPrecisionColor(Color c, bool silent)
    {
        if (txtNote) txtNote.color = c;

        if (centsSlider != null)
        {
            if (centsSlider.handleRect != null)
            {
                var hImg = centsSlider.handleRect.GetComponent<Image>();
                if (hImg != null) hImg.color = c;
            }
            if (sliderFill != null) sliderFill.color = c;
            if (sliderBackground != null)
                sliderBackground.color = silent ? new Color(0.3f, 0.3f, 0.3f, 1f) : Color.white;
        }
    }

    string CurrentPresetName()
    {
        int i = 0; // si no usas dropdown
        return IndexToPresetName(i);
    }

    string IndexToPresetName(int i)
    {
        return "E estándar";
    }
}
