using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SliderHighlighter : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler {

    [SerializeField] GameObject selectionBox;

    bool isUsingControllerInput;

    private void Awake() {
        isUsingControllerInput = InputManager.inputMode == InputManager.InputMode.Controller;
    }

    private void OnDisable() {
        selectionBox.SetActive(false);
    }

    private void Update() {
        bool inputMethodChanged = isUsingControllerInput && InputManager.inputMode != InputManager.InputMode.Controller;
        inputMethodChanged |= !isUsingControllerInput && InputManager.inputMode == InputManager.InputMode.Controller;
        if (inputMethodChanged) {
            isUsingControllerInput = InputManager.inputMode == InputManager.InputMode.Controller;
            selectionBox.SetActive(false);
        }
    }

    public void OnSelect(BaseEventData eventData) {
        selectionBox.SetActive(true);
    }

    public void OnDeselect(BaseEventData eventData) {
        selectionBox.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        selectionBox.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData) {
        selectionBox.SetActive(false);
    }
}
