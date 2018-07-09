using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBox : MonoBehaviour {

    [SerializeField] GameObject myEmptySelf;
    [SerializeField] GameObject myFullSelf;
    [SerializeField] GameObject myX;

    public void BecomeEmpty() {
        myEmptySelf.SetActive(true);
        myFullSelf.SetActive(false);
    }

    public void BecomeFull(){
        myEmptySelf.SetActive(false);
        myFullSelf.SetActive(true);
    }

    public void BecomeXedOut() {
        myX.SetActive(true);
    }

    public void BecomeUnXedOut() {
        myX.SetActive(false);
    }
}
