using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class PlyWare_WandController : MonoBehaviour
{
    private bool rightWand = false;
    public bool isRight
    {
        get { return rightWand; }
    }

    // Network Mode
    public bool networkMode = false;
    public bool isMine = false;

    // Ragdoll hand collider
    public Collider ragdollHand;
    private int ragdollEnableDelay = -1;

    // Trigger stuff
    private Valve.VR.EVRButtonId triggerButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;
    private bool trigD = false;
    private bool trigU = false;
    private bool trig = false;
    public bool triggerDown { get { return trigD; } }
    public bool triggerUp { get { return trigU; } }
    public bool triggerPressed { get { return trig; } }

    // Grip stuff
    private Valve.VR.EVRButtonId gripButton = Valve.VR.EVRButtonId.k_EButton_Grip;
    private bool gripD = false;
    private bool gripU = false;
    private bool grip = false;
    public bool gripDown { get { return gripD; } }
    public bool gripUp { get { return gripU; } }
    public bool gripPressed { get { return grip; } }

    // Touchpad stuff
    private Valve.VR.EVRButtonId padButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad;
    private bool padD = false;
    private bool padU = false;
    private bool pad = false;
    private Vector2 padA = new Vector2();
    public bool padDown { get { return padD; } }
    public bool padUp { get { return padU; } }
    public bool padPressed { get { return pad; } }
    public Vector2 padAxis { get { return padA; } }

    private SteamVR_Controller.Device controller { get { return SteamVR_Controller.Input((int)trackedObj.index); } }
    private SteamVR_TrackedObject trackedObj;

    private Collider wandCollider;

    private List<PlyWare_InteractObject> potentialObjs = new List<PlyWare_InteractObject>();
    private bool currentlyInteracting = false;
    private int maxInteract = 1;
    private List<PlyWare_InteractObject> currentIObj = new List<PlyWare_InteractObject>();
    private Valve.VR.EVRButtonId currentIObjDropButton = Valve.VR.EVRButtonId.k_EButton_Grip;

    private float stickyTime = 0.0f;    // Time in seconds during which wand cannot drop item

    // Auto-Pickup vars
    private bool autoPickupTracker = false;
    private Valve.VR.EVRButtonId autoPickupButton;
    private int autoPickupMaxInt = 1;
    private int autoPickupCt = 0;
    public int autoPickupMaxInteractions
    {
        set
        {
            autoPickupMaxInt = value;
        }
    }
    public Valve.VR.EVRButtonId autoPickup
    {
        set
        {
            autoPickupTracker = true;
            autoPickupButton = value;
            autoPickupMaxInt = 1;   // Default max interactions to 1
            if (autoPickupTracker)
                autoPickupCt = 2;       // 2 frames is plenty of time for trigger interactions, and too fast for player to notice ;)
        }
    }

    // PUBLIC ACCESS TO JOINT REFERENCES
    public Joint springJoint;
    public Joint fixedJoint;

    #region System Functions

    // Use this for initialization
    void Start()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        wandCollider = GetComponent<Collider>();

        if (name.Contains("right"))
            rightWand = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!networkMode || isMine)
        {   // Should only be in here if this is our client's instance

            // Update sticky time
            if (stickyTime > 0)
                stickyTime -= Time.deltaTime;

            if (controller.connected)
            {
                // Update controller state
                trigD = controller.GetPressDown(triggerButton);
                trigU = controller.GetPressUp(triggerButton);
                if (trigD)
                    trig = true;
                if (trigU)
                    trig = false;
                gripD = controller.GetPressDown(gripButton);
                gripU = controller.GetPressUp(gripButton);
                if (gripD)
                    grip = true;
                if (gripU)
                    grip = false;
                padD = controller.GetPressDown(padButton);
                padU = controller.GetPressUp(padButton);
                if (padD)
                    pad = true;
                if (padU)
                    pad = false;
                padA = controller.GetAxis(padButton);

                // Determine if we need to drop our currently interacting item
                if (currentlyInteracting && currentIObj.Count > 0)
                {
                    if (controller.GetPressUp(currentIObjDropButton))
                    {
                        //Debug.Log("Drop Interacting Object!");
                        dropInteractObject();   // Drop it
                    }
                }

                // Fire events
                if (trigD)
                    btnDownNearest(triggerButton);
                if (trigU)
                    btnUpNearest(triggerButton);
                if (gripD)
                    btnDownNearest(gripButton);
                if (gripU)
                    btnUpNearest(gripButton);
                if (padD)
                    btnDownNearest(padButton);
                if (padU)
                    btnUpNearest(padButton);
            }
        }

        // Delay enable our ragdoll collider
        if (ragdollEnableDelay >= 0)
        {
            if (--ragdollEnableDelay < 0)
            {
                ragdollHand.enabled = true;
            }
        }
    }

    void FixedUpdate()
    {
        if (autoPickupTracker && autoPickupCt-- <= 0)
        {   // AUTO PICKUP
            autoPickupTracker = false;

            // Auto-Pickup
            PlyWare_InteractObject no = NearestObject();
            if (no)
                pickupObject(no, autoPickupMaxInt, autoPickupButton);

            // DEBUG ONLY
            trackedObj.enabled = true;
        }
    }

    #endregion

    #region Listeners

    void OnTriggerEnter(Collider col)
    {
        PlyWare_InteractObject IObj = col.GetComponent<PlyWare_InteractObject>();

        if (IObj && !currentIObj.Contains(IObj))    // Don't try to pickup our pickedup items :P
        {
            Debug.Log("WandTriggerEnter: " + col.name);
            potentialObjs.Add(IObj);
        }
    }

    void OnTriggerExit(Collider col)
    {
        PlyWare_InteractObject IObj = col.GetComponent<PlyWare_InteractObject>();

        if (IObj && potentialObjs.Contains(IObj))
        {
            Debug.Log("WandTriggerExit: " + col.name);
            potentialObjs.Remove(IObj);
        }

        // If we have no more potentials, unsticky
        if (potentialObjs.Count == 0)
            stickyTime = 0f;
    }

    #endregion

    #region Functions

    public PlyWare_InteractObject InteractingObject()
    {
        if (currentIObj.Count > 0)
            return currentIObj[0];
        else
            return null;
    }
    public bool IsInteracting()
    {
        return currentlyInteracting;
    }

    private PlyWare_InteractObject NearestObject()
    {
        // Determine nearest by basic distance
        float minDist = float.MaxValue;
        PlyWare_InteractObject closestObj = null;
        float tDist;
        foreach (PlyWare_InteractObject IObj in potentialObjs)
        {
            tDist = (IObj.transform.position - transform.position).sqrMagnitude;
            if (tDist < minDist)
            {
                closestObj = IObj;
                minDist = tDist;
            }
        }

        return closestObj;
    }

    private void btnDownNearest(Valve.VR.EVRButtonId btn)
    {
        PlyWare_InteractObject IObj = NearestObject();

        if (IObj)
        {
            IObj.WandButtonDown(this, btn);
        }
    }

    private void btnUpNearest(Valve.VR.EVRButtonId btn)
    {
        PlyWare_InteractObject IObj = NearestObject();

        if (IObj)
        {
            IObj.WandButtonUp(this, btn);
        }
    }

    public int CanInteract(Type objType)
    {
        if (currentlyInteracting)
        {   // Make sure this new thing matches what we have and if we can  have more
            if (currentIObj[0].GetType() == objType)
            {
                if (currentIObj.Count >= maxInteract)
                    return -1; // Too many
            }
            else
            {
                return -1; // Type doesn't match, leave
            }
        }
        return currentIObj.Count;
    }

    public bool pickupObject(PlyWare_InteractObject IObj, int maxInteractions = 1, Valve.VR.EVRButtonId dropButton = Valve.VR.EVRButtonId.k_EButton_Grip)
    {
        int ind = CanInteract(IObj.GetType());
        if (ind < 0)
            return false; // Can't interact right now

        currentIObj.Add(IObj);
        currentIObjDropButton = dropButton;

        // Set our maximum interactions of this object type
        this.maxInteract = maxInteractions;

        // Pickup the object
        currentIObj[ind].BeginInteraction(gameObject);
        currentlyInteracting = true;

        // If we have a ragdoll hand set, disable it for picking things up
        if (ragdollHand)
        {
            ragdollHand.enabled = false;
        }

        //Debug.Log("Picking up [" + currentIObj[ind] + " : " + ind + "]");

        // Network stuff
        if (!networkMode)
        {
            Debug.LogWarning("Pickup Network Event Raised: " + IObj.name);
            RaiseEventOptions REO = new RaiseEventOptions();
            Dictionary<string, object> EMC = new Dictionary<string, object>();  // Event Message Content
            NetworkEventManager.AttachPoints att = NetworkEventManager.AttachPoints.LeftHand;
            if (isRight)
                att = NetworkEventManager.AttachPoints.RightHand;
            EMC.Add("attachTo", att);   // Add our attach point to our EMC
            // Grab relative orientation
            Vector3 relativePos = transform.position - IObj.transform.position;
            Quaternion relativeRot = transform.rotation * Quaternion.Inverse(IObj.transform.rotation);
            EMC.Add("relativeWandPos", relativePos);
            EMC.Add("relativeWandRot", relativeRot);
            EMC.Add("networkID", IObj.networkID);
            EMC.Add("dropBtn", dropButton);
            EMC.Add("maxInt", maxInteractions);
            PlyWare_NetworkEventManager.RaiseEvent((byte)PlyWare_NetworkEventManager.EventCodes.PickupInteractObject, EMC, true, REO);
        }

        return true;
    }

    public void dropInteractObject(bool viaNetwork = false)
    {   // Drop first object in list
        if (stickyTime > 0)
        {   // But not if we are in stickyTime-out
            return;
        }
        //Debug.Log("[dropInteractObject] Count: " + currentIObj.Count);
        if (currentIObj.Count > 0 && currentIObj[0])    // Not null
        {
            currentIObj[0].EndInteraction(gameObject, viaNetwork);   // Drop it
            currentIObj.RemoveAt(0);                // Remove from list

            if (currentIObj.Count == 0)
            {   // Reset defaults
                currentIObjDropButton = Valve.VR.EVRButtonId.k_EButton_Grip;
                maxInteract = 1;
                currentlyInteracting = false;

                // If we have a ragdoll hand collider set, enable it
                if (ragdollHand)
                {   // Delay our drop so we can throw things.  2-5 frames should be fine.
                    ragdollEnableDelay = 5;
                }
            }
        }
    }

    public void stickyPickup(float time)
    {   // Defines time within which wand cannot drop interactObject
        stickyTime = time;
    }

    #endregion

}