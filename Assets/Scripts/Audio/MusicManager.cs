using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class MusicManager : MonoBehaviour {

    [SerializeField] AudioMixer musicMasterMixer;
    [SerializeField] AudioMixer layerAMixer;
    [SerializeField] AudioMixer layerBMixer;
    [SerializeField] AudioMixer rhythmMixer;

    int tracksPerLayer = 2;

    private void Update() {
        if (Input.GetKeyDown(KeyCode.J)) {
            RandomizeAllMusicVolumeLevels();
        }
    }

    public void ChooseAllTracksRandomly() {
        SelectRandomTrack(layerAMixer);
        SelectRandomTrack(layerBMixer);
        SelectRandomTrack(rhythmMixer);
    }

    void SelectRandomTrack(AudioMixer mixer) {
        int trackToSelect = Random.Range(1, tracksPerLayer + 1);
        for (int i = 1; i <= tracksPerLayer; i++) {
            if (i == trackToSelect) { mixer.SetFloat("Track " + trackToSelect + " Volume", 0f); }
            else { mixer.SetFloat("Track " + i + " Volume", -80f); }
        }
    }

    public void RandomizeAllMusicVolumeLevels() {
        RandomizeAllTrackVolumeLevels(layerAMixer);
        RandomizeAllTrackVolumeLevels(layerBMixer);
        RandomizeAllTrackVolumeLevels(rhythmMixer);
    }

    void RandomizeAllTrackVolumeLevels(AudioMixer mixer) {
        int loudTrack = Random.Range(1, tracksPerLayer + 1);
        for (int i = 1; i <= tracksPerLayer; i++) {
            if (i == loudTrack) { mixer.SetFloat("Track " + i + " Volume", 0f); }
            else { mixer.SetFloat("Track " + i + " Volume", Random.Range(-60f, -10f)); }
        }
    }

    public void ReturnMusicPitchToFullSpeed() {
        //musicManager..DOPitch(1f, 1f).SetUpdate(true);
    }

    public void PitchDownMusicForSlowMotion() {
        //GameManager.musicManager.GetComponent<AudioSource>().DOPitch(0.1f, 0.1f).SetUpdate(true);
    }
}
