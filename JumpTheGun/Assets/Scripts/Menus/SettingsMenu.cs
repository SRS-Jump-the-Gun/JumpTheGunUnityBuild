using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private Slider mouseSensSlider;
    [SerializeField] private Slider fovSlider;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TextMeshProUGUI volumeValueText;

    private void Start()
    {
        mouseSensSlider.value = PlayerPrefs.GetFloat("MouseSens", 2f);
        fovSlider.value       = PlayerPrefs.GetFloat("FOV", 60f);
        volumeSlider.value    = PlayerPrefs.GetFloat("Volume", 0.5f);

        AudioListener.volume = volumeSlider.value;

        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        UpdateVolumeText(volumeSlider.value);
    }

    private void OnVolumeChanged(float value)
    {
        AudioListener.volume = value;
        UpdateVolumeText(value);
    }

    private void UpdateVolumeText(float value)
    {
        volumeValueText.text = Mathf.RoundToInt(value * 100f) + "%";
    }

    public void Apply()
    {
        PlayerPrefs.SetFloat("MouseSens", mouseSensSlider.value);
        PlayerPrefs.SetFloat("FOV", fovSlider.value);
        PlayerPrefs.SetFloat("Volume", volumeSlider.value);
        PlayerPrefs.Save();
    }
}
