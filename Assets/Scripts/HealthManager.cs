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
                Debug.Log("doing this");
                getHurtAudio.Play();
                GameObject.Find("Screen").BroadcastMessage("IncreaseShake", 0.3f);
                //GameObject.Find("Screen").BroadcastMessage("IncreaseResShift", 0.5f);
                GameObject.Find("Pain Flash").GetComponent<Animator>().SetTrigger("Pain Flash");

                // Delete health box.
                for (int i = Mathf.Clamp(value, 0, 4); i < 5; i++) healthBlocks[i].SetActive(false);
                //for (int i = 0; i < healthBlocks.Length; i++) { healthBlocks[i].SetActive(false); }
            }

            else
            {
                // Reactivate health box.
                if (value <= 5) healthBlocks[value - 1].SetActive(true);
                //for(int i = 0; i < healthBlocks.Length; i++) { healthBlocks[i].SetActive(true); }
            }

            //Debug.Log(value);

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
