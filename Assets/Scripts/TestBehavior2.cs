using UnityEngine;
using System.Collections;
using Valve.VR;

public class TestBehavior2 : PlyWare_InteractObject {

    private bool track = false;
    private GameObject trig = null;

    [Space(15)]
    [Header("TestBehavior")]
    public GameObject target;

	// Use this for initialization
	new void Start () {
	    
	}
	
	// Update is called once per frame
	new void Update ()
    {
        if (track)
        {
            //Debug.Log("Update");
            //track = false;
        }
    }

    new void FixedUpdate()
    {
        if (track)
        {
            //Debug.Log("FixedUpdate");
            //track = false;
        }
    }

    void OnTriggerEnter(Collider col)
    {
        //Debug.Log("Trigger entered");
        track = true;
        trig = col.gameObject;
    }

    public override void WandButtonDown(PlyWare_WandController wand, EVRButtonId btn)
    {
        base.WandButtonDown(wand, btn);

        Debug.Log(btn);

        switch(btn)
        {
            case EVRButtonId.k_EButton_SteamVR_Trigger:
                if(target)
                {
                    Debug.LogWarning("Run autoPickup");


                    SteamVR_TrackedObject to = wand.GetComponent<SteamVR_TrackedObject>();
                    to.enabled = false;

                    wand.transform.position = target.transform.position;
                    wand.transform.rotation = target.transform.rotation;

                    wand.autoPickup = EVRButtonId.k_EButton_SteamVR_Trigger;
                }
                break;
        }
    }
}
