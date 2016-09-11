using UnityEngine;
using System.Collections;

public class RagdollController : MonoBehaviour {

    public Rigidbody head;
    public Transform vRHead;
    public Vector3 headPositionOffset;
    public Quaternion headRotationOffset;

    public Rigidbody rightHand;
    public Transform vRRightHand;
    public Vector3 rightHandPositionOffset;
    public Quaternion rightHandRotationOffset;

    public Rigidbody leftHand;
    public Transform vRLeftHand;
    public Vector3 leftHandPositionOffset;
    public Quaternion leftHandRotationOffset;

    // Use this for initialization
    void Start () {
        // Make ragdoll parts kinematic so they can be moved to controllers
        head.isKinematic = rightHand.isKinematic = leftHand.isKinematic = true;

        head.transform.SetParent(vRHead, false);
        head.transform.localPosition = headPositionOffset;
        head.transform.localRotation = headRotationOffset;
        rightHand.transform.SetParent(vRRightHand, false);
        rightHand.transform.localPosition = rightHandPositionOffset;
        rightHand.transform.localRotation = rightHandRotationOffset;
        leftHand.transform.SetParent(vRLeftHand, false);
        leftHand.transform.localPosition = leftHandPositionOffset;
        leftHand.transform.localRotation = leftHandRotationOffset;
    }
	
	void FixedUpdate () {
        // Attach ragdoll to living player
        //head.transform.rotation = vRHead.rotation * headRotationOffset;
        //head.transform.position = vRHead.position;
        //head.transform.localPosition += headPositionOffset;

        //rightHand.transform.rotation = vRRightHand.rotation * rightHandRotationOffset;
        //rightHand.transform.position = vRRightHand.position;
        //rightHand.transform.localPosition += rightHandPositionOffset;

        //leftHand.transform.rotation = vRLeftHand.rotation * leftHandRotationOffset;
        //leftHand.transform.position = vRLeftHand.position;
        //leftHand.transform.localPosition += leftHandPositionOffset;
	}
}
