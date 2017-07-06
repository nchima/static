using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PauseMenu : MonoBehaviour {

    //* THIS SCRIPT WILL EVENTUALLY CONTAIN CODE FOR A THEORETICAL PAUSE MENU - RIGHT NOW IT JUST RESETS THE SCENE WHEN THE PLAYER PRESSES ESCAPE *//

	void Update()
    {
		if (Input.GetButtonDown("Back"))
        {
			MenuButton();
		}
	}

	public void MenuButton()
    {
        // Reload the scene.
		SceneManager.LoadScene ("mainScene");

        // Unlock and show cursor.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
