using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class KunaiBehavior : InteractObject {

    private Rigidbody rig;

    public float maxAutoAimAngle = 10.0f;
    public float throwingVelocityMultiplier = 1.0f;

    public float lifeSpan = 5.0f;
    
    private GameObject autoAimTarget = null;

    private enum handleMode
    {
        Block, Throw
    }

    private handleMode state = handleMode.Block;

    // Use this for initialization
    new void Start () {
        base.Start();

        rig = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	new void Update ()
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

    new void FixedUpdate()
    {
        base.FixedUpdate();

        // Look in velocity direction
        Vector3 lookin = rig.velocity;
        transform.LookAt(lookin + transform.position, new Vector3(0,0, Mathf.Sin(Time.time * Mathf.PI) * 360.0f));
    }

    void OnCollisionEnter(Collision col)
    {
        //Debug.Log("Collision! " + col.ToString());
    }

    public override void InitPickup(WandController wand, int maxCount, Valve.VR.EVRButtonId btn)
    {
        // Grab StarWandCtrllr and add our star
        //starWand = wand.GetComponent<ThrowingStar_WandController>();
        //starWand.addStar(this);
        
        if (btn == Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger)
        {   // Throw mode with trigger
            state = handleMode.Throw;
        } else if (btn == Valve.VR.EVRButtonId.k_EButton_Grip)
        {   // Block mode with grip
            state = handleMode.Block;
        }

        wand.pickupObject(this, maxCount, btn);
    }

    public override void OnGripDown(WandController wand)
    {
        InitPickup(wand, 1, Valve.VR.EVRButtonId.k_EButton_Grip);
    }

    public override void BeginInteraction(WandController wand)
    {
        base.BeginInteraction(wand);

        if (state == handleMode.Block)
            SetInteractionPointLocal(new Vector3(0,-0.01f,-0.02f), new Vector3(-184,0,0));
        else
            SetInteractionPointLocal(new Vector3(0, -0.01f, -0.02f), new Vector3(-4, 0, 0));
    }

    public override void EndInteraction(WandController wand)
    {
        bool wasInteracting = IsInteracting();

        base.EndInteraction(wand);

        if (wasInteracting)
        {   // Provide throwingMultiplier impulse boost
            Rigidbody rig = GetComponent<Rigidbody>();
            if (rig)
            {
                rig.velocity *= throwingVelocityMultiplier;
                rig.angularVelocity *= throwingVelocityMultiplier;
            }

            #region AutoAim
            // ====================================
            // Auto-aim logic
            //
            // Find object with lowest angle offset from our initial trajectory
            Transform objTrans;
            GameObject tmpGO = new GameObject();
            GameObject tmpVelGO = new GameObject();
            float diffAngle;
            float lowestDiff = float.MaxValue;
            GameObject bestTarget = null;
            
            // Orient our temp velocity object to aim in the direction of our velocity
            tmpVelGO.transform.LookAt(transform.position + rig.velocity);

            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("AutoAimTarget"))
            {   // Filter out best target
                objTrans = obj.GetComponentInParent<Transform>();
                tmpGO.transform.position = transform.position;
                tmpGO.transform.LookAt(objTrans);
                diffAngle = Quaternion.Angle(tmpVelGO.transform.rotation, tmpGO.transform.rotation);
                if (diffAngle < lowestDiff)
                {
                    lowestDiff = diffAngle;
                    bestTarget = obj;
                }
            }

            Destroy(tmpGO);
            Destroy(tmpVelGO);

            // If best target angle is lower than acceptable amount, AUTO-AIM!!!
            if (bestTarget && lowestDiff <= maxAutoAimAngle)
            {
                Debug.Log("Found best auto-aim target: " + bestTarget);
                autoAimTarget = bestTarget;
                // Set velocity to make the shot!
                float mag = rig.velocity.magnitude;

                GameObject go = new GameObject();
                go.transform.position = transform.position;
                float dist = (autoAimTarget.transform.position - transform.position).magnitude;
                float eta = dist / mag;
                go.transform.LookAt(autoAimTarget.transform.position);
                
                rig.velocity = go.transform.forward * mag;
                rig.velocity += -Physics.gravity * (eta / 2.0f);    // Divide by 2 because we only want to counteract gravity for half of our travel time... that is, we reach our peak at half-time, like we should :)
            } else
            {
                Debug.Log("Failed auto-aim: " + lowestDiff);
            }
            #endregion
        }

        // Remove star from StarWand
        //if (starWand)
        //{
        //    starWand.removeStar(this);
        //    starWand = null;
        //}

        // Set to active physics object
        //Rigidbody rigidbody = GetComponent<Rigidbody>();
        //if (rigidbody)
        //{
        //    rigidbody.detectCollisions = true;
        //    rigidbody.isKinematic = false;
        //}
    }
}
