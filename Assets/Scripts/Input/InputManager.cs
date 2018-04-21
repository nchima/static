using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour {

    public enum InputMode { MouseAndKeyboard, Controller }
    [HideInInspector] public static InputMode inputMode = InputMode.MouseAndKeyboard;

    [HideInInspector] public static Vector2 movementAxis;
    [HideInInspector] public static float turningValue;

    [HideInInspector] public static float gunTuningValue;

    [HideInInspector] public static bool fireButton;
    [HideInInspector] public static bool fireButtonDown;
    [HideInInspector] public static bool fireButtonUp;

    [HideInInspector] public static bool specialMoveButton;
    [HideInInspector] public static bool specialMoveButtonDown;
    [HideInInspector] public static bool specialMoveButtonUp;

    [HideInInspector] public static bool pauseButton;
    [HideInInspector] public static bool pauseButtonDown;

    [HideInInspector] public static bool submitButton;
    [HideInInspector] public static bool submitButtonDown;

    [HideInInspector] public static bool cancelButton;
    [HideInInspector] public static bool cancelButtonDown;


    public static bool AnyControllerButtonPressed {
        get {

            if (Input.GetAxisRaw("Horizontal Controller") != 0) { return true; }
            if (Input.GetAxisRaw("Vertical Controller") != 0) { return true; }
            if (Input.GetAxisRaw("Controller Right Stick Horizontal") != 0) { return true; }
            if (Input.GetAxisRaw("Controller Right Stick Vertical") != 0) { return true; }
            if (Input.GetAxisRaw("Fire Controller") != 0) { return true; }
            if (Input.GetAxisRaw("Run Controller") != 0) { return true; }
            if (Input.GetButton("Special Move Controller")) { return true; }
            if (Input.GetButton("Start")) { return true; }

            return false;
        }
    }
    public static bool MouseAndKeyboardUsed {
        get {
            if (Input.anyKey) { return true; }
            if (Input.GetAxis("Mouse X") != 0) { return true; }
            if (Input.GetAxis("Mouse Y") != 0) { return true; }

            return false;
        }
    }
    public static bool AnyInputPressed {
        get {
            return AnyControllerButtonPressed || MouseAndKeyboardUsed;
        }
    }


    private void Update() {

        ResetTriggers();

        // Figure out which input mode to use.
        if (MouseAndKeyboardUsed) { inputMode = InputMode.MouseAndKeyboard; }
        if (AnyControllerButtonPressed) {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            inputMode = InputMode.Controller;
        }

        switch(inputMode) {
            case InputMode.MouseAndKeyboard:
                GetMouseAndKeyboardInput();
                break;
            case InputMode.Controller:
                GetControllerInput();
                break;
            default:
                break;
        }
    } 


    void ResetTriggers() {
        fireButtonDown = false;
        fireButtonUp = false;
        specialMoveButtonDown = false;
        specialMoveButtonUp = false;
        pauseButtonDown = false;
        submitButtonDown = false;
        cancelButtonDown = false;
    }


    void GetMouseAndKeyboardInput() {
        movementAxis.x = Input.GetAxisRaw("Horizontal Keyboard");
        movementAxis.y = Input.GetAxisRaw("Vertical Keyboard");

        turningValue = Input.GetAxis("Mouse X");

        gunTuningValue = Input.GetAxis("Mouse Y");

        fireButton = Input.GetButton("Fire Mouse and Keyboard");
        fireButtonDown = Input.GetButtonDown("Fire Mouse and Keyboard");
        fireButtonUp = Input.GetButtonUp("Fire Mouse and Keyboard");

        specialMoveButton = Input.GetButton("Special Move Mouse and Keyboard");
        specialMoveButtonDown = Input.GetButtonDown("Special Move Mouse and Keyboard");
        specialMoveButtonUp = Input.GetButtonUp("Special Move Mouse and Keyboard");

        pauseButton = Input.GetKey(KeyCode.Escape);
        pauseButtonDown = Input.GetKeyDown(KeyCode.Escape);

        submitButton = Input.GetButton("Submit");
        submitButtonDown = Input.GetButton("Submit");

        cancelButton = Input.GetKey(KeyCode.Delete);
        cancelButton = Input.GetKey(KeyCode.Backspace);
        cancelButtonDown = Input.GetKeyDown(KeyCode.Delete);
        cancelButtonDown = Input.GetKeyDown(KeyCode.Backspace);
    }


    void GetControllerInput() {
        movementAxis.x = Input.GetAxis("Horizontal Controller");
        movementAxis.y = Input.GetAxis("Vertical Controller");

        turningValue = Input.GetAxis("Controller Right Stick Horizontal");

        gunTuningValue = Input.GetAxis("Controller Right Stick Vertical");

        fireButton = Input.GetAxisRaw("Fire Controller") != 0;
        fireButtonDown = Input.GetAxisRaw("Fire Controller") == 1f;

        specialMoveButton = Input.GetButton("Special Move Controller");
        specialMoveButtonDown = Input.GetButtonDown("Special Move Controller");
        specialMoveButtonUp = Input.GetButtonUp("Special Move Controller");

        pauseButton = Input.GetButton("Start");
        pauseButtonDown = Input.GetButtonDown("Start");

        submitButton = Input.GetButton("Special Move Controller");
        submitButtonDown = Input.GetButtonDown("Special Move Controller");

        cancelButton = Input.GetButton("Cancel");
        cancelButtonDown = Input.GetButtonDown("Cancel");
    }
}