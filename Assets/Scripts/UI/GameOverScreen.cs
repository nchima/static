using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class GameOverScreen : MonoBehaviour
{

    GameObject player;
    ScoreManager scoreManager;
    [SerializeField] GameObject nameEntryScreen;

    [SerializeField] GameObject specialMoveUIScreen;


    private void Awake()
    {
        // Enable cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Disable player controls.
        GameManager.player.GetComponent<PlayerController>().UnlockCursor();
        //GameManager.player.GetComponent<CapsuleCollider>().center = new Vector3(player.GetComponent<CapsuleCollider>().center.x, player.GetComponent<CapsuleCollider>().center.y - 0.5f, player.GetComponent<CapsuleCollider>().center.z);
        GameManager.player.GetComponent<PlayerController>().isMovementEnabled = false;

        // Disable fall catcher so that a new level is not loaded.
        FindObjectOfType<FallCatcher>().enabled = false;

        // Disable gun object.
        foreach (Gun gun in FindObjectsOfType<Gun>()) {
            gun.enabled = false;
        }

        // Disable other UI elements.
        specialMoveUIScreen.SetActive(false);

        // Disable floor
        GameManager.levelManager.SetFloorCollidersActive(false);

        // Turn background red.
        //GameObject.Find("Background Grain").GetComponent<MeshRenderer>().material.color = new Color(GameObject.Find("Background Grain").GetComponent<MeshRenderer>().material.color.r + 0.1f, GameObject.Find("Background Grain").GetComponent<MeshRenderer>().material.color.g, GameObject.Find("Background Grain").GetComponent<MeshRenderer>().material.color.b);

        // Update final score number.
        scoreManager = GameObject.Find("Game Manager").GetComponent<ScoreManager>();
        transform.Find("Final Score Number").GetComponent<TextMesh>().text = scoreManager.score.ToString();

        // Update level complete number.
        transform.Find("You Reached Level X Text").GetComponent<TextMesh>().text = "Y O U  R E A C H E D  L E V E L  " + GameManager.levelManager.LevelNumber.ToString();

        // If this is a high score, show the name entry interface.
        if (scoreManager.score > 
            scoreManager.highScoreEntries[9].score)
        {
            nameEntryScreen.SetActive(true);
        }
    }


    private void Update()
    {
        if (!nameEntryScreen.activeSelf && Input.GetButton("Start")) {
            GameObject.Find("Game Manager").GetComponent<GameManager>().RestartGame();
        }
    }
}