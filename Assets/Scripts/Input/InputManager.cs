﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

            if (Input.GetAxisRaw("Horizontal Controller") != 0) { return true; }
            if (Input.GetAxisRaw("Vertical Controller") != 0) { return true; }
            if (Input.GetAxisRaw("Controller Right Stick Horizontal") != 0) { return true; }
            if (Input.GetAxisRaw("Controller Right Stick Vertical") != 0) { return true; }
            if (Input.GetAxisRaw("Fire Controller") != 0) { return true; }
            if (Input.GetAxisRaw("Dash Controller") != 0) { return true; }
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


    void Awake() {
        inputMode = InputMode.MouseAndKeyboard;
    }


    const float UI_INPUT_COOLDOWN = 0.1f;
    float uiInputTimer = UI_INPUT_COOLDOWN;
    private void Update() {

        ResetTriggers();

        // Figure out which input mode to use.
        if (MouseAndKeyboardUsed) {
            inputMode = InputMode.MouseAndKeyboard;
            Services.uiManager.SwitchControlPrompts(InputMode.MouseAndKeyboard);
        }

        if (AnyControllerButtonPressed) {
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
        uiInputTimer += Time.deltaTime;
        if (movementAxis == Vector2.zero) { uiInputTimer = UI_INPUT_COOLDOWN; }
        if (uiInputTimer >= UI_INPUT_COOLDOWN) {

            AxisEventData ad = new AxisEventData(EventSystem.current);
            ad.moveDir = MoveDirection.None;
            if (movementAxis.x < 0) { ad.moveDir = MoveDirection.Left; } 
            else if (movementAxis.x > 0) { ad.moveDir = MoveDirection.Right; } 
            else if (movementAxis.y < 0) { ad.moveDir = MoveDirection.Down; } 
            else if (movementAxis.y > 0) { ad.moveDir = MoveDirection.Up; }

            if (ad.moveDir != MoveDirection.None) {
                //ExecuteEvents.Execute(EventSystem.current.currentSelectedGameObject, ad, ExecuteEvents.deselectHandler);
                ExecuteEvents.Execute(EventSystem.current.currentSelectedGameObject, ad, ExecuteEvents.moveHandler);
                uiInputTimer = 0f;
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

        turningValue = Input.GetAxis("Mouse X");

        gunTuningValue = Input.GetAxis("Mouse Y");

        fireButton = Input.GetButton("Fire Mouse and Keyboard");
        fireButtonDown = Input.GetButtonDown("Fire Mouse and Keyboard");
        fireButtonUp = Input.GetButtonUp("Fire Mouse and Keyboard");

        specialMoveButton = Input.GetButton("Special Move Mouse and Keyboard");
        specialMoveButtonDown = Input.GetButtonDown("Special Move Mouse and Keyboard");
        specialMoveButtonUp = Input.GetButtonUp("Special Move Mouse and Keyboard");

        dashButton = Input.GetButton("Dash Mouse and Keyboard");
        dashButtonDown = Input.GetButtonDown("Dash Mouse and Keyboard");
        dashButtonUp = Input.GetButtonUp("Dash Mouse and Keyboard");

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

        dashButton = Input.GetAxisRaw("Dash Controller") != 0;
        dashButtonDown = Input.GetAxisRaw("Dash Controller") != 0;
        //dashButtonUp = Input.GetButton("Dash Controller");

        pauseButton = Input.GetButton("Start");
        pauseButtonDown = Input.GetButtonDown("Start");

        submitButton = Input.GetButton("Special Move Controller");
        submitButtonDown = Input.GetButtonDown("Special Move Controller");

        cancelButton = Input.GetButton("Cancel");
        cancelButtonDown = Input.GetButtonDown("Cancel");
    }
}