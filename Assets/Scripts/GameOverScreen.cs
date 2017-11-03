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
        player = FindObjectOfType<PlayerController>().gameObject;
        player.GetComponent<PlayerController>().UnlockCursor();
        player.GetComponent<CapsuleCollider>().center = new Vector3(player.GetComponent<CapsuleCollider>().center.x, player.GetComponent<CapsuleCollider>().center.y - 0.5f, player.GetComponent<CapsuleCollider>().center.z);
        player.GetComponent<PlayerController>().maxAirSpeed = 0;
        //player.GetComponent<PlayerController>(). = 0;

        // Disable gun object.
        foreach (Gun gun in FindObjectsOfType<Gun>())
        {
            gun.enabled = false;
        }

        // Disable floor
        GameManager.instance.SetFloorCollidersActive(false);
        //GameObject.Find("Floor").SetActive(false);

        // Turn background red.
        GameObject.Find("Background Grain").GetComponent<MeshRenderer>().material.color = new Color(GameObject.Find("Background Grain").GetComponent<MeshRenderer>().material.color.r + 0.1f, GameObject.Find("Background Grain").GetComponent<MeshRenderer>().material.color.g, GameObject.Find("Background Grain").GetComponent<MeshRenderer>().material.color.b);

        // Update final score number.
        scoreManager = GameObject.Find("Game Manager").GetComponent<ScoreManager>();
        transform.Find("Final Score Number").GetComponent<TextMesh>().text = scoreManager.score.ToString();

        // Update level complete number.
        transform.Find("You Reached Level X Text").GetComponent<TextMesh>().text = "Y O U  R E A C H E D  L E V E L  " + GameManager.levelManager.currentLevelNumber.ToString();

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