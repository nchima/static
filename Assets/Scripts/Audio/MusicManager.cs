using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using DG.Tweening;
using System.IO;

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

    [SerializeField] FloatRange masterVolumeRange;
    [SerializeField] FloatRange musicMasterVolumeRange;

    [HideInInspector] public float masterVolumeMax = 1;

    [HideInInspector] private float masterVolume = 1;
    [HideInInspector] private float masterVolumeSliderValue = 1;
    [HideInInspector] private float musicVolume = 1;
    [HideInInspector] private float musicVolumeSliderValue = 1;

    public enum State { MainMenu, Normal, SpeedFall, FallingSequence, DashCharging, Dashing, GettingTased, Nothing }
    public State state = State.MainMenu;

    int tracksPerLayer = 2;

    MusicDebugState[] musicDebugStates;
    int currentDebugState = 0;

    [SerializeField] AudioClip[] newMusicSpeedClips;
    int currentSpeedClip = 0;

    private void OnEnable() {
        GameEventManager.instance.Subscribe<GameEvents.GameStarted>(GameStartedHandler);
        GameEventManager.instance.Subscribe<GameEvents.LevelLoaded>(LevelLoadedHandler);
    }

    private void OnDisable() {
        GameEventManager.instance.Unsubscribe<GameEvents.GameStarted>(GameStartedHandler);
        GameEventManager.instance.Unsubscribe<GameEvents.LevelLoaded>(LevelLoadedHandler);
    }

    private void Start() {
        musicDebugStates = new MusicDebugState[3];
        musicDebugStates[0] = new MusicDebugState(0f, 0f);
        musicDebugStates[1] = new MusicDebugState(1f, 0f);
        musicDebugStates[2] = new MusicDebugState(0f, 1f);

        SetMasterVolume(masterVolume);
        SetMusicVolume(musicVolume);
    }

    private void Update() {
        switch (state) {

            case State.MainMenu:
                AdjustMusicForGunValueMainMenu();

                // If the menu music has finished playing, loop all audio sources
                if (!GetComponentInChildren<AudioSource>().isPlaying) {
                    foreach(AudioSource audioSource in GetComponentsInChildren<AudioSource>()) {
                        if (audioSource.playOnAwake) { audioSource.Play(); }
                    }
                }
                break;

            case State.Normal:
                AdjustMusicForGunValueGameplay();
                break;

            case State.GettingTased:
                SetMusicVolume(0.2f);
                break;

            case State.FallingSequence:
                if (Services.playerController.fallSpeedControlType == PlayerController.FallSpeedControlType.Mouse) {
                    float newPitch = GunValueManager.MapTo(0.5f, 3f);
                    SetAllAudioSourcePitch(newPitch, 0f);
                }
                break;

            case State.SpeedFall:
                break;

            case State.Nothing:
                break;
        }

#if UNITY_EDITOR
        // The following code is for debugging
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
#endif

        if (Input.GetKeyDown(KeyCode.P)) {
            StartCoroutine(LoadTestMusicCo());
        }
    }

    private WWW www;
    IEnumerator LoadTestMusicCo() {
        www = new WWW("file://" + Path.Combine(Application.streamingAssetsPath, "testMusic.wav"));
        yield return new WaitUntil(()=>{ return www.isDone; });
        transform.Find("New Music").GetComponent<AudioSource>().clip = www.GetAudioClip();
        transform.Find("New Music").GetComponent<AudioSource>().Play();
        yield return null;
    }

    public void EnterFallingSequence() {
        if (state == State.FallingSequence) { return; }

        masterMixer.SetFloat("Lowpass Cutoff Freq", 300f);
        masterMixer.SetFloat("Lowpass Resonance", 6f);
        // masterMixer.SetFloat("Pitch", 0.5f);
        SetAllAudioSourcePitch(0.5f, 0.2f);
        musicMasterMixer.SetFloat("Master Volume", -5f);
        RandomizeAllMusicVolumeLevels();
        state = State.FallingSequence;
    }

    public void EnterSpeedFall() {
        if (state == State.SpeedFall) { return; }

        masterMixer.SetFloat("Pitch", 1f);
        masterMixer.SetFloat("Lowpass Cutoff Freq", 5000f);
        masterMixer.SetFloat("Lowpass Resonance", 3f);
        musicMasterMixer.SetFloat("Master Volume", 0f);
        SetAllAudioSourcePitch(3f, 0.1f);
        state = State.SpeedFall;
    }

    public void ExitFallingSequence() {
        masterMixer.SetFloat("Lowpass Cutoff Freq", 5000f);
        masterMixer.SetFloat("Lowpass Resonance", 3f);
        masterMixer.SetFloat("Pitch", 1f);
        SetAllAudioSourcePitch(1f, 0.5f);
        state = State.Normal;
    }

    public void EnterDashCharge() {
        masterMixer.SetFloat("Lowpass Cutoff Freq", 23f);
        masterMixer.SetFloat("Lowpass Resonance", 10f);
        masterMixer.SetFloat("Highpass Cutoff Freq", 2000f);
        masterMixer.SetFloat("Highpass Resonance", 10f);
        //SetAllAudioSourcePitch(0.5f, 0.6f);
        state = State.DashCharging;
    }

    public void BeginDash() {
        masterMixer.SetFloat("Pitch", 1f);
        masterMixer.SetFloat("Lowpass Cutoff Freq", 5000f);
        masterMixer.SetFloat("Lowpass Resonance", 3f);
        musicMasterMixer.SetFloat("Master Volume", 0f);
        SetAllAudioSourcePitch(3f, 0.1f);
        state = State.Dashing;
    }

    public void ExitDash() {
        masterMixer.SetFloat("Lowpass Cutoff Freq", 5000f);
        masterMixer.SetFloat("Lowpass Resonance", 3f);
        masterMixer.SetFloat("Highpass Cutoff Freq", 4685f);
        masterMixer.SetFloat("Highpass Resonance", 1f);
        masterMixer.SetFloat("Pitch", 0.1f);
        state = State.Normal;
    }

    void AdjustMusicForGunValueMainMenu() {
        float layerABVolume = MyMath.Map(GunValueManager.currentValue, -1f, 1f, ABVolumeRange.min, ABVolumeRange.max);
        musicMasterMixer.SetFloat("Layer A Volume", layerABVolume);
        musicMasterMixer.SetFloat("Layer B Volume", layerABVolume);

        float rhythmVolume = MyMath.Map(GunValueManager.currentValue, -1f, 1f, rhythmVolumeRange.max, rhythmVolumeRange.min);
        musicMasterMixer.SetFloat("Rhythm Volume", rhythmVolume);

        float lowpassCutoffFreq = MyMath.Map(GunValueManager.currentValue, -1f, 1f, 1000f, 9000f);
        masterMixer.SetFloat("Lowpass Cutoff Freq", lowpassCutoffFreq);

        float highpassCutoffFreq = MyMath.Map(GunValueManager.currentValue, -1f, 1f, 10f, 11000f);
        masterMixer.SetFloat("Highpass Cutoff Freq", highpassCutoffFreq);

        if (Services.fallingSequenceManager.isSpeedFallActive) { return; }
        float pitch = MyMath.Map(GunValueManager.currentValue, -1f, 1f, 0.98f, 1.02f);
        musicAudioSource.pitch = pitch;
    }


    void AdjustMusicForGunValueGameplay() {
        float lowpassCutoffFreq = MyMath.Map(GunValueManager.currentValue, -1f, 1f, 1000f, 9000f);
        masterMixer.SetFloat("Lowpass Cutoff Freq", lowpassCutoffFreq);

        if (Services.fallingSequenceManager.isSpeedFallActive) { return; }
        float pitch = MyMath.Map(GunValueManager.currentValue, -1f, 1f, 0.98f, 1.02f);
        musicAudioSource.pitch = pitch;
    }

    // Selects one track from each layer to play at full volume and randomized the volume of all others.
    public void RandomizeAllMusicVolumeLevels() {
        // Right now I don't want this to do anything because it's just fucking up the main menu music but you know, later if I do end up separating the music into multiple tracks by instrument then I might want to you know do um yeah!
        return;
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

    public void ReturnMusicPitchToFullSpeed(float duration) {
        SetAllAudioSourcePitch(1f, duration);
    }

    public void PitchDownMusicForSlowMotion(float duration) {
        state = State.Nothing;
        SetAllAudioSourcePitch(0.1f, duration * 0.4f);
    }

    void SetAllAudioSourcePitch(float pitch, float duration) {
        foreach (AudioSource audioSource in FindObjectsOfType<AudioSource>()) {
            if (duration == 0) {
                audioSource.pitch = pitch;
            }
            else {
                audioSource.DOPitch(pitch, duration).SetUpdate(true);
            }
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

    public void GameStartedHandler(GameEvent gameEvent) {
        musicMasterMixer.SetFloat("Layer A Volume", -80f);
        musicMasterMixer.SetFloat("Layer B Volume", -80f);
        musicMasterMixer.SetFloat("Rhythm Volume", -80f);
        musicAudioSource.Play();
        EnterFallingSequence();
    }

    public void LevelLoadedHandler(GameEvent gameEvent) {
        EnterFallingSequence();
    }

    public void SetMasterVolumeSliderValue(float value) {
        masterVolumeSliderValue = Mathf.Clamp01(value);
        SetMasterVolume(masterVolume);
    }

    public void SetMasterVolume(float value) {
        masterVolume = Mathf.Clamp(value, 0f, 1f);
        masterMixer.SetFloat("Master Volume", masterVolumeRange.MapTo(masterVolume * masterVolumeSliderValue, 0f, 1f));
    }

    public void SetMusicVolumeSliderValue(float value) {
        musicVolumeSliderValue = Mathf.Clamp01(value);
        SetMusicVolume(musicVolume);
    }

    public void SetMusicVolume(float value) {
        musicVolume = Mathf.Clamp01(value);
        musicMasterMixer.SetFloat("Master Volume", musicMasterVolumeRange.MapTo(musicVolume * musicVolumeSliderValue, 0f, 1f));
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
