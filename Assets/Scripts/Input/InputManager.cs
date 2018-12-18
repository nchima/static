using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputManager : MonoBehaviour {

    public enum InputMode { MouseAndKeyboard, Controller }
    [HideInInspector] public static InputMode inputMode = InputMode.MouseAndKeyboard;

    [HideInInspector] public static Vector2 movementAxis;
    [HideInInspector] public static float turningValue;

    [HideInInspector] public static float mouseSensitivityOverall = 1.5f;
    [HideInInspector] public static float mouseSensitivityXMod = 1.5f;
    [HideInInspector] public static float mouseSensitivityYMod = 1.5f;

    [HideInInspector] public static float controllerSensitivityOverall = 3.5f;
    [HideInInspector] public static float controllerSensitivityXMod = 1f;
    [HideInInspector] public static float controllerSensitivityYMod = 1f;

    [HideInInspector] public static float gunTuningValue;

    [HideInInspector] public static bool fireButton;
    [HideInInspector] public static bool fireButtonDown;
    [HideInInspector] public static bool fireButtonUp;

    [HideInInspector] public static bool specialMoveButton;
    [HideInInspector] public static bool specialMoveButtonDown;
    [HideInInspector] public static bool specialMoveButtonUp;

    [HideInInspector] public static bool dashButton;
    [HideInInspector] public static bool dashButtonDown;
    [HideInInspector] public static bool dashButtonUp;

    [HideInInspector] public static bool pauseButton;
    [HideInInspector] public static bool pauseButtonDown;

    [HideInInspector] public static bool submitButton;
    [HideInInspector] public static bool submitButtonDown;

    [HideInInspector] public static bool cancelButton;
    [HideInInspector] public static bool cancelButtonDown;

    public static bool AnyControllerButtonPressed {
        get {

            if (Input.GetAxisRaw("Horizontal Controller Gameplay") != 0) { return true; }
            if (Input.GetAxisRaw("Vertical Controller Gameplay") != 0) { return true; }
            if (Input.GetAxisRaw("Horizontal Controller") != 0) { return true; }
            if (Input.GetAxisRaw("Vertical Controller") != 0) { return true; }
            if (Input.GetAxisRaw("Controller Right Stick Horizontal") != 0) { return true; }
            if (Input.GetAxisRaw("Controller Right Stick Vertical") != 0) { return true; }
            if (Input.GetAxisRaw("Fire Controller") != 0) { return true; }
            if (Input.GetAxisRaw("Dash Controller") != 0) { return true; }
            if (Input.GetButton("Special Move Controller")) { return true; }
            if (Input.GetButton("Submit Controller")) { return true; }
            if (Input.GetButton("Cancel Controller")) { return true; }
            if (Input.GetButton("Pause Controller")) { return true; }

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

    const float UI_INPUT_COOLDOWN = 0.15f;
    float uiInputTimer = UI_INPUT_COOLDOWN;
    const float UI_DEAD_ZONE = 0.5f;

    void Awake() {
        RetrieveSavedSettings();
        inputMode = InputMode.MouseAndKeyboard;
    }

    private void Update() {

        ResetTriggers();

        // Figure out which input mode to use.
        if (inputMode != InputMode.MouseAndKeyboard && MouseAndKeyboardUsed) {
            inputMode = InputMode.MouseAndKeyboard;
            if (!Services.gameManager.isGameStarted || GameManager.isGamePaused) {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                //EventSystem.current.SetSelectedGameObject(null);
            }
            Services.uiManager.SwitchControlPrompts(InputMode.MouseAndKeyboard);
        }

        if (inputMode != InputMode.Controller && AnyControllerButtonPressed) {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            inputMode = InputMode.Controller;
            Services.uiManager.SwitchControlPrompts(InputMode.Controller);
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

        // Handle UI input
        uiInputTimer += Time.unscaledDeltaTime;
        if (movementAxis.magnitude < UI_DEAD_ZONE) { uiInputTimer = UI_INPUT_COOLDOWN; }

        if (uiInputTimer >= UI_INPUT_COOLDOWN) {
            AxisEventData ad = new AxisEventData(EventSystem.current);
            ad.moveDir = MoveDirection.None;
            if (movementAxis.x < -UI_DEAD_ZONE) { ad.moveDir = MoveDirection.Left; }
            else if (movementAxis.x > UI_DEAD_ZONE) { ad.moveDir = MoveDirection.Right; }
            else if (movementAxis.y < -UI_DEAD_ZONE) { ad.moveDir = MoveDirection.Down; }
            else if (movementAxis.y > UI_DEAD_ZONE) { ad.moveDir = MoveDirection.Up; }

            if (ad.moveDir != MoveDirection.None) {
                //ExecuteEvents.Execute(EventSystem.current.currentSelectedGameObject, ad, ExecuteEvents.deselectHandler);
                ExecuteEvents.Execute(EventSystem.current.currentSelectedGameObject, ad, ExecuteEvents.moveHandler);
                uiInputTimer = 0f;
            }

            if (submitButtonDown) {
                ExecuteEvents.Execute(EventSystem.current.currentSelectedGameObject, ad, ExecuteEvents.submitHandler);
            }

            if (cancelButtonDown) {
                ExecuteEvents.Execute(EventSystem.current.currentSelectedGameObject, ad, ExecuteEvents.cancelHandler);
            }
        }
    } 

    void ResetTriggers() {
        fireButtonDown = false;
        fireButtonUp = false;
        specialMoveButtonDown = false;
        specialMoveButtonUp = false;
        dashButtonDown = false;
        dashButtonUp = false;
        pauseButtonDown = false;
        submitButtonDown = false;
        cancelButtonDown = false;
    }

    void GetMouseAndKeyboardInput() {
        movementAxis.x = Input.GetAxisRaw("Horizontal Keyboard");
        movementAxis.y = Input.GetAxisRaw("Vertical Keyboard");

        turningValue = Input.GetAxis("Mouse X") * mouseSensitivityOverall * mouseSensitivityXMod;
        gunTuningValue = Input.GetAxis("Mouse Y") * mouseSensitivityOverall * mouseSensitivityYMod;

        fireButton = Input.GetButton("Fire Mouse and Keyboard");
        fireButtonDown = Input.GetButtonDown("Fire Mouse and Keyboard");
        fireButtonUp = Input.GetButtonUp("Fire Mouse and Keyboard");

        specialMoveButton = Input.GetButton("Special Move Mouse and Keyboard");
        specialMoveButtonDown = Input.GetButtonDown("Special Move Mouse and Keyboard");
        specialMoveButtonUp = Input.GetButtonUp("Special Move Mouse and Keyboard");

        dashButton = Input.GetButton("Dash Mouse and Keyboard");
        dashButtonDown = Input.GetButtonDown("Dash Mouse and Keyboard");
        dashButtonUp = Input.GetButtonUp("Dash Mouse and Keyboard");

        pauseButton = Input.GetButton("Pause Mouse and Keyboard");
        pauseButtonDown = Input.GetButtonDown("Pause Mouse and Keyboard");

        submitButton = Input.GetButton("Submit Mouse and Keyboard");
        submitButtonDown = Input.GetButtonDown("Submit Mouse and Keyboard");
        
        cancelButton = Input.GetButton("Cancel Mouse and Keyboard");
        cancelButtonDown = Input.GetButtonDown("Cancel Mouse and Keyboard");
    }

    void GetControllerInput() {
        movementAxis.x = Input.GetAxis("Horizontal Controller Gameplay");
        movementAxis.y = Input.GetAxis("Vertical Controller Gameplay");

        turningValue = Input.GetAxis("Controller Right Stick Horizontal") * controllerSensitivityOverall * controllerSensitivityXMod;
        gunTuningValue = Input.GetAxis("Controller Right Stick Vertical") * controllerSensitivityOverall * controllerSensitivityYMod;

        fireButton = Input.GetAxisRaw("Fire Controller") != 0;
        fireButtonDown = Input.GetAxisRaw("Fire Controller") == 1f;

        specialMoveButton = Input.GetButton("Special Move Controller");
        specialMoveButtonDown = Input.GetButtonDown("Special Move Controller");
        specialMoveButtonUp = Input.GetButtonUp("Special Move Controller");

        dashButton = Input.GetAxisRaw("Dash Controller") != 0;
        dashButtonDown = Input.GetAxisRaw("Dash Controller") != 0;

        pauseButton = Input.GetButton("Pause Controller");
        pauseButtonDown = Input.GetButtonDown("Pause Controller");

        submitButton = Input.GetButton("Submit Controller");
        submitButtonDown = Input.GetButtonDown("Submit Controller");

        cancelButton = Input.GetButton("Cancel Controller");
        cancelButtonDown = Input.GetButtonDown("Cancel Controller");
    }

    public static void SaveSettings() {
        PlayerPrefs.SetFloat("Overall Mouse Sensitivity", mouseSensitivityOverall);
        PlayerPrefs.SetFloat("Mouse Sensitivity X Modifier", mouseSensitivityXMod);
        PlayerPrefs.SetFloat("Mouse Sensitivity Y Modifier", mouseSensitivityYMod);
        PlayerPrefs.SetFloat("Overall Controller Sensitivity", controllerSensitivityOverall);
        PlayerPrefs.SetFloat("Controller Sensitivity X Modifier", controllerSensitivityXMod);
        PlayerPrefs.SetFloat("Controller Sensitivity Y Modifier", controllerSensitivityYMod);
    }

    private void RetrieveSavedSettings() {
        // Check to see if player prefs have been saved to previously.
        if (PlayerPrefs.GetFloat("Overall Mouse Sensitivity") != 0 && PlayerPrefs.GetFloat("Overall Controller Sensitivity") != 0) {
            mouseSensitivityOverall = PlayerPrefs.GetFloat("Overall Mouse Sensitivity");
            mouseSensitivityXMod = PlayerPrefs.GetFloat("Mouse Sensitivity X Modifier");
            mouseSensitivityYMod = PlayerPrefs.GetFloat("Mouse Sensitivity Y Modifier");
            controllerSensitivityOverall = PlayerPrefs.GetFloat("Overall Controller Sensitivity");
            controllerSensitivityXMod = PlayerPrefs.GetFloat("Controller Sensitivity X Modifier");
            controllerSensitivityYMod = PlayerPrefs.GetFloat("Controller Sensitivity Y Modifier");
        }

        // If player prefs do not already exist for these settings, create them.
        else {
            Debug.Log("Creating player prefs.");
            SaveSettings();
        }
    }
}