using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PathSelectedScreen : MonoBehaviour {

    [SerializeField] Text nextEpisodeText;

    public void UpdateText(string episodeName) {
        nextEpisodeText.text = "\"" + episodeName + "\"";
    }
}
