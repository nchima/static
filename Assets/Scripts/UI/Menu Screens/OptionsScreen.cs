using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsScreen : MonoBehaviour {

    [SerializeField] int numericalValueDecimalPlaces = 2;

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
        overallSensitivitySlider.GetComponentInChildren<Text>().text = MyMath.RoundToDecimalPlaces(overallSensitivitySlider.value, numericalValueDecimalPlaces).ToString();
    }

    public void XSensitivityChanged() {
        InputManager.mouseSensitivityXMod = XSensitivitySlider.value;
        InputManager.SaveSettings();
        XSensitivitySlider.GetComponentInChildren<Text>().text = MyMath.RoundToDecimalPlaces(XSensitivitySlider.value, numericalValueDecimalPlaces).ToString();
    }

    public void YSensitivityChanged() {
        InputManager.mouseSensitivityYMod = YSensitivitySlider.value;
        InputManager.SaveSettings();
        YSensitivitySlider.GetComponentInChildren<Text>().text = MyMath.RoundToDecimalPlaces(YSensitivitySlider.value, numericalValueDecimalPlaces).ToString();
    }

    public void MasterVolumeChanged() {
        Services.musicManager.SetMasterVolume(masterVolumeSlider.value);
        masterVolumeSlider.GetComponentInChildren<Text>().text = MyMath.RoundToDecimalPlaces(masterVolumeSlider.value, numericalValueDecimalPlaces).ToString();
    }

    public void MusicVolumeChanged() {
        Services.musicManager.SetMusicVolume(musicVolumeSlider.value);
        musicVolumeSlider.GetComponentInChildren<Text>().text = MyMath.RoundToDecimalPlaces(musicVolumeSlider.value, numericalValueDecimalPlaces).ToString();
    }

    public void SFXVolumeChanged() {
        Services.sfxManager.SetVolume(sfxVolumeSlider.value);
        sfxVolumeSlider.GetComponentInChildren<Text>().text = MyMath.RoundToDecimalPlaces(sfxVolumeSlider.value, numericalValueDecimalPlaces).ToString();
    }
}
