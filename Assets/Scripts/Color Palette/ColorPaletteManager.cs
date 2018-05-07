using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ColorPaletteManager : MonoBehaviour {

    // Environment materials.
    [SerializeField] Material wallMaterial;
    [SerializeField] Material floorMaterial;
    [SerializeField] Material obstacleMaterial;

    // Screen materials
    [SerializeField] GameObject perspectiveScreen1;
    [SerializeField] GameObject perspectiveScreen2;
    [SerializeField] GameObject perspectiveScreen3;
    [SerializeField] GameObject orthoScreen1;
    [SerializeField] GameObject orthoScreen2;
    [SerializeField] GameObject orthoScreen3;
    [SerializeField] GameObject backgroundScreen;

    [SerializeField] ColorPalette defaultPalette;
    [SerializeField] ColorPalette savedPalette;
    [SerializeField] ColorPalette tempSavedPalette;
    [SerializeField] ColorPalette playerVulnerablePalette;
    [SerializeField] ColorPalette fallingSequencePalette;
 
    private void Awake() {
        // Memorize original colors.
        ChangePaletteImmediate(defaultPalette);
        GameEventManager.instance.Subscribe<GameEvents.PlayerWasHurt>(PlayerWasHurtHandler);
        GameEventManager.instance.Subscribe<GameEvents.LevelCompleted>(LevelCompletedHandler);
    }

    private void Start() {
        SaveCurrentPalette(savedPalette);
    }

    void ChangePalette(ColorPalette newPalette, float duration) {
        wallMaterial.DOColor(newPalette.wallColor, duration);
        floorMaterial.DOColor(newPalette.floorColor, duration);
        obstacleMaterial.DOColor(newPalette.obstacleColor, duration);
        perspectiveScreen1.GetComponent<MeshRenderer>().material.DOColor(newPalette.perspectiveColor1, duration);
        perspectiveScreen2.GetComponent<MeshRenderer>().material.DOColor(newPalette.perspectiveColor2, duration);
        perspectiveScreen3.GetComponent<MeshRenderer>().material.DOColor(newPalette.perspectiveColor3, duration);
        orthoScreen1.GetComponent<MeshRenderer>().material.DOColor(newPalette.orthoColor1, duration);
        orthoScreen2.GetComponent<MeshRenderer>().material.DOColor(newPalette.orthoColor2, duration);
        orthoScreen3.GetComponent<MeshRenderer>().material.DOColor(newPalette.orthoColor3, duration);
        backgroundScreen.GetComponent<MeshRenderer>().material.DOColor(newPalette.backgroundColor, duration);
    }

    void ChangePaletteImmediate(ColorPalette newPalette) {
        wallMaterial.color = newPalette.wallColor;
        floorMaterial.color = newPalette.floorColor;
        obstacleMaterial.color = newPalette.obstacleColor;
        perspectiveScreen1.GetComponent<MeshRenderer>().material.color = newPalette.perspectiveColor1;
        perspectiveScreen2.GetComponent<MeshRenderer>().material.color = newPalette.perspectiveColor2;
        perspectiveScreen3.GetComponent<MeshRenderer>().material.color = newPalette.perspectiveColor3;
        orthoScreen1.GetComponent<MeshRenderer>().material.color = newPalette.orthoColor1;
        orthoScreen2.GetComponent<MeshRenderer>().material.color = newPalette.orthoColor2;
        orthoScreen3.GetComponent<MeshRenderer>().material.color = newPalette.orthoColor3;
        backgroundScreen.GetComponent<MeshRenderer>().material.color = newPalette.backgroundColor;
    }

    void SaveCurrentPalette(ColorPalette saveTarget) {
        saveTarget.wallColor = wallMaterial.color;
        saveTarget.floorColor = floorMaterial.color;
        saveTarget.obstacleColor = obstacleMaterial.color;
        saveTarget.perspectiveColor1 = perspectiveScreen1.GetComponent<MeshRenderer>().material.color;
        saveTarget.perspectiveColor2 = perspectiveScreen2.GetComponent<MeshRenderer>().material.color;
        saveTarget.perspectiveColor3 = perspectiveScreen3.GetComponent<MeshRenderer>().material.color;
        saveTarget.orthoColor1 = orthoScreen1.GetComponent<MeshRenderer>().material.GetColor("_Color");
        saveTarget.orthoColor2 = orthoScreen2.GetComponent<MeshRenderer>().material.GetColor("_Color");
        saveTarget.orthoColor3 = orthoScreen3.GetComponent<MeshRenderer>().material.GetColor("_Color");
        saveTarget.backgroundColor = backgroundScreen.GetComponent<MeshRenderer>().material.color;
    }

    public void ChangeToRandomPalette(float duration) {
        StartCoroutine(ChangeToRandomPaletteCoroutine(duration));
    }

    IEnumerator ChangeToRandomPaletteCoroutine(float duration) {
        Color[] environmentPalette = ThreeColorPalette(0.33f, 0.2f);
        wallMaterial.DOColor(environmentPalette[0], duration);
        floorMaterial.DOColor(environmentPalette[1], duration);
        obstacleMaterial.DOColor(environmentPalette[2], duration);

        Color[] perspectiveScreenPalette = ThreeColorPalette(0.33f, 0.2f);
        perspectiveScreen1.GetComponent<MeshRenderer>().material.DOColor(perspectiveScreenPalette[0], duration);
        perspectiveScreen2.GetComponent<MeshRenderer>().material.DOColor(perspectiveScreenPalette[1], duration);
        perspectiveScreen3.GetComponent<MeshRenderer>().material.DOColor(perspectiveScreenPalette[2], duration);

        Color[] orthoScreenPalette = ThreeColorPalette(0.33f, 0.2f);
        orthoScreen1.GetComponent<MeshRenderer>().material.DOColor(orthoScreenPalette[0], duration);
        orthoScreen2.GetComponent<MeshRenderer>().material.DOColor(orthoScreenPalette[1], duration);
        orthoScreen3.GetComponent<MeshRenderer>().material.DOColor(orthoScreenPalette[2], duration);

        backgroundScreen.GetComponent<MeshRenderer>().material.color = randomBackgroundColor;

        yield return new WaitForSeconds(duration);

        SaveCurrentPalette(savedPalette);

        yield return null;
    }

    Color randomBackgroundColor { get { return Random.ColorHSV(0f, 1f, 0f, 1f, 0f, 0.75f); } }
    Color[] ThreeColorPalette(float baseHueDifference, float maxVariation) {
        float hue1 = Random.value;
        float hue2 = MyMath.Wrap01(hue1 + baseHueDifference);
        float hue3 = MyMath.Wrap01(hue2 + baseHueDifference);
        hue1 += Random.Range(-maxVariation, maxVariation);
        hue2 += Random.Range(-maxVariation, maxVariation);
        hue3 += Random.Range(-maxVariation, maxVariation);
        return new Color[3] {
            Color.HSVToRGB(hue1, 1f, 1f),
            Color.HSVToRGB(hue2, 1f, 1f),
            Color.HSVToRGB(hue3, 1f, 1f)
        };
    }

    public void LoadVulnerablePalette() {
        ChangePalette(playerVulnerablePalette, 0.1f);
    }

    public void LoadFallingSequencePalette() {
        ChangePalette(fallingSequencePalette, 0.1f);
    }

    public void RestoreSavedPalette() {
        ChangePalette(savedPalette, 0.78f);
    }

    public void PlayerWasHurtHandler(GameEvent gameEvent) {
        LoadVulnerablePalette();
    }

    public void LevelCompletedHandler(GameEvent gameEvent) {
        ChangeToRandomPalette(0.1f);
    }
}
