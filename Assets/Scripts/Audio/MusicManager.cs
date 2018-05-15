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
    [SerializeField] AudioMixer newMusicMixer;
    [SerializeField] AudioSource musicAudioSource;

    [SerializeField] FloatRange rhythmVolumeRange = new FloatRange(-5f, 5f);
    [SerializeField] FloatRange ABVolumeRange = new FloatRange(-5f, 5f);

    int tracksPerLayer = 2;
    bool isInFallingSequence;

    MusicDebugState[] musicDebugStates;
    int currentDebugState = 0;

    [SerializeField] AudioClip[] newMusicSpeedClips;
    int currentSpeedClip = 0;


    private void Awake() {
        GameEventManager.instance.Subscribe<GameEvents.LevelCompleted>(LevelCompletedHandler);
    }


    private void Start() {
        musicDebugStates = new MusicDebugState[3];
        musicDebugStates[0] = new MusicDebugState(0f, 0f);
        musicDebugStates[1] = new MusicDebugState(1f, 0f);
        musicDebugStates[2] = new MusicDebugState(0f, 1f);
    }


    private void Update() {
        if (!isInFallingSequence) { SetLayerVolumeByGunValue(); }

        if (Input.GetKeyDown(KeyCode.M)) {
            int nextDebugState = currentDebugState;
            nextDebugState++;
            if (nextDebugState > musicDebugStates.Length - 1) { nextDebugState = 0; }
            ApplyDebugState(musicDebugStates[nextDebugState]);
            currentDebugState = nextDebugState;
        }

        if (Input.GetKeyDown(KeyCode.K)) {
            int nextSpeedClip = currentSpeedClip;
            nextSpeedClip++;
            if (nextSpeedClip > newMusicSpeedClips.Length - 1) { nextSpeedClip = 0; }
            transform.Find("New Music").GetComponent<AudioSource>().clip = newMusicSpeedClips[nextSpeedClip];
            transform.Find("New Music").GetComponent<AudioSource>().Play();
            currentSpeedClip = nextSpeedClip;
        }
    }


    public void EnterFallingSequence() {
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

        float lowpassCutoffFreq = MyMath.Map(GunValueManager.currentValue, -1f, 1f, 1000f, 9000f);
        masterMixer.SetFloat("Lowpass Cutoff Freq", lowpassCutoffFreq);

        //float highpassCutoffFreq = MyMath.Map(GunValueManager.currentValue, -1f, 1f, 10f, 11000f);
        //masterMixer.SetFloat("Highpass Cutoff Freq", highpassCutoffFreq);

        float pitch = MyMath.Map(GunValueManager.currentValue, -1f, 1f, 0.98f, 1.02f);
        musicAudioSource.pitch = pitch;
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


    void ApplyDebugState(MusicDebugState state) {
        transform.Find("Layer A 1").GetComponent<AudioSource>().volume = state.oldMusicVol;
        transform.Find("Layer A 2").GetComponent<AudioSource>().volume = state.oldMusicVol;
        transform.Find("Layer B 1").GetComponent<AudioSource>().volume = state.oldMusicVol;
        transform.Find("Layer B 2").GetComponent<AudioSource>().volume = state.oldMusicVol;
        transform.Find("Rhythm 1").GetComponent<AudioSource>().volume = state.oldMusicVol;
        transform.Find("Rhythm 2").GetComponent<AudioSource>().volume = state.oldMusicVol;
        transform.Find("New Music").GetComponent<AudioSource>().volume = state.newMusicVol;
    }


    public void LevelCompletedHandler(GameEvent gameEvent) {
        EnterFallingSequence();
    }


    class MusicDebugState {
        public float oldMusicVol;
        public float newMusicVol;

        public MusicDebugState(float oldMusicVol, float newMusicVol) {
            this.oldMusicVol = oldMusicVol;
            this.newMusicVol = newMusicVol;
        }
    }
}
