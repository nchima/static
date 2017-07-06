using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class GameOverScreen : MonoBehaviour
{

    GameObject player;
    ScoreManager scoreManager;
    [SerializeField] GameObject nameEntryScreen;


    private void Awake()
    {
        // Enable cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Disable player controls.
        player = GameObject.Find("FPSController");
        player.GetComponent<FirstPersonController>().m_MouseLook.lockCursor = false;
        player.GetComponent<CharacterController>().center = new Vector3(player.GetComponent<CharacterController>().center.x, player.GetComponent<CharacterController>().center.y - 0.5f, player.GetComponent<CharacterController>().center.z);
        player.GetComponent<FirstPersonController>().m_WalkSpeed = 0;
        player.GetComponent<FirstPersonController>().m_RunSpeed = 0;

        // Disable gun object.
        foreach (Gun gun in FindObjectsOfType<Gun>())
        {
            gun.enabled = false;
        }

        // Disable floor
        GameObject.Find("Floor").SetActive(false);

        // Turn background red.
        GameObject.Find("Background Grain").GetComponent<MeshRenderer>().material.color = new Color(GameObject.Find("Background Grain").GetComponent<MeshRenderer>().material.color.r + 0.1f, GameObject.Find("Background Grain").GetComponent<MeshRenderer>().material.color.g, GameObject.Find("Background Grain").GetComponent<MeshRenderer>().material.color.b);

        // Update final score number.
        scoreManager = GameObject.Find("Game Manager").GetComponent<ScoreManager>();
        GameObject.Find("Final Score Number").GetComponent<TextMesh>().text = scoreManager.score.ToString();

        // If this is a high score, show the name entry interface.
        if (scoreManager.score > 
            scoreManager.highScoreEntries[9].score)
        {
            nameEntryScreen.SetActive(true);
        }
    }


    private void Update()
    {
        if (!nameEntryScreen.activeSelf && Input.GetButton("Start"))
        {
            GameObject.Find("Game Manager").GetComponent<GameManager>().RestartGame();
        }
    }
}