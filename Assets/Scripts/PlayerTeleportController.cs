using UnityEngine;
using System.Collections;

public class PlayerTeleportController : MonoBehaviour {

	// Use this for initialization
	void Start () {
        ShadowStepController.OnTeleport += Teleport;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    private void Teleport(object s)
    {

    }
}
