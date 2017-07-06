using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepUpright : MonoBehaviour {

	private void Update ()
    {
        // Keep self upright
        transform.rotation = Quaternion.identity;
	}
}
