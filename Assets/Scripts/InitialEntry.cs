using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitialEntry : MonoBehaviour {

    public Initial[] initials;
    Initial ActiveInitial
    {
        get
        {
            return initials[activeInitalIndex];
        }
    }
    public static char[] letters = { '_', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' }; 
    int activeInitalIndex = 0;   // The initial which is currently being controlled by the player.

    public Color activeColor;   // The color of the letter when it is active.
    public Color inactiveColor;

    public float activeScale = 1.1f;
    public float inactiveScale = 1.0f;

    float keyCooldown = 0.09f;   // How often a keypress is registered.
    float sinceLastKeypress = 0f;

    float joystickDeadzone = 0.5f;

    ScoreManager scoreManager;
    Transform gameOverScreen;
    Transform nameEntry;


    void Start()
    {
        scoreManager = GameObject.Find("Game Manager").GetComponent<ScoreManager>();

        ActiveInitial.Active = true;
    }

	
	void Update ()
    {
        sinceLastKeypress += Time.deltaTime;

        /* PLAYER CONTROL */

        // See if the player is using the keyboard to type their initials.
        if (Input.anyKeyDown && Input.inputString.Length > 0)
        {
            foreach (char letter in letters)
            {
                if (char.ToUpper(Input.inputString[0]) == letter)
                {
                    sinceLastKeypress = 100f;
                    ActiveInitial.SetChar(letter);
                    ActiveInitial.Active = false;
                    activeInitalIndex++;
                    if (activeInitalIndex > initials.Length - 1) activeInitalIndex = initials.Length - 1;
                    ActiveInitial.Active = true;
                }
            }
        }

        // Check to make sure it has not been too soon since the player last pressed a key.
        if (sinceLastKeypress >= keyCooldown && (Input.anyKey || Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0 || Input.GetAxis("Fire1") > joystickDeadzone))
        {
            // If the player pressed up or down, change the character of the active initial.
            if (Input.GetAxisRaw("Vertical") < -joystickDeadzone && !Input.GetKey(KeyCode.S))
            {
                ActiveInitial.charIndex--;
            }

            else if (Input.GetAxisRaw("Vertical") > joystickDeadzone && !Input.GetKey(KeyCode.W))
            {
                ActiveInitial.charIndex++;
            }

            // If the player pressed a vertical direction, switch active letter.
            else if ((Input.GetAxisRaw("Horizontal") < -joystickDeadzone || Input.GetKey(KeyCode.Backspace) || Input.GetKey(KeyCode.Delete) || Input.GetButtonDown("Cancel")) && !Input.GetKey(KeyCode.A))
            {
                ActiveInitial.Active = false;
                activeInitalIndex--;
                if (activeInitalIndex < 0) activeInitalIndex = initials.Length - 1;
                ActiveInitial.Active = true;
            }

            else if ((Input.GetAxisRaw("Horizontal") > joystickDeadzone || Input.GetButtonDown("Fire1") || Input.GetAxisRaw("Fire1") > joystickDeadzone || Input.GetKey(KeyCode.KeypadEnter) || Input.GetButtonDown("Submit")) && !Input.GetKey(KeyCode.D))
            {
                ActiveInitial.Active = false;
                activeInitalIndex++;
                if (activeInitalIndex > initials.Length - 1) activeInitalIndex = 0;
                ActiveInitial.Active = true;
            }

            sinceLastKeypress = 0f;

            // If the player is finished they should press fire.
            //if (AllInitialsEntered() && (Input.GetButtonDown("Fire1") || Input.GetKey(KeyCode.Return) || Input.GetAxisRaw("Fire1") > joystickDeadzone))
            if (AllInitialsEntered() && Input.GetButtonDown("Start") || Input.GetKey(KeyCode.Return))
            {
                // Go through each initial and add it to a string.
                string enteredInitials = "";
                bool cancel = false;
                foreach (Initial initial in initials)
                {
                    if (initial.charIndex != 0)
                    {
                        enteredInitials += letters[initial.charIndex].ToString();
                    }

                    // If the player hasn't had time to enter their initials then go back.
                    else
                    {
                        cancel = true;
                    }
                }

                // Tell the score controller to add this entry to its score list and then close this screen.
                if (!cancel)
                {
                    scoreManager.InsertScore(enteredInitials);
                    GameObject.Find("Game Manager").GetComponent<GameManager>().ShowHighScores();
                }
            }
        }
    }


    bool AllInitialsEntered()
    {
        bool returnValue = true;

        foreach (Initial initial in initials)
        {
            if (initial.charIndex == 0)
            {
                returnValue = false;
            }
        }

        return returnValue;
    }
}
