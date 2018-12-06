using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkipSeizureWarning : MonoBehaviour {

    public bool skipSeizureWarning = false;

    private void Start() {
        if (FindObjectsOfType<SkipSeizureWarning>().Length == 1) {
            DontDestroyOnLoad(this);
        } else {
            Destroy(gameObject);
        }
    }
}
