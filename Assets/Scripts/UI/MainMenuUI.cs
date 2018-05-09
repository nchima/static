using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.FirstPerson;
using System.Collections;

public class MainMenuUI : MonoBehaviour {

    private void Update()
    {
        // Get controller input.
		//if (Input.GetButtonDown("Start") || Input.GetKeyDown(KeyCode.Return) || Input.GetButtonDown("Fire1"))
  //      {
  //          PlayButton();
  //      }

        // Use escape to quit game.
        //if (Input.GetKeyDown(KeyCode.Escape)) {
        //    Application.Quit();
        //}
    }

    public void PlayButton() {
        // Turn off the main menu.
        transform.parent.gameObject.SetActive(false);

        //FindObjectOfType<GameManager>().StartGame();
    }


	public void QuitButton() {
		Application.Quit ();
	}


	public void InfoButton() {
		SceneManager.LoadScene ("InfoScene");
	}
}
