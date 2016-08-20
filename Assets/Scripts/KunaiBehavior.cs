using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class KunaiBehavior : InteractObject {

    private Rigidbody rig;

    public float lifeSpan = 5.0f;

    private enum handleMode
    {
        Block, Throw
    }

    private handleMode state = handleMode.Block;

    // Use this for initialization
    new void Start () {
        base.Start();

        rig = GetComponent<Rigidbody>();
        Debug.Log("Kunai rig: " + rig);
	}
	
	// Update is called once per frame
	new void Update ()
    {
        base.Update();
        if (IsInteracting())
        {   //
        } else
        {   // active!

            // Lower life until dead
            lifeSpan -= Time.deltaTime;
            if (lifeSpan <= 0.0f)
            {
                Destroy(gameObject);
            }

        }
    }

    new void FixedUpdate()
    {
        base.FixedUpdate();

        // Look in velocity direction when flying
        if (!IsInteracting())
        {
            if (state == handleMode.Throw)
            {   // If we threw the kunai, make it face forward, cuz we is NINJA MASTA!
                Vector3 lookin = rig.velocity;
                if (lookin.sqrMagnitude >= 4.0f)
                    transform.LookAt(lookin + transform.position, new Vector3(0, 0, Mathf.Sin(Time.time * Mathf.PI) * 360.0f));
            }
        }
    }

    void OnCollisionEnter(Collision col)
    {
        //Debug.Log("Collision! " + col.ToString());
    }

    public override void InitPickup(WandController wand, int maxCount, Valve.VR.EVRButtonId btn)
    {
        
        if (btn == Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger)
        {   // Throw mode with trigger
            state = handleMode.Throw;
            usingJoint = true;
        } else if (btn == Valve.VR.EVRButtonId.k_EButton_Grip)
        {   // Block mode with grip
            usingJoint = false;
            state = handleMode.Block;
        }

        wand.pickupObject(this, maxCount, btn);
    }

    public override void OnGripDown(WandController wand)
    {
        InitPickup(wand, 1, Valve.VR.EVRButtonId.k_EButton_Grip);
    }

    public override void BeginInteraction(GameObject wand)
    {
        base.BeginInteraction(wand);

        if (state == handleMode.Block)
            SetInteractionPointLocal(new Vector3(0, -0.01f, -0.02f), new Vector3(-184, 0, 0));
        else
        {
            SetInteractionPointLocal(new Vector3(0, -0.01f, -0.02f), new Vector3(-4, 0, 0));
            

            // Attach to wand's hinge joint
            SpringJoint jnt = wand.GetComponent<SpringJoint>();
            if (rig == null)    // For some reason our rig gets lost before we get here :(
                rig = GetComponent<Rigidbody>();
            jnt.connectedBody = rig;
            jnt.connectedAnchor = new Vector3(0,0, 0.028f);
            //Debug.Log(jnt.connectedBody);
        }
    }

    public override void EndInteractionFromNetwork(Vector3 pos, Quaternion rot, Vector3 vel, Vector3 avel)
    {
        base.EndInteractionFromNetwork(pos, rot, vel, avel);
    }

    public override void EndInteraction(GameObject wand)
    {
        bool wasInteracting = IsInteracting();
        
        // Dettach from wand's joint
        SpringJoint jnt = wand.GetComponent<SpringJoint>();
        if (rig == null)    // For some reason our rig gets lost before we get here :(
            rig = GetComponent<Rigidbody>();
        if (jnt.connectedBody == rig)
            jnt.connectedBody = null;

        base.EndInteraction(wand);
    }
}
