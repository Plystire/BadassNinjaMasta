using UnityEngine;
using System.Collections;

public class StickyCollider : MonoBehaviour {

    public float thresholdVelocity = 0.0f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerEnter(Collider col)
    {
        Rigidbody rig = GetComponentInParent<Rigidbody>();
        if (rig && rig.velocity.magnitude < thresholdVelocity)
            return; // Don't stick if we're not moving fast enough
        CanStickTo cst = col.GetComponent<CanStickTo>();

        Debug.Log("[StickyCollider.OnTriggerEnter] " + cst);
        Debug.Log(rig.velocity.magnitude);

        if (cst)
        {
            // Stick to colliding object
            //
            InteractObject parentInteract = GetComponentInParent<InteractObject>();
            Transform parentTrans = parentInteract.transform;

            if (rig)
            {   // Set kinematic and reset velocities
                rig.velocity = new Vector3();
                rig.angularVelocity = new Vector3();
                rig.isKinematic = true;
                rig.detectCollisions = false;       // DOn't detect collisions anymore
            }

            // Set parent to buffer object so we can retain orientation
            GameObject stickBuffer = new GameObject();
            stickBuffer.transform.SetParent(col.transform);
            parentTrans.SetParent(stickBuffer.transform);
            // Make sure we destroy the buffer when we die
            parentInteract.DestroyObjectOnDestroy(stickBuffer);
        }
    }
}
