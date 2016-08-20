using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class InteractObject : MonoBehaviour {

    new private Rigidbody rigidbody;

    private bool currentlyInteracting = false;

    private WandController attachedWand;

    private Transform interactionPoint;
    
    public bool networkMode = false;

    public float maxAutoAimAngle = 10.0f;
    public float throwingVelocityMultiplier = 1.0f;

    private float velocityFactor = 20000.0f;
    public float GetVelocityFactor() { return velocityFactor; }
    private Vector3 deltaPos;
    private Vector3 lastPos;
    public Vector3 GetDeltaPos() { return deltaPos; }
    private float rotationFactor = 200000.0f;
    public float GetRotationFactor() { return rotationFactor; }
    private Quaternion deltaRot;
    private Quaternion lastRot;
    public Quaternion GetDeltaRot() { return deltaRot; }
    private float angle;
    private Vector3 axis;

    // PUBLICS
    //
    // Time in seconds that sticky pickup will occur
    public float stickyPickup = 0.0f;
    //
    // will object lag behind based on mass or will it snap onto wand
    public bool snapHold = false;

    public bool usingJoint = false;

    virtual public void OnTriggerDown(WandController wand) { }
    virtual public void OnTriggerUp(WandController wand) { }

    virtual public void OnGripDown(WandController wand) { }
    virtual public void OnGripUp(WandController wand) { }

    // Use this for initialization
    public void Start () {
        rigidbody = GetComponent<Rigidbody>();
        //interactionPoint = new GameObject().transform;
        if (rigidbody)
        {
            rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            velocityFactor /= rigidbody.mass;
            rotationFactor /= rigidbody.mass;
        }

        lastPos = new Vector3();
        lastRot = new Quaternion();
    }

    public void Update()
    {
        // Do update stuff
    }
	
	// Update is called once per frame
	public void FixedUpdate () {
        if (currentlyInteracting)
        {
            if (!usingJoint)
            {
                if (!snapHold)
                {   // Update physics to follow attached wand
                    deltaPos = interactionPoint.position - transform.position;
                    if (deltaPos.sqrMagnitude < 0.001)
                    {   // Snap to controller if our distance is low enough (Prevents velocity flicker)
                        transform.position = interactionPoint.position;
                        rigidbody.velocity = new Vector3();
                    }
                    else
                    {   // Transition to our desired position via velocity
                        rigidbody.velocity = deltaPos * velocityFactor * Time.fixedDeltaTime;
                    }

                    deltaRot = interactionPoint.rotation * Quaternion.Inverse(transform.rotation);
                    deltaRot.ToAngleAxis(out angle, out axis);

                    if (angle < 1 && angle > -1)
                    {   // Snap to controller is our distance is low enough (Prevent velocity flicker)
                        transform.rotation = interactionPoint.rotation;
                        rigidbody.angularVelocity = new Vector3();
                    }
                    else
                    {   // Transition to our desired rotation
                        if (angle > 180)
                        {
                            angle -= 360;
                        }
                        else if (angle < -180)    // Only you can prevent wrap jump
                        {
                            angle += 360;
                        }

                        rigidbody.angularVelocity = (Time.fixedDeltaTime * angle * axis) * rotationFactor;
                    }
                }
                else
                {   // Snap to wand
                    transform.position = interactionPoint.position;
                    transform.rotation = interactionPoint.rotation;

                    // If we have a rigidbody, ensure our velocities are reset so forces do not accumulate
                    Rigidbody rig = GetComponent<Rigidbody>();
                    if (rig)
                    {
                        rig.velocity = Vector3.zero;
                        rig.angularVelocity = Vector3.zero;
                    }

                    // Track our deltas for when we let go
                    deltaPos = transform.position - lastPos;

                    deltaRot = transform.rotation * Quaternion.Inverse(lastRot);
                    deltaRot.ToAngleAxis(out angle, out axis);

                    lastPos = transform.position;
                    lastRot = transform.rotation;
                }
            }
        }
	}

    private List<GameObject> destroyList = new List<GameObject>();

    /// <summary>
    /// Destroy obj when this object is destroyed
    /// </summary>
    /// <param name="obj"></param>
    public void DestroyObjectOnDestroy(GameObject obj)
    {   // Add to destroy list
        destroyList.Add(obj);
    }
    
    void OnDestroy()
    {
        if (interactionPoint)
        {   // We need to dispose of the interactionPoint because it will set its parent to root and the GameObject will stick around after destruction. Hurray detailed comments :D
            Destroy(interactionPoint.gameObject);
        }

        if (destroyList.Count != 0)
        {   // Destroy any game objects we're keeping track of
            foreach (GameObject obj in destroyList)
            {
                try
                {
                    Destroy(obj.gameObject);
                } catch(Exception) { }  // If we fail, don't worry. It may have been destroyed at some other point
            }
        }
    }

    public virtual void InitNetworkPickup(GameObject wand, int maxCount)
    {
        BeginInteraction(wand);
    }

    public virtual void InitPickup(WandController wand, int maxCount, Valve.VR.EVRButtonId btn)
    {
        wand.pickupObject(this, maxCount, btn);
    }

    public virtual void BeginInteraction(GameObject wand)
    {
        if (wand)
        {
            if(interactionPoint == null)
                interactionPoint = new GameObject().transform;  // If we lost it, make a new one

            attachedWand = wand.GetComponent<WandController>();
            interactionPoint.position = wand.transform.position;
            interactionPoint.rotation = wand.transform.rotation;
            interactionPoint.SetParent(wand.transform, true);

            currentlyInteracting = true;

            // Start timer for sticky pickup
            if (attachedWand)
                attachedWand.stickyPickup(stickyPickup);
        }
    }

    public void SetInteractionPoint(Vector3 pos, Vector3 rot)
    {
        if (pos != null)
            interactionPoint.position = pos;
        if (rot != null)
            interactionPoint.rotation = Quaternion.Euler(rot);
    }

    public void SetInteractionPointLocal(Vector3 pos, Vector3 rot)
    {
        if (pos != null)
            interactionPoint.localPosition = pos;
        if (rot != null)
            interactionPoint.localRotation = Quaternion.Euler(rot);
    }

    public virtual void EndInteractionFromNetwork(Vector3 pos, Quaternion rot, Vector3 vel, Vector3 avel)
    {
        // Apply orientation and velocities from network
        transform.position = pos;
        transform.rotation = rot;
        rigidbody.velocity = vel;
        rigidbody.angularVelocity = avel;

        attachedWand = null;
        currentlyInteracting = false;

        Rigidbody rig = GetComponent<Rigidbody>();
        if (rig)
        {
            rig.velocity *= throwingVelocityMultiplier;
            rig.angularVelocity *= throwingVelocityMultiplier;

            #region AutoAim
            if (maxAutoAimAngle > 0)
            {
                // ====================================
                // Auto-aim logic
                //
                // Find object with lowest angle offset from our initial trajectory
                Transform objTrans;
                GameObject tmpGO = new GameObject();
                tmpGO.name = "tmpGO";
                GameObject tmpVelGO = new GameObject();
                tmpVelGO.name = "tmpVelGO";
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
                    //Debug.Log("Found best auto-aim target: " + bestTarget);
                    // Set velocity to make the shot!
                    float mag = rig.velocity.magnitude;

                    GameObject go = new GameObject();
                    go.name = "go";
                    go.transform.position = transform.position;
                    float dist = (bestTarget.transform.position - transform.position).magnitude;
                    float eta = dist / mag;
                    go.transform.LookAt(bestTarget.transform.position);

                    rig.velocity = go.transform.forward * mag;
                    rig.velocity += -Physics.gravity * (eta / 2.0f);    // Divide by 2 because we only want to counteract gravity for half of our travel time... that is, we reach our peak at half-time, like we should :)
                    Destroy(go);
                }
                else
                {
                    Debug.Log("Failed auto-aim: " + lowestDiff);
                }
            }
            #endregion
        }
    }

    public virtual void EndInteraction(GameObject wand)
    {
        if (IsInteracting() && snapHold)
        {   // Update physics to follow attached wand when snapHold, otherwise physics won't be applied
            rigidbody.velocity = deltaPos * velocityFactor * Time.fixedDeltaTime;
            rigidbody.angularVelocity = (Time.fixedDeltaTime * angle * axis) * rotationFactor;

            if (networkMode)
            {   // Raise network event to inform the masses
                Dictionary<string, object> content = new Dictionary<string, object>();
                RaiseEventOptions REO = new RaiseEventOptions();
                content.Add("pos", transform.position);
                content.Add("rot", transform.rotation);
                content.Add("vel", rigidbody.velocity);
                content.Add("avel", rigidbody.angularVelocity);
                content.Add("isRight", wand.name.Contains("right"));
                NetworkEventManager.RaiseEvent((byte)NetworkEventManager.EventCodes.ReleaseWeapon, content, true, REO);
            }
            

#if DEBUG
            Debug.Log("wandPos: " + attachedWand.transform.position);
            Debug.Log("Pos: " + rigidbody.velocity + " ; delta: " + deltaPos + " ; factor: " + velocityFactor + " ; time: " + Time.fixedDeltaTime);
            Debug.Log("Rot: " + rigidbody.angularVelocity);
#endif
        }
        attachedWand = null;
        currentlyInteracting = false;
    }

    public bool IsInteracting()
    {
        return currentlyInteracting;
    }

    public WandController GetAttachedWand()
    {
        return attachedWand;
    }

    public Transform GetInteractionPoint()
    {
        return interactionPoint;
    }
}
