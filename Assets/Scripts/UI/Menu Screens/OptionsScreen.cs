using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsScreen : MonoBehaviour {

    [SerializeField] int numericalValueDecimalPlaces = 2;

    [SerializeField] Slider overallSensitivitySlider;
    [SerializeField] Slider XSensitivitySlider;
    [SerializeField] Slider YSensitivitySlider;
    [SerializeField] Slider controllerSensitivitySlider;
    [SerializeField] Slider controllerXSensitivitySlider;
    [SerializeField] Slider controllerYSensitivitySlider;
    [SerializeField] Slider masterVolumeSlider;
    [SerializeField] Slider musicVolumeSlider;
    [SerializeField] Slider sfxVolumeSlider;

    bool isUsingControllerInput = false;

    private void OnEnable() {
        overallSensitivitySlider.value = InputManager.mouseSensitivityOverall;
        XSensitivitySlider.value = InputManager.mouseSensitivityXMod;
        YSensitivitySlider.value = InputManager.mouseSensitivityYMod;
        controllerSensitivitySlider.value = InputManager.controllerSensitivityOverall;
        controllerXSensitivitySlider.value = InputManager.controllerSensitivityXMod;
        controllerYSensitivitySlider.value = InputManager.controllerSensitivityYMod;

        isUsingControllerInput = false;
    }

    private void OnDisable() {
        isUsingControllerInput = false;
    }

    private void Update() {
        if (InputManager.cancelButtonDown || InputManager.pauseButtonDown) {
            Services.uiManager.ShowOptionsScreen(false);
        }

        if (isUsingControllerInput && InputManager.inputMode != InputManager.InputMode.Controller) {
            isUsingControllerInput = false;
        }

        else if (!isUsingControllerInput && InputManager.inputMode == InputManager.InputMode.Controller) {
            overallSensitivitySlider.Select();
            isUsingControllerInput = true;
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

    public void OverallControllerSensitivityChanged() {
        InputManager.controllerSensitivityOverall = controllerSensitivitySlider.value;
        InputManager.SaveSettings();
        controllerSensitivitySlider.GetComponentInChildren<Text>().text = MyMath.RoundToDecimalPlaces(controllerSensitivitySlider.value, numericalValueDecimalPlaces).ToString();
    }

    public void ControllerXSensitivityChanged() {
        InputManager.controllerSensitivityXMod = controllerXSensitivitySlider.value;
        InputManager.SaveSettings();
        controllerXSensitivitySlider.GetComponentInChildren<Text>().text = MyMath.RoundToDecimalPlaces(controllerXSensitivitySlider.value, numericalValueDecimalPlaces).ToString();
    }

    public void ControllerYSensitivityChanged() {
        InputManager.controllerSensitivityYMod = controllerYSensitivitySlider.value;
        InputManager.SaveSettings();
        controllerYSensitivitySlider.GetComponentInChildren<Text>().text = MyMath.RoundToDecimalPlaces(controllerYSensitivitySlider.value, numericalValueDecimalPlaces).ToString();
    }


    public void MasterVolumeChanged() {
        Services.musicManager.SetMasterVolumeSliderValue(masterVolumeSlider.value);
        masterVolumeSlider.GetComponentInChildren<Text>().text = MyMath.RoundToDecimalPlaces(masterVolumeSlider.value, numericalValueDecimalPlaces).ToString();
    }

    public void MusicVolumeChanged() {
        Services.musicManager.SetMusicVolumeSliderValue(musicVolumeSlider.value);
        musicVolumeSlider.GetComponentInChildren<Text>().text = MyMath.RoundToDecimalPlaces(musicVolumeSlider.value, numericalValueDecimalPlaces).ToString();
    }

    public void SFXVolumeChanged() {
        Services.sfxManager.SetVolume(sfxVolumeSlider.value);
        sfxVolumeSlider.GetComponentInChildren<Text>().text = MyMath.RoundToDecimalPlaces(sfxVolumeSlider.value, numericalValueDecimalPlaces).ToString();
    }
}
