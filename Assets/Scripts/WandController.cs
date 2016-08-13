using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class WandController : MonoBehaviour {

    // Trigger stuff
    private Valve.VR.EVRButtonId triggerButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;
    public bool triggerDown { get { return controller.GetPressDown(triggerButton); } }
    public bool triggerUp { get { return controller.GetPressUp(triggerButton); } }
    public bool triggerPressed { get { return controller.GetPress(triggerButton); } }

    // Grip stuff
    private Valve.VR.EVRButtonId gripButton = Valve.VR.EVRButtonId.k_EButton_Grip;
    public bool gripDown { get { return controller.GetPressDown(gripButton); } }
    public bool gripUp { get { return controller.GetPressUp(gripButton); } }
    public bool gripPressed { get { return controller.GetPress(gripButton); } }

    private SteamVR_Controller.Device controller { get { return SteamVR_Controller.Input((int)trackedObj.index); } }
    private SteamVR_TrackedObject trackedObj;

    private Collider wandCollider;

    private List<InteractObject> potentialObjs = new List<InteractObject>();
    private bool currentlyInteracting = false;
    private int maxInteract = 1;
    private List<InteractObject> currentIObj = new List<InteractObject>();
    private Valve.VR.EVRButtonId currentIObjDropButton = Valve.VR.EVRButtonId.k_EButton_Grip;

    private float stickyTime = 0.0f;    // Time in seconds during which wand cannot drop item

    #region System Functions

    // Use this for initialization
    void Start () {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        wandCollider = GetComponent<Collider>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        // Update sticky time
        stickyTime -= Time.deltaTime;

        // Determine if we need to drop our currently interacting item
        if (currentlyInteracting && currentIObj.Count > 0)
        {
            if (controller.GetPressUp(currentIObjDropButton))
            {
                Debug.Log("Drop Interacting Object!");
                dropInteractObject();   // Drop it
            }
        }

        if (triggerDown)
        {
            Debug.Log("TriggerDown Nearest!");
            triggerNearest();
            Debug.Log(potentialObjs.Count);
        }
        if (triggerUp)
        {
            triggerUpNearest();
        }

        if (gripDown)
        {
            gripNearest();
        }
        if (gripUp)
        {
            gripUpNearest();
        }

    }

    #endregion

    #region Listeners

    void OnTriggerEnter(Collider col)
    {
        InteractObject IObj = col.GetComponent<InteractObject>();

        if (IObj && !currentIObj.Contains(IObj))    // Don't try to pickup our pickedup items :P
        {
            Debug.Log("WandTriggerEnter: " + col.name);
            potentialObjs.Add(IObj);
        }
    }

    void OnTriggerExit(Collider col)
    {
        InteractObject IObj = col.GetComponent<InteractObject>();

        if (IObj && potentialObjs.Contains(IObj))
        {
            potentialObjs.Remove(IObj);
        }

        // If we have no more potentials, unsticky
        if (potentialObjs.Count == 0)
            stickyTime = 0f;
    }

    #endregion

    #region Functions

    public InteractObject InteractingObject()
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

    private InteractObject NearestObject()
    {
        // Determine nearest by basic distance
        float minDist = float.MaxValue;
        InteractObject closestObj = null;
        float tDist;
        foreach (InteractObject IObj in potentialObjs)
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

    private void triggerNearest()
    {
        InteractObject IObj = NearestObject();

        if (IObj)
        {
            IObj.OnTriggerDown(this);
        }
    }
    private void triggerUpNearest()
    {
        InteractObject IObj = NearestObject();

        if (IObj)
        {
            IObj.OnTriggerUp(this);
        }
    }

    private void gripNearest()
    {
        InteractObject IObj = NearestObject();

        if (IObj)
        {
            IObj.OnGripDown(this);
        }
    }
    private void gripUpNearest()
    {
        InteractObject IObj = NearestObject();

        if (IObj)
        {
            IObj.OnGripUp(this);
        }
    }

    public int CanInteract(InteractObject IObj)
    {
        if (currentlyInteracting)
        {   // Make sure this new thing matches what we have and if we can  have more
            if (IObj.GetType() == currentIObj[0].GetType())
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

    public void pickupObject(InteractObject IObj, int maxInteractions = 1, Valve.VR.EVRButtonId dropButton = Valve.VR.EVRButtonId.k_EButton_Grip)
    {
        int ind = CanInteract(IObj);
        if (ind < 0)
            return; // Can't interact right now

        currentIObj.Add(IObj);
        currentIObjDropButton = dropButton;

        // Set our maximum interactions of this object type
        this.maxInteract = maxInteractions;

        // Pickup the object
        currentIObj[ind].BeginInteraction(this);
        currentlyInteracting = true;

        Debug.Log("Picking up [" + currentIObj[ind] + " : " + ind + "]");
    }

    public void dropInteractObject()
    {   // Drop first object in list
        if (stickyTime > 0)
        {   // But not if we are in stickyTime-out
            return;
        }
        Debug.Log("[dropInteractObject] Count: " + currentIObj.Count);
        if (currentIObj.Count > 0 && currentIObj[0])    // Not null
        {
            currentIObj[0].EndInteraction(this);   // Drop it
            currentIObj.RemoveAt(0);                // Remove from list

            if (currentIObj.Count == 0)
            {   // Reset defaults
                currentIObjDropButton = Valve.VR.EVRButtonId.k_EButton_Grip;
                maxInteract = 1;
                currentlyInteracting = false;
            }
        }
    }

    public void stickyPickup(float time)
    {   // Defines time within which wand cannot drop interactObject
        stickyTime = time;
    }

    #endregion

}
