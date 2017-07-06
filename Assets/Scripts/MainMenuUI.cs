using UnityEngine;
using UnityEngine.SceneManagement;
using UnityStandardAssets.Characters.FirstPerson;
using System.Collections;

public class MainMenuUI : MonoBehaviour {

    private void Update()
    {
        // Get controller input.
		if (Input.GetButtonDown("Start") || Input.GetKeyDown(KeyCode.Return) || Input.GetButtonDown("Fire1"))
        {
            PlayButton();
        }

        // Use escape to quit game.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    public void PlayButton()
    {
        // Unpause enemies in the background.
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            enemy.GetComponent<Enemy>().enabled = true;
        }

        // Enable player movement and shooting.
        GameObject.Find("FPSController").GetComponent<FirstPersonController>().enabled = true;
        foreach (Gun gun in FindObjectsOfType<Gun>())
        {
            gun.enabled = true;
        }

        // Turn off the main menu.
        transform.parent.gameObject.SetActive(false);

        FindObjectOfType<GameManager>().gameStarted = true;
    }


	public void QuitButton()
    {
		Application.Quit ();
	}


	public void InfoButton()
    {
		SceneManager.LoadScene ("InfoScene");
	}
}
