using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialMoveAmmo : Pickup {

    [SerializeField] float ammoValue = 0.1f;

    protected override void GetAbsorbed() {
        base.GetAbsorbed();
        Services.specialBarManager.PlayerAbsorbedAmmo(ammoValue);
    }
}
