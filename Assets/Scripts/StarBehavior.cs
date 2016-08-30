using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StarBehavior : InteractObject {

    private Rigidbody rig;

    private ThrowingStar_WandController starWand;

    public float lifeSpan = 5.0f;

    // Use this for initialization
    public override void Start () {
        base.Start();

        rig = GetComponent<Rigidbody>();
	}

    // Update is called once per frame
    public override void Update ()
    {
        base.Update();
        if (IsInteracting())
        {   //
        } else
        {   // Star is active!

            // Lower life until dead
            lifeSpan -= Time.deltaTime;
            if (lifeSpan <= 0.0f)
            {
                Destroy(gameObject);
            }
        }
    }

    public override void InitNetworkPickup(GameObject wand, int maxCount)
    {
        starWand = wand.GetComponent<ThrowingStar_WandController>();
        starWand.addStar(this);

        BeginInteraction(wand);
    }

    public override void InitPickup(WandController wand, int maxCount, Valve.VR.EVRButtonId btn)
    {
        // Grab StarWandCtrllr and add our star
        starWand = wand.GetComponent<ThrowingStar_WandController>();
        starWand.addStar(this);

        wand.pickupObject(this, maxCount, btn);
    }

    public override void OnTriggerDown(WandController wand)
    {
        InitPickup(wand, 4, Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger);
    }

    public override void BeginInteraction(GameObject wand)
    {
        base.BeginInteraction(wand);

        // Override interactionPoint using starOffsets
        if (starWand) {
            Vector3 posOffset;
            Vector3 rotOffset;
            //Debug.Log("[StarBeh.BeginInter] Enter");
            starWand.getStarOffsets(this, out posOffset, out rotOffset);
            //Debug.Log("[StarBeh.BeginInter] posOffset: " + posOffset);
            //Debug.Log("[StarBeh.BeginInter] rotOffset: " + (wand.transform.rotation.eulerAngles + rotOffset));

            //Debug.Log("IntPt: " + GetInteractionPoint().rotation);
            SetInteractionPointLocal(posOffset, rotOffset);
            //Debug.Log("AfterIntPt: " + GetInteractionPoint().rotation);
        } else
        {
            Debug.Log("[StarBehavior.BeginInteraction] StarWand not set!");
        }
        
        if(rig)
        {
            rig.detectCollisions = false;
            rig.isKinematic = true;
        } else
        {   // Try to get it again
            rig = GetComponent<Rigidbody>();
            if (rig)
            {
                rig.detectCollisions = false;
                rig.isKinematic = true;
            }
        }
    }

    public override void EndInteractionFromNetwork(Vector3 pos, Quaternion rot, Vector3 vel, Vector3 avel)
    {
        Debug.LogWarning("Threw star!");

        base.EndInteractionFromNetwork(pos, rot, vel, avel);

        // Remove star from StarWand
        if (starWand)
        {
            starWand.removeStar(this);
            starWand = null;
        }

        // Set to active physics object
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        if (rigidbody)
        {
            rigidbody.detectCollisions = true;
            rigidbody.isKinematic = false;
        }
    }

    public override void EndInteraction(GameObject wand)
    {
        bool wasInteracting = IsInteracting();
        
        base.EndInteraction(wand);

        // Remove star from StarWand
        if (starWand)
        {
            starWand.removeStar(this);
            starWand = null;
        }

        // Set to active physics object
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        if (rigidbody)
        {
            rigidbody.detectCollisions = true;
            rigidbody.isKinematic = false;
        }
    }
}
