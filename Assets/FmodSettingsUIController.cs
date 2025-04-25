using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class FMODSettingsUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Dropdown outputModeDropdown;
    [SerializeField] private TMP_Dropdown asioDriverDropdown;
    [SerializeField] private GameObject asioDriverPanel;
    [SerializeField] private Button applyButton;

    private FMODAudioSettings _audioSettings;
    
    private void Start()
    {
        _audioSettings = FMODAudioSettings.Instance;
        if (_audioSettings == null)
        {
            Debug.LogError("FMODAudioSettings instance not found!");
            return;
        }
        
        InitializeUI();
    }
    
    private void InitializeUI()
    {
        // Setup output mode dropdown
        outputModeDropdown.ClearOptions();
        List<string> outputOptions = new List<string>()
        {
            "System Default",
            "WASAPI",
            "ASIO"
        };
        outputModeDropdown.AddOptions(outputOptions);
        outputModeDropdown.value = (int)_audioSettings.CurrentOutputMode;
        outputModeDropdown.onValueChanged.AddListener(OnOutputModeChanged);
        
        // Setup ASIO driver dropdown
        RefreshASIODrivers();
        asioDriverDropdown.onValueChanged.AddListener(OnASIODriverChanged);
        
        // Show/hide ASIO panel based on current selection
        UpdateASIOPanelVisibility();
        
        // Setup apply button
        applyButton.onClick.AddListener(OnApplyClicked);
    }
    
    private int RefreshASIODrivers()
    {
        asioDriverDropdown.ClearOptions();
        
        if (_audioSettings.AvailableASIODrivers.Count > 0)
        {
            asioDriverDropdown.AddOptions(_audioSettings.AvailableASIODrivers);
            asioDriverDropdown.value = _audioSettings.CurrentASIODriverIndex;
            asioDriverDropdown.interactable = true;
            return _audioSettings.AvailableASIODrivers.Count;
        }
        else
        {
            asioDriverDropdown.AddOptions(new List<string> { "No ASIO drivers found" });
            asioDriverDropdown.interactable = false;
            return 0;
        }
    }
    
    private void OnOutputModeChanged(int value)
    {
        applyButton.interactable = true;
        
        FMODAudioSettings.AudioOutputMode selectedMode = (FMODAudioSettings.AudioOutputMode)value;
        _audioSettings.CurrentOutputMode = selectedMode;
        UpdateASIOPanelVisibility();
    }
    
    private void OnASIODriverChanged(int index)
    {
        _audioSettings.SetASIODriver(index);
    }
    
    private void UpdateASIOPanelVisibility()
    {
        if (_audioSettings.CurrentOutputMode == FMODAudioSettings.AudioOutputMode.ASIO)
        {
            asioDriverPanel.SetActive(true);
            
            _audioSettings.PopulateASIODrivers();
            int asioDriverCount = RefreshASIODrivers();
            if (asioDriverCount == 0)
            {
                applyButton.interactable = false;
            }
        }
        else
        {
            asioDriverPanel.SetActive(false);
        }
    }
    
    private void OnApplyClicked()
    {
        _audioSettings.ApplyOutputSettings();
    }
}