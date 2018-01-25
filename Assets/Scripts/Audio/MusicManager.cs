using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using DG.Tweening;

public class MusicManager : MonoBehaviour {

    [SerializeField] AudioMixer masterMixer;
    [SerializeField] AudioMixer musicMasterMixer;
    [SerializeField] AudioMixer layerAMixer;
    [SerializeField] AudioMixer layerBMixer;
    [SerializeField] AudioMixer rhythmMixer;

    [SerializeField] FloatRange rhythmVolumeRange = new FloatRange(-5f, 5f);
    [SerializeField] FloatRange ABVolumeRange = new FloatRange(-5f, 5f);

    int tracksPerLayer = 2;
    bool isInFallingSequence;


    private void Update() {
        if (!isInFallingSequence) { SetLayerVolumeByGunValue(); }
    }


    public void EnterFallingSequence() {
        Debug.Log("enter falling sequence");
        isInFallingSequence = true;
        masterMixer.SetFloat("Lowpass Cutoff Freq", 300f);
        masterMixer.SetFloat("Lowpass Resonance", 6f);
        masterMixer.SetFloat("Pitch", 0.5f);
        masterMixer.SetFloat("Master Volume", -5f);
        RandomizeAllMusicVolumeLevels();
    }


    public void EnterSpeedFall() {
        masterMixer.SetFloat("Pitch", 1f);
        masterMixer.SetFloat("Lowpass Cutoff Freq", 5000f);
        masterMixer.SetFloat("Lowpass Resonance", 3f);
        masterMixer.SetFloat("Master Volume", 0f);
        SetAllAudioSourcePitch(3f, 0.1f);
    }


    public void ExitFallingSequence() {
        masterMixer.SetFloat("Lowpass Cutoff Freq", 5000f);
        masterMixer.SetFloat("Lowpass Resonance", 3f);
        masterMixer.SetFloat("Pitch", 1f);
        isInFallingSequence = false;
    }


    void SetLayerVolumeByGunValue() {
        float layerABVolume = MyMath.Map(GunValueManager.currentValue, -1f, 1f, ABVolumeRange.min, ABVolumeRange.max);
        musicMasterMixer.SetFloat("Layer A Volume", layerABVolume);
        musicMasterMixer.SetFloat("Layer B Volume", layerABVolume);

        float rhythmVolume = MyMath.Map(GunValueManager.currentValue, -1f, 1f, rhythmVolumeRange.max, rhythmVolumeRange.min);
        musicMasterMixer.SetFloat("Rhythm Volume", rhythmVolume);

        float lowpassCutoffFreq = MyMath.Map(GunValueManager.currentValue, -1f, 1f, 3000f, 7000f);
        masterMixer.SetFloat("Lowpass Cutoff Freq", lowpassCutoffFreq);
    }


    // Selects one track from each layer to play at full volume and randomized the volume of all others.
    public void RandomizeAllMusicVolumeLevels() {
        RandomizeAllTrackVolumeLevels(layerAMixer);
        RandomizeAllTrackVolumeLevels(layerBMixer);
        RandomizeAllTrackVolumeLevels(rhythmMixer);
    }

    // Unused.
    void SelectRandomTrack(AudioMixer mixer) {
        int trackToSelect = Random.Range(1, tracksPerLayer + 1);
        for (int i = 1; i <= tracksPerLayer; i++) {
            if (i == trackToSelect) { mixer.SetFloat("Track " + trackToSelect + " Volume", 0f); }
            else { mixer.SetFloat("Track " + i + " Volume", -80f); }
        }
    }

    // Picks a random track from a layer and plays it at full volume. Plays the others and low volume.
    void RandomizeAllTrackVolumeLevels(AudioMixer mixer) {
        int loudTrack = Random.Range(1, tracksPerLayer + 1);
        for (int i = 1; i <= tracksPerLayer; i++) {
            if (i == loudTrack) { mixer.SetFloat("Track " + i + " Volume", 0f); }
            else { mixer.SetFloat("Track " + i + " Volume", Random.Range(-60f, -10f)); }
        }
    }

    public void ReturnMusicPitchToFullSpeed() {
        SetAllAudioSourcePitch(1f, 1f);
    }

    public void PitchDownMusicForSlowMotion() {
        SetAllAudioSourcePitch(0.1f, 0.1f);
    }

    void SetAllAudioSourcePitch(float pitch, float duration) {
        foreach (AudioSource audioSource in FindObjectsOfType<AudioSource>()) {
            audioSource.DOPitch(pitch, duration).SetUpdate(true);
        }
    }
}
