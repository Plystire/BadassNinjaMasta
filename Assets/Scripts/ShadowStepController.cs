using UnityEngine;
using System.Collections;

public class ShadowStepController : MonoBehaviour {

    public delegate void TeleportAction(object sender);
    public static event TeleportAction OnTeleport;  // To teleport use OnTeleport() when not null

	// Use this for initialization
	void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
