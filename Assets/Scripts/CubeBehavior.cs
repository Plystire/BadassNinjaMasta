using UnityEngine;
using System.Collections;

public class CubeBehavior : InteractObject {

    private Renderer cuberenderer;

    public Material defMaterial;
    public Material hitMaterial;

    public float flashStop = 0.15f;
    private float flashStart = -1;

    // Use this for initialization
    public override void Start () {
        base.Start();

        cuberenderer = GetComponent<Renderer>();

    }

    // Update is called once per frame
    public override void Update () {
        base.Update();

        if (flashStart > 0 && flashStop <= Time.time - flashStart)
        {   // Kill the flash
            cuberenderer.material = defMaterial;
            flashStart = -1;
        }
	}

    void OnCollisionEnter(Collider col)
    {
        Debug.Log("CubeHit!");
        cuberenderer.material = hitMaterial;
        flashStart = Time.time;
    }

    public override void OnTriggerDown(WandController wand)
    {
        wand.pickupObject(this, 1, Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger);
    }
}
