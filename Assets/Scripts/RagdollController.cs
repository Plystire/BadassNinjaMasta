using UnityEngine;
using System.Collections;

public class RagdollController : MonoBehaviour {

    public Rigidbody head;
    public Transform vRHead;

    public Rigidbody rightHand;
    public Transform vRRightHand;

    public Rigidbody leftHand;
    public Transform vRLeftHand;

    // Use this for initialization
    void Start () {
        // Make ragdoll parts kinematic so they can be moved to controllers
        head.isKinematic = rightHand.isKinematic = leftHand.isKinematic = true;
	}
	
	void FixedUpdate () {
        // Attach ragdoll to living player
        head.transform.position = vRHead.position;
        head.transform.rotation = vRHead.rotation;
        rightHand.transform.position = vRRightHand.position;
        rightHand.transform.rotation = vRRightHand.rotation;
        leftHand.transform.position = vRLeftHand.position;
        leftHand.transform.rotation = vRLeftHand.rotation;
	}
}
