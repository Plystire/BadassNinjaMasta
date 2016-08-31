using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class PlyWare_InteractObject : MonoBehaviour
{

    new private Rigidbody rigidbody;

    private bool currentlyInteracting = false;

    private PlyWare_WandController attachedWand;

    private Transform interactionPoint;

    private float velocityFactor = 20000.0f;
    public float GetVelocityFactor() { return velocityFactor; }
    private Vector3 deltaPos;
    private Vector3 lastPos;
    public Vector3 GetDeltaPos() { return deltaPos; }
    private float rotationFactor = 360.0f;
    public float GetRotationFactor() { return rotationFactor; }
    private Quaternion deltaRot;
    private Quaternion lastRot;
    public Quaternion GetDeltaRot() { return deltaRot; }
    private float angle;
    private Vector3 axis;

    // PUBLICS
    //
    // If this object is to act as a network-controlled object
    public bool networkMode = false;
    public string networkID = "";

    [Space(10)]
    [Header("Pickup options")]
    // Can this object be picked up
    public bool canPickup = true;
    // Button to use for picking up this object
    public Valve.VR.EVRButtonId pickupButton;
    // Time in seconds that sticky pickup will occur
    public float stickyPickup = 0.0f;
    // will object lag behind based on mass or will it snap onto interactPoint
    public bool snapPosition = false;
    public bool snapRotation = false;
    // Should we attach to wand joint instead of matching position?
    public enum Joints
    {
        None, Fixed, Spring
    }
    public Joints usingJoint = Joints.Spring;

    [Space(10)]
    [Header("Throwing options")]
    // Max angle from target at which auto-aim will trigger
    public float maxAutoAimAngle = 10.0f;
    // Min velocity at which auto-aim will trigger
    public float minAutoAimVelocity = 5.0f;
    /// <summary>
    /// Velocity multiplier for throwing this object
    /// </summary>
    public float throwingVelocityMultiplier = 1.0f;



    private Joint attachJoint;
    /// <summary>
    /// Can use this to attach/dettach to/from joints
    /// </summary>
    public Joint attachedJoint
    {   // 
        get
        {
            return attachJoint;
        }

        set
        {   
            // Remove from previous attached joint
            if (attachJoint)
            {
                attachJoint.connectedBody = null;
                attachJoint = null;
            }
            if (value)
            {   // Attach to new joint
                if (!rigidbody)
                    rigidbody = GetComponent<Rigidbody>();
                value.connectedBody = rigidbody;
                attachJoint = value;
            }
        }
    }

    /// <summary>
    /// Called when wand has collided with object and pressed down a button.
    /// </summary>
    /// <param name="wand">Wand that has pressed the button</param>
    /// <param name="btn">Button that was pressed</param>
    virtual public void WandButtonDown(PlyWare_WandController wand, Valve.VR.EVRButtonId btn)
    {
        if (canPickup && btn == pickupButton)        // Pickup object when we press our pickup button
            InitPickup(wand, 1, btn);
    }
    /// <summary>
    /// Called when wand has collided with object and released a button.
    /// </summary>
    /// <param name="wand">Wand that is releasing the button</param>
    /// <param name="btn">Button that was released</param>
    virtual public void WandButtonUp(PlyWare_WandController wand, Valve.VR.EVRButtonId btn)
    { }

    /// <summary>
    /// Use this for initialization
    /// </summary>
    public virtual void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        //interactionPoint = new GameObject().transform;
        if (rigidbody)
        {
            rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            velocityFactor /= rigidbody.mass;
            rotationFactor /= rigidbody.mass;
        }
        else
        {
            //Debug.LogError("InteractObject[" + name + "]: Initialized with no Rigidbody");
        }

        if (networkMode && string.IsNullOrEmpty(networkID))
            Debug.LogWarning("Network mode enabled with empty networkID: " + name);

        lastPos = new Vector3();
        lastRot = new Quaternion();
    }

    /// <summary>
    /// Use this to Update things each frame
    /// </summary>
    public virtual void Update()
    {
        // Do update stuff
    }

    /// <summary>
    /// Use this to Update things when physics update
    /// </summary>
    public virtual void FixedUpdate()
    {
        if (currentlyInteracting)
        {
            if (!rigidbody)
                rigidbody = GetComponent<Rigidbody>();
            if (usingJoint == Joints.None)
            {
                if (!snapPosition)
                {   // Update physics to follow attached wand, or attach directly
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
                } else
                {   // Snap position
                    transform.position = interactionPoint.position;
                    if (rigidbody)
                    {   // Reset velocity
                        rigidbody.velocity = Vector3.zero;
                    }

                    // Track our deltas for when we let go
                    deltaPos = transform.position - lastPos;
                    lastPos = transform.position;
                }

                if (!snapRotation)
                {   // Update physics to follow attached wand, or attach directly
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
                } else
                {   // Snap to interactPOint
                    transform.rotation = interactionPoint.rotation;

                    // If we have a rigidbody, ensure our velocities are reset so forces do not accumulate
                    if (rigidbody)
                    {
                        rigidbody.angularVelocity = Vector3.zero;
                    }

                    deltaRot = transform.rotation * Quaternion.Inverse(lastRot);
                    deltaRot.ToAngleAxis(out angle, out axis);

                    lastRot = transform.rotation;
                }
            } else
            {   // When using joints
                switch(usingJoint)
                {
                    case Joints.Fixed:
                        // Track deltas for release of fixed joint
                        deltaPos = transform.position - lastPos;
                        lastPos = transform.position;

                        deltaRot = transform.rotation * Quaternion.Inverse(lastRot);
                        deltaRot.ToAngleAxis(out angle, out axis);
                        lastRot = transform.rotation;
                        break;
                }
            }
        }
    }

    private List<GameObject> destroyList = new List<GameObject>();

    /// <summary>
    /// Destroy obj when this object is destroyed
    /// </summary>
    /// <param name="obj">Object to destroy</param>
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
                }
                catch (Exception) { }  // If we fail, don't worry. It may have been destroyed at some other point
            }
        }
    }

    public virtual void InitNetworkPickup(GameObject wand, int maxCount)
    {
        BeginInteraction(wand);
    }

    public virtual void InitPickup(PlyWare_WandController wand, int maxCount, Valve.VR.EVRButtonId btn)
    {
        wand.pickupObject(this, maxCount, btn);
    }

    public virtual void BeginInteraction(GameObject wand)
    {
        if (wand)
        {
            attachedWand = wand.GetComponent<PlyWare_WandController>();

            // Attach to specified joint
            if (attachedWand)
            {
                switch (usingJoint)
                {
                    case Joints.Fixed:
                        attachedJoint = attachedWand.fixedJoint;
                        //attachedWand.fixedJoint.connectedBody = rigidbody;
                        break;
                    case Joints.Spring:
                        attachedJoint = attachedWand.springJoint;
                        //attachedWand.springJoint.connectedBody = rigidbody;
                        break;
                    default:
                        // When not a joint
                        if (interactionPoint == null)
                            interactionPoint = new GameObject().transform;  // If we lost it, make a new one

                        interactionPoint.position = transform.position;     // Set to our current orientation
                        interactionPoint.rotation = transform.rotation;
                        interactionPoint.SetParent(wand.transform, true);   // Set to follow wand, so we can follow this
                        break;
                }

                // Start timer for sticky pickup
                attachedWand.stickyPickup(stickyPickup);
            }

            currentlyInteracting = true;
        }
    }

    public void SetInteractionPoint(Vector3 pos, Vector3 rot)
    {
        if (pos != null)
            interactionPoint.position = pos;
        if (rot != null)
            interactionPoint.rotation = Quaternion.Euler(rot);
    }

    public void SetInteractionPointLocal(Vector3? pos, Vector3? rot)
    {
        if (pos != null)
            interactionPoint.localPosition = (Vector3)pos;
        if (rot != null)
            interactionPoint.localRotation = Quaternion.Euler((Vector3)rot);
    }

    public virtual void EndInteractionFromNetwork(Vector3 pos, Quaternion rot, Vector3 vel, Vector3 avel)
    {
        Debug.LogWarning("Throwing Network object!!!!!!!!!!!!!!!!!!!");
        Debug.LogWarning("Position: " + pos);
        Debug.LogWarning("Rotation: " + rot);
        Debug.LogWarning("Velocity: " + vel);
        Debug.LogWarning("angularVelocity: " + avel);

        if (!rigidbody)
            rigidbody = GetComponent<Rigidbody>();
        // Apply orientation and velocities from network
        transform.position = pos;
        transform.rotation = rot;
        rigidbody.velocity = vel;
        rigidbody.angularVelocity = avel;

        attachedWand = null;
        currentlyInteracting = false;

        //AutoAim();    // I don't think we need to run AutoAim on receiving client, do we? The other client has already performed autoaim
    }

    public virtual void EndInteraction(GameObject wand)
    {
        bool applyPhysics = true;

        if (!rigidbody)
            rigidbody = GetComponent<Rigidbody>();

        // Dettach from specified joint
        switch (usingJoint)
        {
            case Joints.Fixed:
                attachedWand.fixedJoint.connectedBody = null;
                break;
            case Joints.Spring:
                attachedWand.springJoint.connectedBody = null;
                applyPhysics = false;
                break;
            default:
                // When not a joint
                break;
        }

        if (applyPhysics)
        {   // Apply direct physics to our object based on past values
            rigidbody.velocity = deltaPos * velocityFactor * Time.fixedDeltaTime;
            rigidbody.angularVelocity = (Time.fixedDeltaTime * angle * axis) * rotationFactor;
        }

#if DEBUG
        Debug.Log("EndInteraction:");
        Debug.Log("wandPos: " + attachedWand.transform.position);
        Debug.Log("Pos: " + rigidbody.velocity + " ; delta: " + deltaPos + " ; factor: " + velocityFactor + " ; time: " + Time.fixedDeltaTime);
        Debug.Log("Rot: " + rigidbody.angularVelocity);
#endif

        // Stop interacting
        attachedWand = null;
        currentlyInteracting = false;

        // Apply throwing multiplier
        rigidbody.velocity *= throwingVelocityMultiplier;
        rigidbody.angularVelocity *= throwingVelocityMultiplier;

        AutoAim();

        if (networkMode)
        {   // Raise network event to inform the masses
            Dictionary<string, object> content = new Dictionary<string, object>();
            RaiseEventOptions REO = new RaiseEventOptions();
            content.Add("pos", transform.position);
            content.Add("rot", transform.rotation);
            content.Add("vel", rigidbody.velocity);
            content.Add("avel", rigidbody.angularVelocity);
            content.Add("isRight", wand.name.Contains("right"));
            Debug.LogError("Throwing object!!!!!!!!!!!!!!!!!!!");
            Debug.LogWarning("Position: " + content["pos"]);
            Debug.LogWarning("Rotation: " + content["rot"]);
            Debug.LogWarning("Velocity: " + content["vel"]);
            Debug.LogWarning("angularVelocity: " + content["avel"]);
            NetworkEventManager.RaiseEvent((byte)NetworkEventManager.EventCodes.ReleaseWeapon, content, true, REO);
        }
    }

    private void AutoAim()
    {
        if (maxAutoAimAngle > 0 & minAutoAimVelocity > 0)
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
            tmpVelGO.transform.LookAt(transform.position + rigidbody.velocity);

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
                float mag = rigidbody.velocity.magnitude;

                GameObject go = new GameObject();
                go.name = "go";
                go.transform.position = transform.position;
                float dist = (bestTarget.transform.position - transform.position).magnitude;
                float eta = dist / mag;
                go.transform.LookAt(bestTarget.transform.position);

                rigidbody.velocity = go.transform.forward * mag;
                rigidbody.velocity += -Physics.gravity * (eta / 2.0f);    // Divide by 2 because we only want to counteract gravity for half of our travel time... that is, we reach our peak at half-time, like we should :)

                Destroy(go);
            }
            else
            {
                Debug.Log("Failed auto-aim: " + lowestDiff);
            }
        }
    }

    public bool IsInteracting()
    {
        return currentlyInteracting;
    }

    public PlyWare_WandController GetAttachedWand()
    {
        return attachedWand;
    }

    public Transform GetInteractionPoint()
    {
        return interactionPoint;
    }
}
