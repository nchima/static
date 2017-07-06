using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MusicManager : MonoBehaviour {

	[SerializeField] private AudioClip[] tracks;    // An array holding references to the music tracks I will choose from.
    Stack<AudioClip> trackStack;
    private int previousTrackIndex;  // The index of the track that I played last (to make sure the same track doesn't play twice.)

    AudioSource audioSource;

	void Start ()
    {
		audioSource = GetComponent<AudioSource> ();

        // Put all the clips into the Stack.
        trackStack = new Stack<AudioClip>();
        foreach (AudioClip clip in tracks)
        {
            trackStack.Push(clip);
        }

		int clipIndex = Random.Range(0, tracks.Length);
        previousTrackIndex = clipIndex;
		audioSource.clip = tracks [clipIndex];
	}


    void LoadTracks()
    {

    }


    void Update()
    {
        if (!audioSource.isPlaying)
        {
            ChooseNewClip();
        }
    }


    void ChooseNewClip()
    {
        // Make sure the same track does not play twice in a row.
        int clipIndex = previousTrackIndex;
        while (clipIndex == previousTrackIndex)
        {
            clipIndex = Random.Range(0, tracks.Length);
        }

        previousTrackIndex = clipIndex;
        audioSource.clip = tracks [clipIndex];
		audioSource.Play ();
	}
	

}
