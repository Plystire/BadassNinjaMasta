using UnityEngine;
using System.Collections;

public class TestBehavior : MonoBehaviour {

    private bool track = false;

	// Use this for initialization
	void Start () {
	    
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (track)
        {
            Debug.Log("TestB: Update");
            track = false;
        }
    }

    void FixedUpdate()
    {
        if (track)
        {
            Debug.Log("TestB: FixedUpdate");
            track = false;
        }
    }

    void OnTriggerEnter(Collider col)
    {
        Debug.Log("TestB: Trigger entered");
        track = true;
    }
}
