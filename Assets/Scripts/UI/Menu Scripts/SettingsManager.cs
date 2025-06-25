using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Reflection;
using ShadowResolution = UnityEngine.Rendering.Universal.ShadowResolution;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;

public class SettingsManager : MonoBehaviour
{
    #region Variables
    public static SettingsManager sharedInstanceSettingsManager;

    #region Audio Variables
    [Header("------------- Audio Variables -------------")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider ambientSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private string masterParameterString, SFXParameterString, ambientParameterString, musicParameterString;
    [SerializeField] private Image masterVolumeImageComponent, SFXVolumeImageComponent, ambientVolumeImageComponent, musicVolumeImageComponent;
    [SerializeField] private Sprite imageSoundMute, imageSound0to24, imageSound25to49, imageSound50to74, imageSound75to100;
    [SerializeField] private AudioMixer audioMixer;
    #endregion

    #region Screen Resolution Variables
    [Header("------------- Screen Resolution Variables -------------")]
    [SerializeField] private TMP_Dropdown screenResolutionDropdown;
    private Resolution[] screenResolutions;
    private int screenResolutionIndex = 0;
    #endregion

    #region Screen Mode Variables
    [Header("------------- Screen Mode Variables -------------")]
    [SerializeField] private TMP_Dropdown screenModeDropdown;
    private int screenModeIndex = 0; //Fullscreen by default
    List<string> screenModeOptions = new List<string>{ "Pantalla completa", "Ventana sin bordes", "Ventana"};

    #endregion

    #region Graphic Quality Variables

    [Header("------------- Graphic Quality Variables -------------")]
    // Indexes
    private int antialiasingIndex = 0;
    private int msaaIndex = 0;
    private int softShadowsIndex = 0;
    private int shadowsStateIndex = 0;

    private Camera[] camerasOnSceneArray;
    // Dropdown Option Lists
    List<string> antialiasingOptionsList = new List<string> { "Apagado", "FXAA", "SMAA", "TAA" };
    List<string> msaaOptionsList = new List<string> { "Apagado", "MSAA X2", "MSAA X4", "MSAA X8" };
    List<string> basicEnableDisableList = new List<string> { "Habilitado", "Desabilitado" };

    //[SerializeField] private UniversalRenderPipelineAsset urpQualityAsset;
    [SerializeField] private TMP_Dropdown antialiasingDropdown;
    [SerializeField] private TMP_Dropdown antialiasingMSAADropdown;
    [SerializeField] private TMP_Dropdown softShadowsDropdown;
    [SerializeField] private TMP_Dropdown shadowsStateDropdown;
    [SerializeField] private Slider renderScaleSlider;
    [SerializeField] private GameObject renderScaleValueIndicator;
    [SerializeField] private List<UniversalRenderPipelineAsset> urpQualityAssetList;
    [SerializeField] private UniversalRendererData urpQualityRendererAsset;
    
    private List<string> qualityLevels = new List<string>();
    private int screenQualityLevelIndex = 2;
    

    // ~~~~~~~~~~~~ Private/Hidden settings on URP ~~~~~~~~~~~~
    #region Graphics Settings Variables
    private FieldInfo mainLightCastShadows_FieldInfo;
    private FieldInfo additionalLightCastShadows_FieldInfo;
    private FieldInfo mainLightShadowmapResolution_FieldInfo;
    private FieldInfo additionalLightShadowmapResolution_FieldInfo;
    private FieldInfo cascade2Split_FieldInfo;
    private FieldInfo cascade4Split_FieldInfo;
    private FieldInfo softShadowsEnabled_FieldInfo;
    #endregion
    #endregion
    #endregion

    private void Awake()  // Make manager not be destroyed on load and subscribes to the scene load event, also iniatializes hidden/private stupid URP features
    {
        InitializeHiddenPrivateFieldsURP();
        DontDestroyOnLoad(gameObject);
        if (sharedInstanceSettingsManager == null)
        {
            sharedInstanceSettingsManager = this;
        }
        else { Destroy(gameObject); }
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) 
    {
        GetCamerasOnScene();
        SetAntialiasingQuality(GetAntialiasingQuality());
        SetAntialiasingMSAAQuality(GetAntialiasingMSAAQuality());
        SetSoftShadowsState(GetSoftShadowsState());
    }


    void Start()
    {
        #region Audio settings loading
        if (PlayerPrefs.HasKey("MasterVolume") || PlayerPrefs.HasKey("SFXVolume") || PlayerPrefs.HasKey("AmbientVolume") || PlayerPrefs.HasKey("MusicVolume"))
        {
            LoadVolume();
        }
        else
        {
            SetMasterVolume();
            SetSFXVolume();
            SetAmbientVolume();
            SetMusicVolume();
        }
        #endregion

        #region Screen Resolutions loading and Screen Mode settings and Framerate

        GetScreenResolutions();
        AddScreenResolutionsToDropdown();
        AddScreenModeOptionsToDropdown();

        // Cargar el modo de pantalla guadrado, si existe.
        if (PlayerPrefs.HasKey("ScreenModeIndex")) { screenModeIndex = PlayerPrefs.GetInt("ScreenModeIndex"); }
        // Cargar la resolución guardada, si existe
        if (PlayerPrefs.HasKey("ScreenResolutionIndex")) { screenResolutionIndex = PlayerPrefs.GetInt("ScreenResolutionIndex"); }
        // Establecer la resolución base de la pantalla actual como predeterminada si no hay ninguna guardada
        else { screenResolutionIndex = GetDefaultResolutionIndex(); }
        screenModeDropdown.value = screenModeIndex; 
        screenResolutionDropdown.value = screenResolutionIndex;
        screenResolutionDropdown.RefreshShownValue();
        screenModeDropdown.RefreshShownValue();
        #endregion

        #region Quality Level loading

        //AddVideoQualitiesToDropdown();

        // Cargar la resolución guardada, si existe
        if (PlayerPrefs.HasKey("QualityLevelIndex")){ SetQualityLevel(PlayerPrefs.GetInt("QualityLevelIndex")); } // Set saved value
        else { SetQualityLevel(screenQualityLevelIndex); } // Set default value

        #endregion

        #region GET URP ASSETS
        GetUrpAssets();
        #endregion

        #region Render Scale loading
        if (PlayerPrefs.HasKey("RenderScaleValue")) { renderScaleSlider.value = PlayerPrefs.GetFloat("RenderScaleValue"); }
        else { SetRenderScale(); }
        #endregion

        #region Shadows settings loading
        shadowsStateDropdown.ClearOptions();
        shadowsStateDropdown.AddOptions(basicEnableDisableList);

        softShadowsDropdown.ClearOptions();
        softShadowsDropdown.AddOptions(basicEnableDisableList);

        if (PlayerPrefs.HasKey("ShadowsStateIndex")) { shadowsStateIndex = PlayerPrefs.GetInt("ShadowsStateIndex"); }
        shadowsStateDropdown.value = shadowsStateIndex; shadowsStateDropdown.RefreshShownValue();
        
        if (PlayerPrefs.HasKey("SoftShadowsIndex")) { softShadowsIndex = PlayerPrefs.GetInt("SoftShadowsIndex"); }
        softShadowsDropdown.value = softShadowsIndex; softShadowsDropdown.RefreshShownValue();

        #endregion

        #region antialiasing settings loading

        antialiasingDropdown.ClearOptions(); antialiasingMSAADropdown.ClearOptions();
        antialiasingDropdown.AddOptions(antialiasingOptionsList); antialiasingMSAADropdown.AddOptions(msaaOptionsList);
        
        if (PlayerPrefs.HasKey("AntialiasingIndex"))
        {
            antialiasingIndex = PlayerPrefs.GetInt("AntialiasingIndex");
        }
        if (PlayerPrefs.HasKey("AntialiasingIndex"))
        {
            msaaIndex = PlayerPrefs.GetInt("MSAAIndex");
        }
        antialiasingDropdown.value = antialiasingIndex;
        antialiasingMSAADropdown.value = msaaIndex;
        antialiasingMSAADropdown.RefreshShownValue(); antialiasingDropdown.RefreshShownValue();
        #endregion
    }

    #region Audio Settings Scripts
    public void SetMasterVolume()
    {
        SetVolume(masterSlider, masterParameterString);
        ChangeVolumeImage(masterSlider);
    }
    public void SetSFXVolume()
    {
        SetVolume(sfxSlider, SFXParameterString);
        ChangeVolumeImage(sfxSlider);
    }
    public void SetAmbientVolume()
    {
        SetVolume(ambientSlider, ambientParameterString);
        ChangeVolumeImage(ambientSlider);
    }
    public void SetMusicVolume()
    {
        SetVolume(musicSlider, musicParameterString);
        ChangeVolumeImage(musicSlider);
    }

    private void SetVolume(Slider slider, string mixerParameter)
    {
        float volume = slider.value;
        //float exposedParam = 0f;
        audioMixer.SetFloat(mixerParameter, Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat(mixerParameter + "Volume", volume);
        PlayerPrefs.Save();
        //audioMixer.GetFloat(mixerParameter, out exposedParam);
    }

    private void LoadVolume()
    {
        masterSlider.value = PlayerPrefs.GetFloat("MasterVolume");
        SetMasterVolume();
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume");
        SetSFXVolume();
        ambientSlider.value = PlayerPrefs.GetFloat("AmbientVolume");
        SetAmbientVolume();
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume");
        SetMusicVolume();
    }

    private void ChangeVolumeImage(Slider slider)
    {
        Sprite spriteToSet = null;
        if (slider.value == 0.0001f) { spriteToSet = imageSoundMute; }
        else if (0.0001 < slider.value && slider.value < 0.24f) { spriteToSet = imageSound0to24; }
        else if (0.24 < slider.value && slider.value < 0.49f) { spriteToSet = imageSound25to49; }
        else if (0.49 < slider.value && slider.value < 0.74f) { spriteToSet = imageSound50to74; }
        else if (0.74 < slider.value) { spriteToSet = imageSound75to100; }

        switch (slider.name)
        {
            case "MasterSlider":
                masterVolumeImageComponent.sprite = spriteToSet;
                break;
            case "SFXSlider":
                SFXVolumeImageComponent.sprite = spriteToSet;
                break;
            case "AmbientSlider":
                ambientVolumeImageComponent.sprite = spriteToSet;
                break;
            case "MusicSlider":
                musicVolumeImageComponent.sprite = spriteToSet;
                break;
            default:
                break;
        }
    }
    #endregion

    #region Video and Graphic Settings Scripts

    #region Screen Resolution Scripts
    private void GetScreenResolutions()
    {
        screenResolutions = Screen.resolutions;
        screenResolutionDropdown.ClearOptions();
    }

    private void AddScreenResolutionsToDropdown()
    {
        List<string> screenResolutionOptions = new List<string>();
        foreach (var resolution in screenResolutions)
        {
            double roundedRate = Math.Round(resolution.refreshRateRatio.value, 2);
            string screenResolutionOption = resolution.width + " x " + resolution.height + " " + roundedRate + "Hz";
            screenResolutionOptions.Add(screenResolutionOption); // Convert options array to list for AddOptions dropdown method.
        }
        screenResolutionDropdown.AddOptions(screenResolutionOptions);
    }

    private int GetDefaultResolutionIndex()
    {
        for (int i = 0; i < screenResolutions.Length; i++)
        {
            if (screenResolutions[i].width == Screen.width && screenResolutions[i].height == Screen.height)
            {
                return i;
            }
        }
        return 0; // Return a default index if no match is found
    }

    public void SetScreenResolution(int resolutionIndex)
    {
        screenResolutionIndex = resolutionIndex;
        Resolution resolution = screenResolutions[screenResolutionIndex];

        switch (screenModeIndex)
        {
            case (0):
                Screen.SetResolution(resolution.width, resolution.height, FullScreenMode.ExclusiveFullScreen);
                break;
            case (1):
                Screen.SetResolution(resolution.width, resolution.height, FullScreenMode.FullScreenWindow);
                break;
            case (2):
                Screen.SetResolution(resolution.width, resolution.height, FullScreenMode.Windowed);
                break;
            default:
                Debug.LogError("There should only be 3 optiones in the resolution dropdown (these have to be integers)");
                break;
        }
        PlayerPrefs.SetInt("ScreenResolutionIndex", screenResolutionIndex); // Guardar el índice de la resolución seleccionada
        PlayerPrefs.Save();
    }
    #endregion
    #region Screen Mode Scripts
    private void AddScreenModeOptionsToDropdown()
    {
        screenModeDropdown.ClearOptions();
        screenModeDropdown.AddOptions(screenModeOptions);
    }

    public void SetScreenMode(int screenModeDropdownIndex)
    {
        screenModeIndex = screenModeDropdownIndex;
        switch (screenModeIndex)
        {
            case (0):
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                break;
            case (1):
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                break;
            case (2):
                Screen.fullScreenMode = FullScreenMode.Windowed;
                break;
            default:
                Debug.LogError("There should only be 3 optiones in the dropdown (these have to be integers)");
                break;
        }
        PlayerPrefs.SetInt("ScreenModeIndex", screenModeIndex); // Guardar el índice del modo de pantalla seleccionado
        PlayerPrefs.Save();
    }
    #endregion
    #region Graphics Quality scripts

    #region Stupid URP bullshit

    #region URP variables, getters and setters for the fields 
    // Please note that this code only works for the current render pipeline asset. As long as the different URP quality assets have the same render pipeline asset, you should be fine.
    private void InitializeHiddenPrivateFieldsURP() // Uses reflective programming to get URP fields that should be public (thank you @JimmyCushnie on GitHub)
    {
        // All the functions that use these fields are inspired or come from the following code: https://gist.github.com/JimmyCushnie/e998cdec15394d6b68a4dbbf700f66ce  
        var pipelineAssetType = typeof(UniversalRenderPipelineAsset);
        var flags = BindingFlags.Instance | BindingFlags.NonPublic;

        mainLightCastShadows_FieldInfo = pipelineAssetType.GetField("m_MainLightShadowsSupported", flags);
        additionalLightCastShadows_FieldInfo = pipelineAssetType.GetField("m_AdditionalLightShadowsSupported", flags);
        mainLightShadowmapResolution_FieldInfo = pipelineAssetType.GetField("m_MainLightShadowmapResolution", flags);
        additionalLightShadowmapResolution_FieldInfo = pipelineAssetType.GetField("m_AdditionalLightsShadowmapResolution", flags);
        cascade2Split_FieldInfo = pipelineAssetType.GetField("m_Cascade2Split", flags);
        cascade4Split_FieldInfo = pipelineAssetType.GetField("m_Cascade4Split", flags);
        softShadowsEnabled_FieldInfo = pipelineAssetType.GetField("m_SoftShadowsSupported", flags);
    }

    public bool MainLightCastShadows   // Implemented
    {
        get => (bool)mainLightCastShadows_FieldInfo.GetValue(GraphicsSettings.currentRenderPipeline);
        set => mainLightCastShadows_FieldInfo.SetValue(GraphicsSettings.currentRenderPipeline, value);
    }

    public bool AdditionalLightCastShadows   // Implemented
    {
        get => (bool)additionalLightCastShadows_FieldInfo.GetValue(GraphicsSettings.currentRenderPipeline);
        set => additionalLightCastShadows_FieldInfo.SetValue(GraphicsSettings.currentRenderPipeline, value);
    }

    public ShadowResolution MainLightShadowResolution
    {
        get => (ShadowResolution)mainLightShadowmapResolution_FieldInfo.GetValue(GraphicsSettings.currentRenderPipeline);
        set => mainLightShadowmapResolution_FieldInfo.SetValue(GraphicsSettings.currentRenderPipeline, value);
    }

    public ShadowResolution AdditionalLightShadowResolution
    {
        get => (ShadowResolution)additionalLightShadowmapResolution_FieldInfo.GetValue(GraphicsSettings.currentRenderPipeline);
        set => additionalLightShadowmapResolution_FieldInfo.SetValue(GraphicsSettings.currentRenderPipeline, value);
    }

    public float Cascade2Split
    {
        get => (float)cascade2Split_FieldInfo.GetValue(GraphicsSettings.currentRenderPipeline);
        set => cascade2Split_FieldInfo.SetValue(GraphicsSettings.currentRenderPipeline, value);
    }

    public Vector3 Cascade4Split
    {
        get => (Vector3)cascade4Split_FieldInfo.GetValue(GraphicsSettings.currentRenderPipeline);
        set => cascade4Split_FieldInfo.SetValue(GraphicsSettings.currentRenderPipeline, value);
    }

    public bool SoftShadowsEnabled    // Implemented
    {
        get => (bool)softShadowsEnabled_FieldInfo.GetValue(GraphicsSettings.currentRenderPipeline);
        set => softShadowsEnabled_FieldInfo.SetValue(GraphicsSettings.currentRenderPipeline, value);
    }
    #endregion
    #region URP Fields UI (Dropdown functions) [Getters and Setters for interaction with the UI] [Render Scale, Shadows Settings]

    #region Render Scale
    public float GetRenderScale() { return PlayerPrefs.HasKey("RenderScaleValue") ? PlayerPrefs.GetFloat("RenderScaleValue") : renderScaleSlider.value; }
    public void SetRenderScale()
    {
        PlayerPrefs.SetFloat("RenderScaleValue", renderScaleSlider.value); PlayerPrefs.Save();
        //changeText(renderScaleValueIndicator, renderScaleSlider.value.ToString());
        foreach (UniversalRenderPipelineAsset urpAsset in urpQualityAssetList) { if (urpAsset != null) { urpAsset.renderScale = renderScaleSlider.value; } }
    }
    #endregion

    #region Shadow settings

    public int GetShadowsState() { return PlayerPrefs.HasKey("ShadowsStateIndex") ? PlayerPrefs.GetInt("ShadowsStateIndex") : softShadowsIndex; }
    public void SetShadowsState(int shadowsStateIndex) // is this condition true ? yes : no  (ternary operator guide)
    {
        Debug.Log("Dame señales de vida");
        shadowsStateIndex = shadowsStateIndex < 1 ? 0 : 1;
        PlayerPrefs.SetInt("ShadowsStateIndex", shadowsStateIndex); PlayerPrefs.Save();
        MainLightCastShadows = shadowsStateIndex == 0;
        if (MainLightCastShadows) { EnableDropdown(softShadowsDropdown); } else { DisableDropdown(softShadowsDropdown, 1); }
        AdditionalLightCastShadows = shadowsStateIndex == 0;

    }

    // is this condition true ? yes : no  (ternary operator guide)
    public int GetSoftShadowsState() { return PlayerPrefs.HasKey("SoftShadowsIndex") ? PlayerPrefs.GetInt("SoftShadowsIndex") : softShadowsIndex; }
    
    public void SetSoftShadowsState(int softShadowsStateIndex) // is this condition true ? yes : no  (ternary operator guide)
    {
        softShadowsStateIndex  = softShadowsStateIndex < 1 ? 0 : 1;
        PlayerPrefs.SetInt("SoftShadowsIndex", softShadowsStateIndex); PlayerPrefs.Save();
        SoftShadowsEnabled = softShadowsStateIndex == 0;
        Light[] allLightsScene = Resources.FindObjectsOfTypeAll<Light>(); // Get all Light components in the scene, including inactive ones
        foreach (Light light in allLightsScene) { light.shadows = LightShadows.Soft; } // all modes are: no shadows, hard shadows, soft shadows. But everything is handled through the URP scriptable object
        //Debug.Log("Light found: " + light.gameObject.name + " - Type: " + light.type + " - Active: " + light.gameObject.activeInHierarchy + "- Shadows mode: "+ light.shadows);

    }
    #endregion
    #endregion
    #endregion
    private void GetUrpAssets()
    {
        urpQualityAssetList = new List<UniversalRenderPipelineAsset>();
        var originalLevel = screenQualityLevelIndex;
        for (int i=0; i < QualitySettings.names.Length;  i++)
        {
            SetQualityLevel(i);
            var currentLevel = QualitySettings.GetQualityLevel();
            var asset = QualitySettings.GetRenderPipelineAssetAt(currentLevel) as UniversalRenderPipelineAsset;
            //var assetRenderer = asset.scriptableRenderer;
            //assetRenderer = asset.GetRenderer(0);
            //urpQualityAssetList[i] = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
            urpQualityAssetList.Add(asset);
        }

        //urpQualityAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
        //urpQualityAsset = asset;
        SetQualityLevel(originalLevel);
    }
    private void GetCamerasOnScene()
    {
        camerasOnSceneArray = new Camera[Camera.allCamerasCount];
        Camera.GetAllCameras(camerasOnSceneArray);
        if (camerasOnSceneArray.Length < 1)
        {
            Debug.LogError("There are not cameras in the scene.");
        }
    }
    #region Antialiasing
    // if theres a saved value get it, use default value otherwise
    // is this condition true ? yes : no  (ternary operator guide)
    public int GetAntialiasingQuality()
    { return PlayerPrefs.HasKey("AntialiasingIndex") ? PlayerPrefs.GetInt("AntialiasingIndex") : antialiasingIndex; }
    public int GetAntialiasingMSAAQuality()
    { return PlayerPrefs.HasKey("MSAAIndex") ? PlayerPrefs.GetInt("MSAAIndex") : msaaIndex; }

    public void SetAntialiasingQuality(int antialiasQualityIndex)
    {
        Debug.Log("Indice antialiasing: " + antialiasQualityIndex);

        PlayerPrefs.SetInt("AntialiasingIndex", antialiasQualityIndex);
        PlayerPrefs.Save();

        bool isMsaaEnabled = antialiasQualityIndex != 3; // TAA requires MSAA to be off
        if (isMsaaEnabled)
        {
            EnableDropdown(antialiasingMSAADropdown);
        }
        else
        {
            DisableDropdown(antialiasingMSAADropdown, 0);
        }

        foreach (var camera in camerasOnSceneArray)
        {
            var cameraData = camera.GetComponent<UniversalAdditionalCameraData>();
            if (cameraData == null)
            {
                Debug.LogWarning("Camera does not have UniversalAdditionalCameraData component attached.");
                continue;
            }

            cameraData.renderPostProcessing = true;
            SetAntialiasingMode(cameraData, antialiasQualityIndex);
        }
    }

    private void SetAntialiasingMode(UniversalAdditionalCameraData cameraData, int antialiasQualityIndex)
    {
        switch (antialiasQualityIndex)
        {
            case 0: // off
                cameraData.antialiasing = AntialiasingMode.None;
                Debug.Log("Antialiasing Apagado");
                break;
            case 1: // FXAA (compatible with MSAA)
                cameraData.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
                cameraData.antialiasingQuality = AntialiasingQuality.High;
                Debug.Log("FXAA Encendido");
                break;
            case 2: // SMAA (compatible with MSAA)
                cameraData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                cameraData.antialiasingQuality = AntialiasingQuality.High;
                Debug.Log("SMAA Encendido");
                break;
            case 3: // TAA (turn off MSAA)
                cameraData.antialiasing = AntialiasingMode.TemporalAntiAliasing;
                cameraData.antialiasingQuality = AntialiasingQuality.High;
                Debug.Log("TAA Encendido");
                break;
            default:
                Debug.LogWarning("Invalid antialiasing quality index.");
                break;
        }
    }


    public void SetAntialiasingMSAAQuality(int antialiasQualityMSAAIndex)
    {
        Debug.Log("Indice MSAA: " + antialiasQualityMSAAIndex);

        bool allowMSAA = antialiasQualityMSAAIndex > 0;
        foreach (var camera in camerasOnSceneArray)
        {
            if (camera == null)
            {
                Debug.LogWarning("Camera is missing from the array.");
                continue;
            }

            camera.allowMSAA = allowMSAA;
        }

        int sampleCount;
        if      (antialiasQualityMSAAIndex == 0) { sampleCount = 1; }// MSAA Off
        else if (antialiasQualityMSAAIndex > 0 && antialiasQualityMSAAIndex < 4) { sampleCount = (int)Mathf.Pow(2, antialiasQualityMSAAIndex); }  // MSAA x2, x4, x8
        else { Debug.LogError("MSAA Dropdown should only have 4 parameters, but received another index."); return; }
        
        foreach (UniversalRenderPipelineAsset urpAsset in urpQualityAssetList){ if (urpAsset != null){ urpAsset.msaaSampleCount = sampleCount; } }
        PlayerPrefs.SetInt("MSAAIndex", antialiasQualityMSAAIndex);
        PlayerPrefs.Save();
    }
    #endregion



    #region URP QUALITY ASSETS

    /*private void AddVideoQualitiesToDropdown() //Method not used on our game
    {
        qualityLevels.AddRange(QualitySettings.names); // get the list of qualities in player settings tab of unity
        qualityLevelsDropdown.ClearOptions();
        qualityLevelsDropdown.AddOptions(qualityLevels); // puts the names of the qualities in the dropdown
    }*/
    public void SetQualityLevel(int qualityLevelIndex)
    {
        screenQualityLevelIndex = qualityLevelIndex;
        QualitySettings.SetQualityLevel(qualityLevelIndex);
        PlayerPrefs.SetInt("QualityLevelIndex", qualityLevelIndex); // Guardar el índice de la resolución seleccionada
        PlayerPrefs.Save();
    }
    #endregion
    #endregion

    #endregion
    #region General dropdown Methods (ENABLE, DISABLE, SET DROPDOWN STATE)
    private void SetDropdownState(TMP_Dropdown dropdown, bool isEnabled, float colorAlpha, int? indexToSet = null)
    {
        if (indexToSet.HasValue) // Sets the index to dropdown if provided.
        {
            dropdown.value = indexToSet.Value;
        }

        Image dropdownImageComponent = dropdown.gameObject.GetComponent<Image>(); // Get the Image component for the dropdown background
        TextMeshProUGUI dropdownLabelComponent = dropdown.gameObject.GetComponentInChildren<TextMeshProUGUI>(); // Get the TextMeshProUGUI component for the dropdown label
        // Set the color with the specified alpha to indicate enabled or disabled state
        dropdownImageComponent.color = new Color(dropdownImageComponent.color.r, dropdownImageComponent.color.g, dropdownImageComponent.color.b, colorAlpha);
        dropdownLabelComponent.color = new Color(dropdownLabelComponent.color.r, dropdownLabelComponent.color.g, dropdownLabelComponent.color.b, colorAlpha);
        dropdown.interactable = isEnabled; // Set the dropdown interactability
    }
    private void EnableDropdown(TMP_Dropdown dropdown, int? indexToSet = null)
    {
        SetDropdownState(dropdown, true, 1f, indexToSet); // Enables the dropdown, making it interactable and visually indicating enabled state.
    }

    private void DisableDropdown(TMP_Dropdown dropdown, int? indexToSet = null)
    {
        SetDropdownState(dropdown, false, 0.3f, indexToSet); // Disables the dropdown, making it non-interactable and visually indicating disabled state.
    }

    #endregion

    #region Change Text
    private void changeText(GameObject _objectTMP, string _text)
    {
        _objectTMP.GetComponent<TextMeshPro>().text = _text;
    }
    #endregion
}
