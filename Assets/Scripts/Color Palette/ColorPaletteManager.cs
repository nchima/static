﻿using System.Collections;
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

    [SerializeField] ColorPalette originalPalette;
    [SerializeField] ColorPalette tempSavedPalette;
    [SerializeField] ColorPalette playerVulnerablePalette;
    [SerializeField] ColorPalette fallingSequencePalette;
 
    private void Awake() {
        // Memorize original colors.
        SaveCurrentPalette(originalPalette);
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
    }

    public void LoadVulnerablePalette() {
        ChangePalette(playerVulnerablePalette, 0.1f);
    }

    public void LoadFallingSequencePalette() {
        ChangePalette(fallingSequencePalette, 0.1f);
    }

    public void RestoreSavedPalette() {
        ChangePalette(originalPalette, 0.78f);
    }

    private void OnDisable() {
        // Restore memorized original colors.
        ChangePaletteImmediate(originalPalette);
    }
}
