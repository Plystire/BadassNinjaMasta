using UnityEngine;
using System.Collections;

public class CubeBehavior : InteractObject {

    private Renderer renderer;

    public Material defMaterial;
    public Material hitMaterial;

    public float flashStop = 0.15f;
    private float flashStart = -1;

	// Use this for initialization
	new void Start () {
        base.Start();

        renderer = GetComponent<Renderer>();

    }
	
	// Update is called once per frame
	new void Update () {
        base.Update();

        if (flashStart > 0 && flashStop <= Time.time - flashStart)
        {   // Kill the flash
            renderer.material = defMaterial;
            flashStart = -1;
        }
	}

    void OnCollisionEnter(Collider col)
    {
        Debug.Log("CubeHit!");
        renderer.material = hitMaterial;
        flashStart = Time.time;
    }

    public override void OnTriggerDown(WandController wand)
    {
        wand.pickupObject(this, 1, Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger);
    }
}
