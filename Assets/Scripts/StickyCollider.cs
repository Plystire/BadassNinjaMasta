using UnityEngine;
using System.Collections;

public class StickyCollider : MonoBehaviour {

    public float thresholdVelocity = 0.0f;

    private bool canStick = false;

    private bool triggerCheck = false;
    private bool collisionCheck = false;

    private bool isStuck = false;

    private Rigidbody rig;

	// Use this for initialization
	void Start ()
    {
        rig = GetComponentInParent<Rigidbody>();
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerEnter(Collider col)
    {
        if (isStuck)
            return;

        triggerCheck = true;

        Debug.LogError("Sticky Trigger [" + name + "]");

        if (!collisionCheck)
        {   // Flag for can stick
            float rigmag = rig.velocity.magnitude;
            //Debug.Log("[StickyCollider.OnTriggerEnter] VelMag: " + rigmag);
            Debug.Log("Sticky Trigger!! Vel: " + rigmag + "  ; Col.Name: " + col.name);
            if (rig && rigmag < thresholdVelocity)
                return; // Don't stick if we're not moving fast enough
            CanStickTo cst = col.GetComponent<CanStickTo>();
            if (cst)
                canStick = true;

            Debug.Log("CanStick: " + canStick);

            return;
        }

        collisionCheck = triggerCheck = false;

        if (canStick)
        {
            stick(col.transform);
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (isStuck)
            return;
        triggerCheck = false;
        Debug.Log("Sticky TriggerLeave [" + name + "]: " + col.name);
    }

    void OnCollisionEnter(Collision col)
    {
        if (isStuck)
            return;

        collisionCheck = true;
        
        Debug.LogError("Sticky Collision [" + name + "]");

        if (!triggerCheck)
        {   // Flag for can stick
            float rigmag = rig.velocity.magnitude;
            //Debug.Log("[StickyCollider.OnTriggerEnter] VelMag: " + rigmag);
            Debug.Log("Sticky Collision!! Vel: " + rigmag + "  ; Col.Name: " + col.gameObject.name);
            if (rig && rigmag < thresholdVelocity)
                return; // Don't stick if we're not moving fast enough
            CanStickTo cst = col.collider.GetComponent<CanStickTo>();
            Debug.Log("CST: " + cst);
            if (cst)
                canStick = true;

            Debug.Log("CanStick: " + canStick);
            return;
        }

        collisionCheck = triggerCheck = false;

        if (canStick) {
            stick(col.transform);
        }
    }

    void OnCollisionExit(Collision col)
    {
        if (isStuck)
            return;
        collisionCheck = false;
        Debug.Log("Sticky CollisionLeave: " + col.collider.name);
    }

    private void stick(Transform parent)
    {
        // Stick to colliding object
        //
        InteractObject parentInteract = GetComponentInParent<InteractObject>();
        Transform parentTrans = parentInteract.transform;

        isStuck = true;

        if (rig)
        {   // Set kinematic and reset velocities
            rig.velocity = Vector3.zero;
            rig.angularVelocity = Vector3.zero;
            rig.isKinematic = true;
            rig.detectCollisions = false;       // DOn't detect collisions anymore
        }

        // Set parent to buffer object so we can retain orientation
        GameObject stickBuffer = new GameObject();
        stickBuffer.transform.SetParent(parent);
        parentTrans.SetParent(stickBuffer.transform);
        // Make sure we destroy the buffer when we die
        parentInteract.DestroyObjectOnDestroy(stickBuffer);
    }
}
