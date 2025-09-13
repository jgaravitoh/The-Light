using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PitchDetector (estable)
/// - Ring buffer + hop fijo para YIN (CMND).
/// - DC removal + ventana Hann.
/// - Umbral de confianza (1-CMND), ZCR para filtrar ruido.
/// - Mediana (ventana corta) para estabilizar CurrentFrequencyInstant.
/// - Histeresis en corrección de armónicos (÷2/÷3/÷4).
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class PitchDetector : MonoBehaviour
{
    [Header("Micrófono")]
    public string microphoneDeviceName = "";
    public int requestedSampleRate = 48000;
    [Range(0.5f, 5f)] public float micClipLengthSec = 1.0f;

    [Header("Análisis")]
    public int analysisWindow = 2048;     // potencia de 2
    public int hopSize = 256;             // procesa cada 256 muestras (≈5.3ms a 48k)
    [Range(0.01f, 0.3f)] public float yinThreshold = 0.10f;
    public float minFrequency = 60f;
    public float maxFrequency = 1200f;

    [Header("Silencio / Voicing")]
    public float rmsVoiceThreshold = 0.008f;
    [Range(0.2f, 1f)] public float rmsHysteresisFactor = 0.7f;
    [Tooltip("Ignorar frames con confianza < este valor (1-CMND). 0.6-0.85 va bien.")]
    [Range(0.3f, 0.98f)] public float minConfidence = 0.75f;
    [Tooltip("Si RMS bajo y ZCR alto, tratar como ruido (silencio).")]
    [Range(0f, 1f)] public float zcrNoiseThreshold = 0.18f;

    [Header("Corrección de armónicos")]
    public bool correctHarmonics = true;
    [Range(1, 4)] public int harmonicDivisions = 3;
    public float harmonicAcceptCents = 12f;
    [Tooltip("Mejora mínima (cents) para cambiar de divisor y evitar parpadeo.")]
    public float harmonicHysteresisCents = 8f;

    [Header("Suavizado / salida")]
    [Range(0f, 1f)] public float hzSmoothing = 0.25f; // EMA p/CurrentFrequency
    public int jumpSemitoneReset = 4;
    public float A4 = 440f;

    // === API que usan tus otros scripts ===
    public bool IsReady { get; private set; } = false;
    public float CurrentFrequencyInstant { get; private set; } = 0f; // Hz sin suavizar (pero con mediana)
    public float CurrentFrequency { get; private set; } = 0f;        // Hz suavizada (EMA)
    public float CurrentLevelRMS { get; private set; } = 0f;
    public bool IsVoiced { get; private set; } = false;
    public int CurrentMidi { get; private set; } = -1;

    // === Internos ===
    private AudioSource _audio;
    private AudioClip _micClip;
    private int _sampleRate;
    private float _rmsVoiceThresholdDown;

    // Ring buffer
    private float[] _ring;
    private int _ringWrite;        // índice de escritura
    private int _ringCount;        // muestras válidas
    private int _samplesSinceLast; // hop scheduler

    // Trabajo
    private float[] _frame;
    private float[] _hann;
    private float[] _yinDiff;
    private float[] _yinCMND;

    // Mediana para f0 instantánea
    private readonly Queue<float> _f0Window = new Queue<float>(5);
    private const int MEDIAN_LEN = 5;

    // Histeresis de armónicos
    private int _lastDivisor = 1;  // 1=fundamental, 2/3/4…

    private static readonly string[] NOTE_NAMES_SHARP =
        { "C","C#","D","D#","E","F","F#","G","G#","A","A#","B" };
    private int _lastMicPos = 0;
    void Awake()
    {
        _audio = GetComponent<AudioSource>();
        _audio.playOnAwake = true;
        _audio.loop = true;
        _audio.volume = 0f; // evitar feedback
    }

    void Start()
    {
        analysisWindow = Mathf.NextPowerOfTwo(Mathf.Max(1024, analysisWindow));
        hopSize = Mathf.Clamp(hopSize, 64, analysisWindow);

        _frame = new float[analysisWindow];
        _hann = MakeHann(analysisWindow);

        _yinDiff = new float[analysisWindow / 2 + 2];
        _yinCMND = new float[_yinDiff.Length];

        // ring buffer = múltiplo de ventana, para holgura
        int ringLen = Mathf.NextPowerOfTwo(Mathf.CeilToInt(micClipLengthSec * requestedSampleRate));
        if (ringLen < analysisWindow * 4) ringLen = analysisWindow * 4;
        _ring = new float[ringLen];

        _sampleRate = (requestedSampleRate > 0) ? requestedSampleRate : AudioSettings.outputSampleRate;
        if (_sampleRate <= 0) _sampleRate = 48000;

        if (Microphone.devices == null || Microphone.devices.Length == 0)
        {
            Debug.LogError("[PitchDetector] No hay micrófono.");
            return;
        }

        string dev = string.IsNullOrEmpty(microphoneDeviceName) ? Microphone.devices[0] : microphoneDeviceName;
        int clipSec = Mathf.Clamp(Mathf.CeilToInt(micClipLengthSec), 1, 10);
        _micClip = Microphone.Start(dev, true, clipSec, _sampleRate);
        while (Microphone.GetPosition(dev) <= 0) { }

        _audio.clip = _micClip;
        _audio.loop = true;
        _audio.Play();

        _rmsVoiceThresholdDown = rmsVoiceThreshold * rmsHysteresisFactor;

        IsReady = true;
        Debug.Log($"[PitchDetector] {dev} @ {_sampleRate}Hz, win={analysisWindow}, hop={hopSize}");
    }

    void OnDisable()
    {
        if (_audio != null && _audio.isPlaying) _audio.Stop();
        if (_micClip != null)
        {
            foreach (var dev in Microphone.devices) Microphone.End(dev);
        }
    }

    // Sustituye TODO el contenido de Update() por esto:
    void Update()
    {
        if (!IsReady || _micClip == null) return;

        int micPos = Microphone.GetPosition(null);
        if (micPos < 0) return;

        int clipSamples = _micClip.samples;
        int delta = micPos - _lastMicPos;
        if (delta < 0) delta += clipSamples; // wrap

        if (delta == 0) return;

        const int CHUNK = 2048;
        int remaining = delta;
        int readPos = _lastMicPos;

        while (remaining > 0)
        {
            int n = Mathf.Min(CHUNK, remaining);

            // Cuidado con wrap dentro del chunk
            int untilEnd = clipSamples - readPos;
            int first = Mathf.Min(n, untilEnd);
            float[] tmp = new float[first];
            _micClip.GetData(tmp, readPos);
            PushRing(tmp, first);

            int leftover = n - first;
            if (leftover > 0)
            {
                float[] tmp2 = new float[leftover];
                _micClip.GetData(tmp2, 0);
                PushRing(tmp2, leftover);
            }

            readPos = (readPos + n) % clipSamples;
            remaining -= n;
        }

        _lastMicPos = micPos;

        // Procesar por hop fijo
        while (_samplesSinceLast >= hopSize && _ringCount >= analysisWindow)
        {
            ReadLastWindow(_frame);
            ProcessFrame(_frame);
            _samplesSinceLast -= hopSize;
        }
    }

    // ===== Audio y ring buffer =====
    private void PushRing(float[] src, int n)
    {
        for (int i = 0; i < n; i++)
        {
            _ring[_ringWrite] = src[i];
            _ringWrite = (_ringWrite + 1) & (_ring.Length - 1);
            if (_ringCount < _ring.Length) _ringCount++;
        }
        _samplesSinceLast += n;
    }

    private void ReadLastWindow(float[] dst)
    {
        int n = dst.Length;
        int start = (_ringWrite - n - 1);
        // Convertir a índice positivo modulo len
        if (start < 0) start = _ring.Length - (Mathf.Abs(start) % _ring.Length) - 1;

        for (int i = 0; i < n; i++)
        {
            int idx = (start + 1 + i) & (_ring.Length - 1);
            dst[i] = _ring[idx];
        }
    }

    // ===== Procesamiento por frame =====
    private void ProcessFrame(float[] frame)
    {
        // Quitar DC y calcular RMS
        double sum = 0;
        double sumSq = 0;
        for (int i = 0; i < frame.Length; i++)
        {
            double s = frame[i];
            sum += s;
            sumSq += s * s;
        }
        float mean = (float)(sum / frame.Length);
        float rms = Mathf.Sqrt((float)(sumSq / frame.Length) - mean * mean);
        CurrentLevelRMS = Mathf.Max(0f, rms);

        // Voicing por RMS con histéresis
        if (!IsVoiced) IsVoiced = (rms >= rmsVoiceThreshold);
        else IsVoiced = (rms >= _rmsVoiceThresholdDown);

        // ZCR (solo para descartar ruido broadband cuando RMS bajo)
        float zcr = ComputeZCR(frame, mean);
        if ((!IsVoiced) || (rms < rmsVoiceThreshold && zcr > zcrNoiseThreshold))
        {
            SetFrequencies(0f, trueReset: false);
            return;
        }

        // Ventana Hann + quitar DC
        for (int i = 0; i < frame.Length; i++)
            _frame[i] = (frame[i] - mean) * _hann[i];

        // YIN
        int tauMin = Mathf.Max(1, Mathf.FloorToInt(_sampleRate / maxFrequency));
        int tauMax = Mathf.Min(_yinDiff.Length - 1, Mathf.CeilToInt(_sampleRate / Mathf.Max(minFrequency, 1f)));

        ComputeYinDifference(_frame, tauMax);
        ComputeYinCMND(_yinDiff, _yinCMND, tauMax);

        int tau = AbsoluteThreshold(_yinCMND, tauMin, tauMax, yinThreshold, out float cmndVal);
        if (tau == -1)
        {
            SetFrequencies(0f, trueReset: false);
            return;
        }

        float tauInterp = ParabolicInterp(_yinCMND, tau);
        float f0 = (tauInterp > 0f) ? (_sampleRate / tauInterp) : 0f;

        // Confianza mínima
        float confidence = 1f - Mathf.Clamp01(cmndVal);
        if (confidence < minConfidence)
        {
            SetFrequencies(0f, trueReset: false);
            return;
        }

        // Corrección de armónicos con histeresis
        if (correctHarmonics && f0 > 0f) f0 = TryHarmonicCorrectionWithHysteresis(f0);

        if (f0 < minFrequency || f0 > maxFrequency) f0 = 0f;

        // Mediana sobre f0 instantánea
        float f0Inst = ApplyMedian(f0);

        SetFrequencies(f0Inst, trueReset: true);
    }

    // ===== Estadísticos y ayudas =====
    private float ComputeZCR(float[] x, float mean)
    {
        int crossings = 0;
        float prev = x[0] - mean;
        for (int i = 1; i < x.Length; i++)
        {
            float cur = x[i] - mean;
            if ((prev >= 0 && cur < 0) || (prev < 0 && cur >= 0)) crossings++;
            prev = cur;
        }
        return crossings / (float)x.Length; // normalizado a [0, ~0.5]
    }

    private float ApplyMedian(float f0)
    {
        if (_f0Window.Count == MEDIAN_LEN) _f0Window.Dequeue();
        _f0Window.Enqueue(f0 <= 0f ? 0f : f0);

        float[] arr = new float[_f0Window.Count];
        _f0Window.CopyTo(arr, 0);
        Array.Sort(arr);
        return arr[arr.Length / 2];
    }

    private float TryHarmonicCorrectionWithHysteresis(float f0)
    {
        // calcular “mejora” en cents al dividir por 2..N
        int bestDiv = 1;
        double bestCents = Math.Abs(CentsToNearestMidi(f0));

        for (int div = 2; div <= harmonicDivisions; div++)
        {
            float cand = f0 / div;
            if (cand < minFrequency) break;

            double cents = Math.Abs(CentsToNearestMidi(cand));
            double improve = bestCents - cents;

            // Acepta si mejora suficiente y está dentro de tolerancia
            if (improve > harmonicHysteresisCents && cents <= harmonicAcceptCents)
            {
                bestCents = cents;
                bestDiv = div;
            }
        }

        // Histeresis: si cambia divisor, que sea porque mejora bastante
        if (bestDiv != _lastDivisor)
        {
            // Si el nuevo no mejora al menos X cents sobre el actual, mantén
            float candNew = f0 / bestDiv;
            float candOld = f0 / _lastDivisor;
            double centsNew = Math.Abs(CentsToNearestMidi(candNew));
            double centsOld = Math.Abs(CentsToNearestMidi(candOld));

            if (centsOld - centsNew > harmonicHysteresisCents)
                _lastDivisor = bestDiv; // cambio justificado
        }

        return f0 / _lastDivisor;
    }

    private double CentsToNearestMidi(float freq)
    {
        if (freq <= 0f) return 1e9;
        double midi = 69.0 + 12.0 * Math.Log(freq / A4, 2.0);
        int mNearest = (int)Math.Round(midi);
        double fNearest = MidiToFreq(mNearest);
        return 1200.0 * Math.Log(freq / fNearest, 2.0);
    }

    // ===== YIN =====
    private void ComputeYinDifference(float[] x, int tauMax)
    {
        Array.Clear(_yinDiff, 0, _yinDiff.Length);
        int N = x.Length;
        int maxTau = Mathf.Min(tauMax, _yinDiff.Length - 1);
        for (int tau = 1; tau <= maxTau; tau++)
        {
            double sum = 0.0;
            int limit = N - tau;
            for (int j = 0; j < limit; j++)
            {
                float d = x[j] - x[j + tau];
                sum += d * d;
            }
            _yinDiff[tau] = (float)sum;
        }
        _yinDiff[0] = 0f;
    }

    private void ComputeYinCMND(float[] diff, float[] cmnd, int tauMax)
    {
        cmnd[0] = 1f;
        float run = 0f;
        for (int tau = 1; tau <= tauMax; tau++)
        {
            run += diff[tau];
            cmnd[tau] = (run > 0f) ? (diff[tau] * tau / run) : 1f;
        }
        for (int i = tauMax + 1; i < cmnd.Length; i++) cmnd[i] = 1f;
    }

    private int AbsoluteThreshold(float[] cmnd, int tauMin, int tauMax, float threshold, out float cmndAtBest)
    {
        int best = -1;
        float bestVal = float.MaxValue;
        for (int tau = tauMin; tau <= tauMax; tau++)
        {
            float v = cmnd[tau];
            if (v < threshold && v < bestVal)
            {
                bestVal = v;
                best = tau;
            }
        }
        cmndAtBest = (best >= 0) ? bestVal : 1f;
        return best;
    }

    private float ParabolicInterp(float[] arr, int idx)
    {
        int i0 = Mathf.Max(1, idx - 1);
        int i2 = Mathf.Min(arr.Length - 2, idx + 1);
        float y0 = arr[i0], y1 = arr[idx], y2 = arr[i2];
        float denom = (y0 - 2f * y1 + y2);
        if (Mathf.Abs(denom) < 1e-12f) return idx;
        float delta = 0.5f * (y0 - y2) / denom;
        return idx + Mathf.Clamp(delta, -1f, 1f);
    }

    // ===== Conversión =====
    public double MidiToFreq(int midi) => A4 * Math.Pow(2.0, (midi - 69) / 12.0);

    public string MidiToNoteName(int midi)
    {
        if (midi < 0 || midi > 127) return "—";
        int semi = midi % 12;
        int oct = (midi / 12) - 1;
        return NOTE_NAMES_SHARP[semi] + oct.ToString();
    }

    // ===== Util =====
    private float[] MakeHann(int N)
    {
        var w = new float[N];
        for (int n = 0; n < N; n++)
            w[n] = 0.5f * (1f - Mathf.Cos(2f * Mathf.PI * n / (N - 1)));
        return w;
    }

    private void SetFrequencies(float fInstant, bool trueReset)
    {
        float prevSmooth = CurrentFrequency;
        CurrentFrequencyInstant = fInstant;

        if (fInstant <= 0f)
        {
            CurrentFrequency = Mathf.Lerp(CurrentFrequency, 0f, 0.6f);
            CurrentMidi = -1;
            return;
        }

        if (prevSmooth > 0f && trueReset)
        {
            double mPrev = 69.0 + 12.0 * Math.Log(prevSmooth / A4, 2.0);
            double mNow = 69.0 + 12.0 * Math.Log(fInstant / A4, 2.0);
            if (Mathf.Abs((float)(mNow - mPrev)) >= jumpSemitoneReset)
                CurrentFrequency = fInstant; // reset
            else
                CurrentFrequency = Mathf.Lerp(CurrentFrequency, fInstant, Mathf.Clamp01(hzSmoothing));
        }
        else
        {
            CurrentFrequency = (CurrentFrequency <= 0f) ? fInstant
                                                        : Mathf.Lerp(CurrentFrequency, fInstant, Mathf.Clamp01(hzSmoothing));
        }

        if (CurrentFrequency > 0f)
        {
            double midi = 69.0 + 12.0 * Math.Log(CurrentFrequency / A4, 2.0);
            CurrentMidi = (int)Math.Round(midi);
        }
        else CurrentMidi = -1;
    }
}