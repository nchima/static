using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Initial : MonoBehaviour {

    // Whether I am currently being controlled by the player.
    bool active;
    [HideInInspector] public bool Active
    {
        get { return active; }

        set
        {
            if (value == true)
            {
                textMesh.color = parentScript.activeColor;
                transform.localScale = Vector3.one * parentScript.activeScale;
            }

            else
            {
                textMesh.color = parentScript.inactiveColor;
                transform.localScale = Vector3.one * parentScript.inactiveScale;
            }
        }
    }

    int _charIndex;
    public int charIndex
    {
        get
        {
            return _charIndex;
        }

        set
        {
            if (value < 0) value = InitialEntry.letters.Length - 1;
            else if (value > InitialEntry.letters.Length - 1) value = 0;
            textMesh.text = InitialEntry.letters[value].ToString();
            _charIndex = value;
        }
    }    // The character I am currently displaying.

    TextMesh textMesh;
    InitialEntry parentScript;

    void Awake()
    {
        textMesh = GetComponent<TextMesh>();
        parentScript = GetComponentInParent<InitialEntry>();
    }

    public void SetChar(char newChar)
    {
        for (int i = 0; i < InitialEntry.letters.Length; i++)
        {
            if (InitialEntry.letters[i] == newChar)
            {
                charIndex = i;
            }
        }
    }
}
