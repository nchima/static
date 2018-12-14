using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class ButtonTextModifier : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler {

    string m_Text { get { return GetComponentInChildren<Text>().text; } set { GetComponentInChildren<Text>().text = value; } }
    string originalText;
    string modifiedText;

    private void Awake() {
        originalText = m_Text;
        modifiedText = GetModifiedText();
    }

    public void OnSelect(BaseEventData eventData) {
        m_Text = modifiedText;
    }

    public void OnDeselect(BaseEventData eventData) {
        m_Text = originalText;
    }

    public void OnPointerEnter(PointerEventData eventData) {
        m_Text = modifiedText;
    }

    public void OnPointerExit(PointerEventData eventData) {
        m_Text = originalText;
    }

    string GetModifiedText() {
        return "> " + originalText;
    }
}

