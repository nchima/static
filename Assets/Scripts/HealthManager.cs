using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthManager : MonoBehaviour {

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
            getHurtAudio.Play();
            GameObject.Find("Screen").BroadcastMessage("IncreaseShake", 0.3f);
            GameObject.Find("Pain Flash").GetComponent<Animator>().SetTrigger("Pain Flash");

            // Delete health box.
            if (value >= 0) healthBlocks[value].SetActive(false);

            // Lower health.
            _playerHealth = value;
        }
    }

    [SerializeField] GameObject[] healthBlocks;
    [SerializeField] AudioSource getHurtAudio;
}
