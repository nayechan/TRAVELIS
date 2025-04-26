using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD;

using Debug = UnityEngine.Debug;

public class FMODAudioSettings : MonoBehaviour
{
    // Singleton pattern for easy access
    public static FMODAudioSettings Instance { get; private set; }

    // Output mode options
    public enum AudioOutputMode
    {
        Default,
        WASAPI,
        ASIO
    }

    // Current selected output mode (default to system default)
    private AudioOutputMode _currentOutputMode = AudioOutputMode.Default;
    public AudioOutputMode CurrentOutputMode 
    {
        get { return _currentOutputMode; }
        set 
        { 
            _currentOutputMode = value;
            SaveSettings();
        }
    }

    // List to store available ASIO drivers
    public List<string> AvailableASIODrivers { get; private set; } = new List<string>();
    public int CurrentASIODriverIndex { get; private set; } = 0;

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Wait for FMOD to be fully initialized by other systems
        Invoke("PopulateASIODrivers", 1.0f);
    }

    public void PopulateASIODrivers()
    {
        AvailableASIODrivers.Clear();
        
        if (!RuntimeManager.CoreSystem.hasHandle())
        {
            Debug.LogWarning("FMOD system not initialized when attempting to populate ASIO drivers");
            return;
        }
        
        try
        {
            // Store current output mode
            OUTPUTTYPE originalOutput;
            RuntimeManager.CoreSystem.getOutput(out originalOutput);

            // We need to be careful NOT to call close() here, just change the output type
            RuntimeManager.CoreSystem.setOutput(OUTPUTTYPE.ASIO);
            
            // Now get the number of drivers
            int numDrivers = 0;
            RuntimeManager.CoreSystem.getNumDrivers(out numDrivers);
            
            Debug.Log($"Total drivers found in ASIO mode: {numDrivers}");
            
            for (int i = 0; i < numDrivers; i++)
            {
                // Get driver info
                string name = "";
                System.Guid guid = System.Guid.Empty;
                int systemRate = 0;
                SPEAKERMODE speakerMode = SPEAKERMODE.DEFAULT;
                int speakerModeChannels = 0;
                
                RuntimeManager.CoreSystem.getDriverInfo(
                    i, out name, 256, out guid, out systemRate, out speakerMode, out speakerModeChannels);
                
                Debug.Log($"ASIO Driver {i}: '{name}' - System Rate: {systemRate}");
                AvailableASIODrivers.Add(name);
            }
            
            // Restore original output mode without closing the system
            RuntimeManager.CoreSystem.setOutput(originalOutput);

            Debug.Log($"ASIO drivers found: {AvailableASIODrivers.Count}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error populating ASIO drivers: {e.Message}");
        }
    }

    // IMPORTANT: This should ONLY be called at application startup BEFORE FMOD is initialized
    // Do not call this after FMOD is initialized or during runtime
    public void ApplyOutputSettings()
    {
        FMOD.System coreSystem = RuntimeManager.CoreSystem;
        
        if (!coreSystem.hasHandle())
        {
            Debug.LogError("Cannot apply FMOD settings to an invalid system");
            return;
        }

        try
        {
            // Set the output type based on user preference
            switch (CurrentOutputMode)
            {
                case AudioOutputMode.WASAPI:
                    coreSystem.setOutput(OUTPUTTYPE.WASAPI);
                    break;
                    
                case AudioOutputMode.ASIO:
                    coreSystem.setOutput(OUTPUTTYPE.ASIO);
                    // Set the specific ASIO driver if we have one selected
                    if (CurrentASIODriverIndex < AvailableASIODrivers.Count && CurrentASIODriverIndex >= 0)
                    {
                        coreSystem.setDriver(CurrentASIODriverIndex);
                    }
                    break;
                    
                default:
                    coreSystem.setOutput(OUTPUTTYPE.AUTODETECT);
                    break;
            }
            
            Debug.Log($"Applied audio output settings: {CurrentOutputMode}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error applying audio settings: {e.Message}");
        }
    }

    // Create a method to safely change the output during runtime (if needed)
    public bool ChangeOutputDuringRuntime(FMOD.System coreSystem)
    {
        if (!coreSystem.hasHandle())
        {
            Debug.LogError("Cannot change output of an invalid FMOD system");
            return false;
        }

        try
        {
            // Get current output type
            OUTPUTTYPE currentOutput;
            coreSystem.getOutput(out currentOutput);
            
            // Set new output type
            OUTPUTTYPE newOutput = OUTPUTTYPE.AUTODETECT;
            switch (CurrentOutputMode)
            {
                case AudioOutputMode.WASAPI:
                    newOutput = OUTPUTTYPE.WASAPI;
                    break;
                case AudioOutputMode.ASIO:
                    newOutput = OUTPUTTYPE.ASIO;
                    break;
                default:
                    newOutput = OUTPUTTYPE.AUTODETECT;
                    break;
            }
            
            // Only change if different
            if (currentOutput != newOutput)
            {
                RESULT result = coreSystem.setOutput(newOutput);
                if (result != RESULT.OK)
                {
                    Debug.LogError($"Failed to change output type: {result}");
                    return false;
                }
                
                // If ASIO, set the driver
                if (newOutput == OUTPUTTYPE.ASIO && 
                    CurrentASIODriverIndex >= 0 && 
                    CurrentASIODriverIndex < AvailableASIODrivers.Count)
                {
                    result = coreSystem.setDriver(CurrentASIODriverIndex);
                    if (result != RESULT.OK)
                    {
                        Debug.LogError($"Failed to set ASIO driver: {result}");
                        return false;
                    }
                }
            }
            
            Debug.Log($"Successfully changed output to {CurrentOutputMode}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error changing output: {e.Message}");
            return false;
        }
    }

    // Set the ASIO driver by index
    public void SetASIODriver(int index)
    {
        if (index >= 0 && index < AvailableASIODrivers.Count)
        {
            CurrentASIODriverIndex = index;
            SaveSettings();
        }
    }

    // Save settings to PlayerPrefs
    private void SaveSettings()
    {
        PlayerPrefs.SetInt("FMODAudioOutputMode", (int)CurrentOutputMode);
        PlayerPrefs.SetInt("FMODASIODriverIndex", CurrentASIODriverIndex);
        PlayerPrefs.Save();
    }

    // Load settings from PlayerPrefs
    private void LoadSettings()
    {
        if (PlayerPrefs.HasKey("FMODAudioOutputMode"))
        {
            _currentOutputMode = (AudioOutputMode)PlayerPrefs.GetInt("FMODAudioOutputMode");
        }
        
        if (PlayerPrefs.HasKey("FMODASIODriverIndex"))
        {
            CurrentASIODriverIndex = PlayerPrefs.GetInt("FMODASIODriverIndex");
        }
    }
}