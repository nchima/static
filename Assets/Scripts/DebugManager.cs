using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugManager : MonoBehaviour {

    public enum AudioState { Normal, Silent, SFXOnly }
    AudioState _audioState = AudioState.Normal;
    public AudioState audioState
    {
        get
        {
            return _audioState;
        }

        set
        {
            if (value == AudioState.Normal)
            {
                AudioListener.volume = originalVolume;
                GetComponentInChildren<MusicManager>().dontPlayMusic = false;
            }

            else if (value == AudioState.Silent)
            {
                AudioListener.volume = 0f;
            }

            else if (value == AudioState.SFXOnly)
            {
                AudioListener.volume = originalVolume;
                GetComponentInChildren<MusicManager>().dontPlayMusic = true;
            }

            _audioState = value;
        }
    }
    [SerializeField] bool beginSilent = false;
    float originalVolume;

    public enum VisualState { Normal, DebugCam, OverheadDebugCam }
    VisualState _visualState = VisualState.Normal;
    public VisualState visualState
    {
        get
        {
            return _visualState;
        }

        set
        {
            if (value == VisualState.DebugCam)
            {
                SetActiveNormalModeObjects(false);
                SetActiveDebugModeObjects(true);
                overheadCamera.gameObject.SetActive(false);
            }

            else if (value == VisualState.Normal)
            {
                Debug.Log("setting my penis on fire.");
                SetActiveNormalModeObjects(true);
                SetActiveDebugModeObjects(false);
                overheadCamera.gameObject.SetActive(false);
            }


            else if (value == VisualState.OverheadDebugCam)
            {
                SetActiveNormalModeObjects(false);
                SetActiveDebugModeObjects(true);
                overheadCamera.gameObject.SetActive(true);
            }


            _visualState = value;
        }
    }

    [SerializeField] GameObject[] debugModeObjects;
    [SerializeField] GameObject[] normalModeObjects;

    [SerializeField] Camera overheadCamera;


    private void Start()
    {
        originalVolume = AudioListener.volume;
        if (beginSilent) audioState = AudioState.Silent;
    }


    private void Update()
    {
        // Handle music switching.
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (audioState == AudioState.Normal) audioState = AudioState.SFXOnly;
            else if (audioState == AudioState.SFXOnly) audioState = AudioState.Silent;
            else if (audioState == AudioState.Silent) audioState = AudioState.Normal;
        }

        // Handle visuals switching.
        if (Input.GetKeyDown(KeyCode.N))
        {
            if (visualState == VisualState.Normal) visualState = VisualState.DebugCam;
            else if (visualState == VisualState.DebugCam) visualState = VisualState.OverheadDebugCam;
            else if (visualState == VisualState.OverheadDebugCam) visualState = VisualState.Normal;
        }
    }


    void SetActiveNormalModeObjects(bool trueOrFalse)
    {
        for (int i = 0; i < normalModeObjects.Length; i++)
        {
            normalModeObjects[i].SetActive(trueOrFalse);
        }
    }


    void SetActiveDebugModeObjects(bool trueOrFalse)
    {
        for (int i = 0; i < debugModeObjects.Length; i++)
        {
            debugModeObjects[i].SetActive(trueOrFalse);
        }
    }
}
