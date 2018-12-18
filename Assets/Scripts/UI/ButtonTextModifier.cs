using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class ButtonTextModifier : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler {

    string m_Text { get { return GetComponentInChildren<Text>().text; } set { GetComponentInChildren<Text>().text = value; } }
    string originalText;
    //string modifiedText;

    bool isUsingControllerInput;

    private void Awake() {
        originalText = m_Text;
        isUsingControllerInput = InputManager.inputMode == InputManager.InputMode.Controller;
    }

    private void Update() {
        bool inputMethodChanged = isUsingControllerInput && InputManager.inputMode != InputManager.InputMode.Controller;
        inputMethodChanged |= !isUsingControllerInput && InputManager.inputMode == InputManager.InputMode.Controller;
        if (inputMethodChanged) {
            isUsingControllerInput = InputManager.inputMode == InputManager.InputMode.Controller;
            m_Text = originalText;
        }
    }

    public void OnSelect(BaseEventData eventData) {
        m_Text = GetModifiedText();
    }

    public void ForceHighlight() {
        m_Text = GetModifiedText();
    }

    public void OnDeselect(BaseEventData eventData) {
        m_Text = originalText;
    }

    public void OnPointerEnter(PointerEventData eventData) {
        m_Text = GetModifiedText();
    }

    public void OnPointerExit(PointerEventData eventData) {
        m_Text = originalText;
    }

    string GetModifiedText() {
        return "> " + originalText;
    }
}

