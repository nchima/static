using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsScreen : MonoBehaviour {

    [SerializeField] Slider overallSensitivitySlider;
    [SerializeField] Slider XSensitivitySlider;
    [SerializeField] Slider YSensitivitySlider;
    [SerializeField] Slider masterVolumeSlider;
    [SerializeField] Slider musicVolumeSlider;
    [SerializeField] Slider sfxVolumeSlider;

    private void OnEnable() {
        overallSensitivitySlider.value = InputManager.mouseSensitivityOverall;
        XSensitivitySlider.value = InputManager.mouseSensitivityXMod;
        YSensitivitySlider.value = InputManager.mouseSensitivityYMod;
    }

    private void Update() {
        if (InputManager.cancelButtonDown || InputManager.pauseButtonDown) {
            Services.uiManager.ShowOptionsScreen(false);
        }
    }

    public void OverallSensitivityChanged() {
        InputManager.mouseSensitivityOverall = overallSensitivitySlider.value;
        InputManager.SaveSettings();
    }

    public void XSensitivityChanged() {
        InputManager.mouseSensitivityXMod = XSensitivitySlider.value;
        InputManager.SaveSettings();
    }

    public void YSensitivityChanged() {
        InputManager.mouseSensitivityYMod = overallSensitivitySlider.value;
        InputManager.SaveSettings();
    }

    public void MasterVolumeChanged() {
        Services.musicManager.SetMasterVolume(masterVolumeSlider.value);
    }

    public void MusicVolumeChanged() {
        Services.musicManager.SetMusicVolume(musicVolumeSlider.value);
    }

    public void SFXVolumeChanged() {
        Services.sfxManager.SetVolume(sfxVolumeSlider.value);
    }
}
