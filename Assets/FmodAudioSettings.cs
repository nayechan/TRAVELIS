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
        // Delay driver population to ensure FMOD is properly initialized
        Invoke("PopulateASIODrivers", 1.0f);
    }

    public void PopulateASIODrivers()
    {
        AvailableASIODrivers.Clear();
        
        if (RuntimeManager.CoreSystem.hasHandle())
        {
            // Store current output mode
            OUTPUTTYPE originalOutput;
            RuntimeManager.CoreSystem.getOutput(out originalOutput);

            // Temporarily set output to ASIO to properly enumerate ASIO devices
            RuntimeManager.CoreSystem.setOutput(OUTPUTTYPE.ASIO);
            
            // Now get the number of drivers (should include ASIO devices)
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
                
                // In ASIO mode, all drivers should be ASIO-compatible
                AvailableASIODrivers.Add(name);
            }
            
            // Restore original output mode
            RuntimeManager.CoreSystem.setOutput(originalOutput);

            Debug.Log($"ASIO drivers found: {AvailableASIODrivers.Count}");
        }
        else
        {
            Debug.LogWarning("FMOD system not initialized when attempting to populate ASIO drivers");
        }
    }

    // Apply the audio output settings - call this before FMOD fully initializes
    public void ApplyOutputSettings()
    {
        if (!RuntimeManager.CoreSystem.hasHandle())
            return;

        try
        {
            // First properly close the system
            RuntimeManager.CoreSystem.close();

            // Set the output type based on user preference
            switch (CurrentOutputMode)
            {
                case AudioOutputMode.WASAPI:
                    RuntimeManager.CoreSystem.setOutput(OUTPUTTYPE.WASAPI);
                    break;
                    
                case AudioOutputMode.ASIO:
                    RuntimeManager.CoreSystem.setOutput(OUTPUTTYPE.ASIO);
                    // Set the specific ASIO driver if we have one selected
                    if (CurrentASIODriverIndex < AvailableASIODrivers.Count && CurrentASIODriverIndex >= 0)
                    {
                        RuntimeManager.CoreSystem.setDriver(CurrentASIODriverIndex);
                    }
                    break;
                    
                default:
                    RuntimeManager.CoreSystem.setOutput(OUTPUTTYPE.AUTODETECT);
                    break;
            }

            // Reinitialize with the new settings
            RESULT result = RuntimeManager.CoreSystem.init(
                512, INITFLAGS.NORMAL, System.IntPtr.Zero);
                
            if (result != RESULT.OK)
            {
                // Failed to initialize with these settings, revert to defaults
                Debug.LogError($"Failed to initialize FMOD with selected output mode. Error: {result}");
                CurrentOutputMode = AudioOutputMode.Default;
                RuntimeManager.CoreSystem.setOutput(OUTPUTTYPE.AUTODETECT);
                RuntimeManager.CoreSystem.init(512, INITFLAGS.NORMAL, System.IntPtr.Zero);
            }
            else
            {
                Debug.Log($"Successfully initialized FMOD with output mode: {CurrentOutputMode}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error applying audio settings: {e.Message}");
            
            // Try to recover
            try
            {
                CurrentOutputMode = AudioOutputMode.Default;
                RuntimeManager.CoreSystem.setOutput(OUTPUTTYPE.AUTODETECT);
                RuntimeManager.CoreSystem.init(512, INITFLAGS.NORMAL, System.IntPtr.Zero);
            }
            catch
            {
                Debug.LogError("Failed to recover FMOD system after error");
            }
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