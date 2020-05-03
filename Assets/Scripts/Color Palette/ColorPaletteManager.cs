using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ColorPaletteManager : MonoBehaviour {

    [SerializeField] bool useRandomPalettes;

    // Environment materials.
    [SerializeField] Material wallMaterial;
    [SerializeField] Material floorMaterial;
    [SerializeField] Material[] obstacleMaterials;

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

    //ColorPalette[] levelPalettes;
    //int levelPaletteIndex = 0;

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

    private void Awake() {
        // Memorize original colors.
        ChangePaletteImmediate(defaultPalette);

        //levelPalettes = new ColorPalette[Resources.LoadAll<ColorPalette>("Level Color Palettes").Length];
        //for (int i = 1; i < levelPalettes.Length; i++) {
        //    string levelNumber = (i + 1).ToString();
        //    levelPalettes[i] = Resources.Load<ColorPalette>("Level Color Palettes/Color Palette Level " + levelNumber);
        //}
    }

    private void OnEnable() {
        GameEventManager.instance.Subscribe<GameEvents.PlayerWasHurt>(PlayerWasHurtHandler);
    }

    private void OnDisable() {
        GameEventManager.instance.Unsubscribe<GameEvents.PlayerWasHurt>(PlayerWasHurtHandler);
    }

    private void Start() {
        SaveCurrentPalette(savedPalette);
    }

    private void Update() {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Alpha5)) {
            SaveCurrentPaletteAsNewAsset();
        }
# endif
    }

    void ChangePalette(ColorPalette newPalette, float duration) {
        wallMaterial.DOColor(newPalette.wallColor, duration);
        floorMaterial.DOColor(newPalette.floorColor, duration);
        foreach(Material obstacleMaterial in obstacleMaterials) { obstacleMaterial.DOColor(newPalette.obstacleColor, duration); }
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
        foreach(Material obstacleMaterial in obstacleMaterials) { obstacleMaterial.color = newPalette.obstacleColor; }
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
        saveTarget.obstacleColor = obstacleMaterials[0].color;
        saveTarget.perspectiveColor1 = perspectiveScreen1.GetComponent<MeshRenderer>().material.color;
        saveTarget.perspectiveColor2 = perspectiveScreen2.GetComponent<MeshRenderer>().material.color;
        saveTarget.perspectiveColor3 = perspectiveScreen3.GetComponent<MeshRenderer>().material.color;
        saveTarget.orthoColor1 = orthoScreen1.GetComponent<MeshRenderer>().material.GetColor("_Color");
        saveTarget.orthoColor2 = orthoScreen2.GetComponent<MeshRenderer>().material.GetColor("_Color");
        saveTarget.orthoColor3 = orthoScreen3.GetComponent<MeshRenderer>().material.GetColor("_Color");
        saveTarget.backgroundColor = backgroundScreen.GetComponent<MeshRenderer>().material.color;
    }

    Coroutine changeToRandomPaletteCoroutine;
    public void ChangeToRandomPalette(float duration) {
        if (changeToRandomPaletteCoroutine != null) { StopCoroutine(changeToRandomPaletteCoroutine); }
        changeToRandomPaletteCoroutine = StartCoroutine(ChangeToRandomPaletteCoroutine(duration));
    }

    IEnumerator ChangeToRandomPaletteCoroutine(float duration) {
        Color[] environmentPalette = ThreeColorPalette(0.33f, 0.2f);
        wallMaterial.DOColor(environmentPalette[0], duration);
        floorMaterial.DOColor(environmentPalette[1], duration);
        foreach(Material obstacleMaterial in obstacleMaterials) { obstacleMaterial.DOColor(environmentPalette[2], duration); }

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

    public void LoadVulnerablePalette() {
        ChangePalette(playerVulnerablePalette, 0.1f);
    }

    public void LoadFallingSequencePalette() {
        ChangePalette(fallingSequencePalette, 0.1f);
    }

    public void LoadLevelPalette() {
        float duration = 0.78f;

        if (Services.levelManager.currentLevelSet.colorPalette == null) {
            Debug.LogWarning("Current level set has not been assigned a color palette. Loading default.");
            ChangePalette(defaultPalette, duration);
            return;
        }

        ChangePalette(Services.levelManager.currentLevelSet.colorPalette, duration);
    }

    void SaveCurrentPaletteAsNewAsset() {
        ColorPalette paletteToSave = new ColorPalette();
        SaveCurrentPalette(paletteToSave);

#if UNITY_EDITOR
        AssetDatabase.CreateAsset(paletteToSave, "Assets/Resources/Level Color Palettes/Color Palette Level X.asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = paletteToSave;
#endif
    }

    public void PlayerWasHurtHandler(GameEvent gameEvent) {
        if (Services.healthManager.isInvincible) { return; }

        LoadVulnerablePalette();

        Invoke("LoadLevelPalette", 1.5f);
    }
}