using UnityEngine;
using System.Collections;

public class PlayerBodyBehavior : MonoBehaviour {

    public GameObject headObj;      // Camera object for head

	
	void Start ()
    {
        // Latch onto head
        transform.position = headObj.transform.position;
    }
	
	void Update ()
    {

        // Smooth toward headObj
        Vector3 movement = headObj.transform.position - transform.position;
        movement /= 4;
        transform.position += movement;

        // Uhhh... rotation -- right!
        Vector3 localA = transform.localEulerAngles;
        Vector3 targA = headObj.transform.localEulerAngles;     // By utilizing euler angles, we are able to understand what is going on. :P
        targA.y -= localA.y;
        if (targA.y > 180)      // Only you can prevent wrap jump
            targA.y -= 360;
        else if (targA.y < -180)
            targA.y += 360;
        targA.y /= 128.0f;  // Go slow
        targA.x = localA.x;
        targA.y += localA.y;
        targA.z = localA.z;

        transform.rotation = Quaternion.Euler(targA);

    }
}
