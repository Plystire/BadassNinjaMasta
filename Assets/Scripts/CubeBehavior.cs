using UnityEngine;
using System.Collections;

public class CubeBehavior : InteractObject {

	// Use this for initialization
	new void Start () {
        base.Start();
	}
	
	// Update is called once per frame
	new void Update () {
        base.Update();
	}

    public override void OnTriggerDown(WandController wand)
    {
        wand.pickupObject(this, 1, Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger);
    }
}
