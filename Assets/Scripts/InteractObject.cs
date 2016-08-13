using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class InteractObject : MonoBehaviour {

    new private Rigidbody rigidbody;

    private bool currentlyInteracting = false;

    private WandController attachedWand;

    private Transform interactionPoint;
    
    private float velocityFactor = 20000.0f;
    public float GetVelocityFactor() { return velocityFactor; }
    private Vector3 deltaPos;
    private Vector3 lastPos;
    public Vector3 GetDeltaPos() { return deltaPos; }
    private float rotationFactor = 600.0f;
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
	
	// Update is called once per frame
	public void Update () {
	    if (currentlyInteracting)
        {
            if (!snapHold)
            {   // Update physics to follow attached wand
                deltaPos = interactionPoint.position - transform.position;
                rigidbody.velocity = deltaPos * velocityFactor * Time.fixedDeltaTime;

                deltaRot = interactionPoint.rotation * Quaternion.Inverse(transform.rotation);
                deltaRot.ToAngleAxis(out angle, out axis);

                if (angle > 180)
                {
                    angle -= 360;
                }
                else if (angle < -180)    // Only you can prevent wrap jump
                {
                    angle += 360;
                }

                rigidbody.angularVelocity = (Time.fixedDeltaTime * angle * axis) * rotationFactor;
            } else
            {   // Snap to wand
                transform.position = interactionPoint.position;
                transform.rotation = interactionPoint.rotation;

                // If we have a rigidbody, ensure our velocities are reset so forces do not accumulate
                Rigidbody rig = GetComponent<Rigidbody>();
                if (rig)
                {
                    rig.velocity = new Vector3();
                    rig.angularVelocity = new Vector3();
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

    public virtual void BeginInteraction(WandController wand)
    {
        if (wand)
        {
            if(interactionPoint == null)
                interactionPoint = new GameObject().transform;  // If we lost it, make a new one

            attachedWand = wand;
            interactionPoint.position = wand.transform.position;
            interactionPoint.rotation = wand.transform.rotation;
            interactionPoint.SetParent(wand.transform, true);

            currentlyInteracting = true;

            // Start timer for sticky pickup
            wand.stickyPickup(stickyPickup);
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

    public virtual void EndInteraction(WandController wand)
    {
        if (IsInteracting() && snapHold)
        {   // Update physics to follow attached wand when snapHold, otherwise physics won't be applied
            rigidbody.velocity = deltaPos * velocityFactor * Time.fixedDeltaTime;
            rigidbody.angularVelocity = (Time.fixedDeltaTime * angle * axis) * rotationFactor;

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
