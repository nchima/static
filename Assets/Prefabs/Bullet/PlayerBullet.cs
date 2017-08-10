using UnityEngine;
using System.Collections;

public class PlayerBullet : MonoBehaviour
{
    [SerializeField]
    float deleteTime = 0.25f;   // How long this bullet lasts on screen before being 'deleted'.
    private float timeOnScreen;    // How long this bullet has existed thus far.
    private bool isOnScreen;    // Whether this bullet is being fired.


    private void Start()
    {
        GetComponent<MeshRenderer>().material.mainTexture = GameManager.instance.noiseGenerator.noiseTex;
    }


    private void Update()
    {
        // If this bullet is currently onscreen, track how long it has been on screen for.
        if (isOnScreen)
        {
            timeOnScreen += Time.deltaTime;
        }

        // If this bullet has existed long enough to be deleted then delete it.
        if (timeOnScreen >= deleteTime)
        {
            // Move this bullet back to its holding location & make it tiny.
            transform.position = new Vector3(0, -500, 0);
            transform.localScale = new Vector3(1, 1, 1);

            isOnScreen = false;
        }
    }


    /// <summary>
    /// Moves the bullet to the given position, rotation and scale, then sets it as active.
    /// </summary>
    /// <param name="_position">The position of the fired bullet.</param>
    /// <param name="_rotation">The rotation of the fired bullet.</param>
    /// <param name="_scale">The scale of the fired bullet</param>
    public void GetFired(Vector3 _position, Quaternion _rotation, Vector3 _scale)
    {
        // Get proper transform values.
        transform.position = _position;
        transform.rotation = _rotation;
        transform.localScale = _scale;

        // Set this bullet to active.
        isOnScreen = true;

        // Reset this bullet's timer.
        timeOnScreen = 0f;
    }


    /*
     * 
     * I've got a problem with illusions that never love back
     * I've gotta admit i feel conspicuous when I fall on my ass
     * 
     * and now all my eyes have this secret layer
     * and I feel I'm vacuumed by the moon when I don't mean to stare
     * but can you feel my hands burning from across the room
     * and can you tell somewhere on my face that I'm ready for you
     * 
     * cause I'm ready I feel so ready right now
     * I feel so ready for love
     * and I want to be seen as if from above
     * 
     * I don't wanna feel contagious I just wanna be touched
     * I don't want to have to get all religious but how else can I run
     * 
     * and now all my hands have this perfect flaw
     * where they can never catch nothing but they are never wrong
     * but I have lived my life in a grain of salt
     * now feels so free in that it's no one's fault
     * 
     * because i'm ready
     * i feel so ready right now
     * i feel so ready for love
     * i wanna learn of your spark
     * i wanna feel it in my lungs
     * 
     * my vomit is all haunted with a spectral kiss
     * and I had a dream when I was a child that I would see days like this
     * 
     * now all my songs are distorting pain
     * some cruel cruel flame that runs through my days
     * but I'll grab that heart I'll wear it like glove
     * because there are good women in your life and you don't have to run
     * 
     * now I'm ready 
     * I feel so ready for love
     * and I want to be found
     * I don't feel I should have to hunt
     * 
     */
}