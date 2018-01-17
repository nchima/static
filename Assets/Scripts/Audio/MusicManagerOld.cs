using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MusicManagerOld : MonoBehaviour {

    public bool dontPlayMusic = false;

	[SerializeField] private AudioClip[] tracks;    // An array holding references to the music tracks I will choose from.
    Stack<AudioClip> trackStack;
    private int previousTrackIndex;  // The index of the track that I played last (to make sure the same track doesn't play twice.)

    AudioSource audioSource;

	void Start () {
		audioSource = GetComponent<AudioSource> ();

        // Put all the clips into the Stack.
        trackStack = new Stack<AudioClip>();
        foreach (AudioClip clip in tracks) {
            trackStack.Push(clip);
        }

		int clipIndex = Random.Range(0, tracks.Length);
        previousTrackIndex = clipIndex;
		audioSource.clip = tracks [clipIndex];
	}


    void LoadTracks()
    {

    }


    void Update() {
        if (dontPlayMusic) {
            audioSource.Stop();
            return;
        }

        if (!audioSource.isPlaying) {
            ChooseNewClip();
        }

        // Set music based on current sine value.
        //GetComponent<AudioSource>().outputAudioMixerGroup.audioMixer.SetFloat("FilterCutoff", (GunValueManager.currentValue + 1f) * 11000f + 200f);
        GetComponent<AudioSource>().outputAudioMixerGroup.audioMixer.SetFloat("FilterCutoff", MyMath.Map(GunValueManager.currentValue, -1f, 1f, 3000f, 7000f));
    }


    void ChooseNewClip() {
        // Make sure the same track does not play twice in a row.
        int clipIndex = previousTrackIndex;
        while (tracks.Length > 1 && clipIndex == previousTrackIndex) {
            clipIndex = Random.Range(0, tracks.Length);
        }

        previousTrackIndex = clipIndex;
        audioSource.clip = tracks [clipIndex];
		audioSource.Play ();
	}
}
