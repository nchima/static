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

    public float activeScale = 1.2f;
    public float inactiveScale = 0.9f;

    [SerializeField] float startPause = 1f;   // How long the UI waits before it accepts the first input.
    float startPauseTimer;
    [SerializeField] float keyCooldown = 0.14f;   // How often a keypress is registered.
    float sinceLastKeypress = 0f;

    float joystickDeadzone = 0.5f;

    Transform gameOverScreen;
    Transform nameEntry;


    void Start() {
        ActiveInitial.Active = true;
    }

	
	void Update ()
    {
        /* PLAYER CONTROL */
        sinceLastKeypress += Time.deltaTime;
        startPauseTimer += Time.deltaTime;
        if (startPauseTimer < startPause) { return; }

        // See if the player is using the keyboard to type their initials.
        if (InputManager.AnyControllerButtonPressed  && Input.inputString.Length > 0)
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
        if (sinceLastKeypress >= keyCooldown && (InputManager.AnyControllerButtonPressed || InputManager.MouseAndKeyboardUsed))
        {
            // If the player pressed up or down, change the character of the active initial.
            if (InputManager.movementAxis.y < -joystickDeadzone) {
                ActiveInitial.charIndex--;
            }

            else if (InputManager.movementAxis.y > joystickDeadzone) {
                ActiveInitial.charIndex++;
            }

            // If the player pressed a horizontal direction, switch active letter.
            else if ((InputManager.movementAxis.x < -joystickDeadzone || InputManager.cancelButtonDown) && !Input.GetKey(KeyCode.A))
            {
                ActiveInitial.Active = false;
                activeInitalIndex--;
                if (activeInitalIndex < 0) activeInitalIndex = initials.Length - 1;
                ActiveInitial.Active = true;
            }

            else if ((InputManager.movementAxis.x > joystickDeadzone || InputManager.fireButtonDown || InputManager.submitButtonDown) && !Input.GetKey(KeyCode.D))
            {
                ActiveInitial.Active = false;
                activeInitalIndex++;
                if (activeInitalIndex > initials.Length - 1) activeInitalIndex = 0;
                ActiveInitial.Active = true;
            }

            sinceLastKeypress = 0f;

            // If the player is finished they should press fire.
            if (AllInitialsEntered() && InputManager.pauseButtonDown || InputManager.submitButtonDown || InputManager.fireButtonDown)
            {
                // Go through each initial and add it to a string.
                string enteredInitials = "";
                bool cancel = false;
                foreach (Initial initial in initials) {
                    if (initial.charIndex != 0) {
                        enteredInitials += letters[initial.charIndex].ToString();
                    }

                    // If the player hasn't had time to enter their initials then go back.
                    else {
                        cancel = true;
                    }
                }

                // Tell the score controller to add this entry to its score list and then close this screen.
                if (!cancel) {
                    Services.scoreManager.InsertScore(enteredInitials);
                    Services.scoreManager.RetrieveScoresForHighScoreScreen();
                    Services.uiManager.ShowHighScoreScreen();
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
