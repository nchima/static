using UnityEngine;
using System.Collections;

public class BatchBillboard : MonoBehaviour {

	private Transform[] billboardTransforms;    // References to the transforms of all sprites which should be billboarded.

	private int indexStart = 0; // Where we should start when iterating through the array of billboards.
	private int indexSkip = 1;  // How many billboard objects we should skip this frame.


	private void Start() {
        FindAllBillboards();
	}


	private void Update() {
        // Determine where in the array to start.
        indexStart = 0;

        FindAllBillboards();

        // Skip through the array and rotate chosen billboards to face the camera.
		for (int i = 0; i < billboardTransforms.Length; i++) {
			if (billboardTransforms [i] != null) {
                float zRot = billboardTransforms[i].rotation.eulerAngles.z;
                Vector3 newRotation = Camera.main.transform.rotation.eulerAngles;
                newRotation.z = zRot;
                billboardTransforms[i].rotation = Quaternion.Euler(newRotation);
            }
		}
	}


    /// <summary>
    /// Finds all objects tagged with "Billboard" in the scene and begins billboarding them.
    /// </summary>
    public void FindAllBillboards() {
        // Find and store references to all sprites which should be billboarded.
        GameObject[] billboards = GameObject.FindGameObjectsWithTag("Billboard");
        billboardTransforms = new Transform[billboards.Length];
        for (int i = 0; i < billboards.Length; i++) {
            billboardTransforms[i] = billboards[i].transform;
        }
    }
}
