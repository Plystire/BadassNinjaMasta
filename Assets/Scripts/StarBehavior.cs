using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StarBehavior : InteractObject {

    private Rigidbody rig;

    private ThrowingStar_WandController starWand;

    public float maxAutoAimAngle = 10.0f;
    public float throwingVelocityMultiplier = 1.0f;

    public float lifeSpan = 5.0f;

    private bool autoAiming = false;
    private GameObject autoAimTarget = null;

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

            // If auto-aiming, follow target
            if (autoAiming)
            {
                //float mag = rig.velocity.magnitude;

                ////GameObject go = new GameObject();
                ////go.transform.position = transform.position;
                //float dist = (autoAimTarget.transform.position - transform.position).magnitude;
                //transform.LookAt(autoAimTarget.transform.position + ((dist / mag) * -Physics.gravity));

                //rig.velocity = transform.forward * mag;
                //rig.velocity = (rig.velocity / rig.velocity.magnitude) * mag;

                //transform.rotation = go.transform.rotation;

                //Destroy(go);
            }

            // Lower life until dead
            lifeSpan -= Time.deltaTime;
            if (lifeSpan <= 0.0f)
            {
                Destroy(gameObject);
            }
        }
    }

    void OnCollisionEnter(Collision col)
    {
        Debug.Log("Collision! " + col.ToString());
    }

    public void Init(WandController wand)
    {
        // Grab StarWandCtrllr and add our star
        starWand = wand.GetComponent<ThrowingStar_WandController>();
        starWand.addStar(this);

        Debug.Log("[StarBehavior.Init]");

        wand.pickupObject(this, 4, Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger);
    }

    public override void OnTriggerDown(WandController wand)
    {
        Init(wand);
    }

    public override void BeginInteraction(WandController wand)
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
                autoAiming = true;
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
