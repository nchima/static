using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointPickup : Pickup {

    [SerializeField] int pointValue;

	protected override void GetAbsorbed() {
        base.GetAbsorbed();
        Services.scoreManager.score += pointValue;
    }
}
