using UnityEngine;
using TMPro;
public class QualityOptions : MonoBehaviour
{
    public TMP_Dropdown optionsDropdown;
    void Start()
    {
        // Clear existing options
        optionsDropdown.ClearOptions();

        // Get quality level names
        string[] qualityNames = QualitySettings.names;

        // Add options to dropdown
        optionsDropdown.AddOptions(new System.Collections.Generic.List<string>(qualityNames));

        // Set current value
        optionsDropdown.value = QualitySettings.GetQualityLevel();
        optionsDropdown.RefreshShownValue();

        // Add listener
        optionsDropdown.onValueChanged.AddListener(SetQuality);

        int savedQuality = PlayerPrefs.GetInt("QualitySetting", QualitySettings.GetQualityLevel());
        optionsDropdown.value = savedQuality;
        optionsDropdown.RefreshShownValue();
        QualitySettings.SetQualityLevel(savedQuality);

        DontDestroyOnLoad(gameObject);
    }

    void SetQuality(int index)
    {
        QualitySettings.SetQualityLevel(index, true);
        PlayerPrefs.SetInt("QualitySetting", index);
        PlayerPrefs.Save();
    }
}