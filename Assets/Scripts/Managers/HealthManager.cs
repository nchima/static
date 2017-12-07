using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthManager : MonoBehaviour {

    [SerializeField] float healthRechargeRate = 2f;
    float timer;

    int _playerHealth = 5;
    public int playerHealth
    {
        get
        {
            return _playerHealth;
        }

        set
        {
            // Audio/visual effects:
            if (value < _playerHealth)
            {
                getHurtAudio.Play();
                GameObject.Find("Screen").BroadcastMessage("IncreaseShake", 1f);
                GameManager.colorPaletteManager.LoadVulnerablePalette();
                GameManager.player.GetComponent<Rigidbody>().velocity *= 0.01f;
                GameObject.Find("Pain Flash").GetComponent<Animator>().SetTrigger("Pain Flash");

                // Delete health box.
                for (int i = Mathf.Clamp(value, 0, 4); i < 5; i++) healthBlocks[i].SetActive(false);
            }

            else
            {
                // Reactivate health box.
                if (value <= 5) healthBlocks[Mathf.Clamp(value - 1, 0, 5)].SetActive(true);
                if (value == 5) GameManager.colorPaletteManager.RestoreSavedPalette();
            }

            // Lower health.
            _playerHealth = value;
        }
    }

    [SerializeField] GameObject[] healthBlocks;
    [SerializeField] AudioSource getHurtAudio;


    private void Update()
    {
        if (playerHealth < 5)
        {
            timer += Time.deltaTime;
            if (timer >= healthRechargeRate)
            {
                playerHealth++;
                timer = 0f;
            }
        }
    }
}
