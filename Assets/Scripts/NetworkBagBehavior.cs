using UnityEngine;
using System.Collections;
using Photon;

public class NetworkBagBehavior : InteractObject
{
    public string spawnPrefab;
    public float spawnDelay = 1f;

    public int maxGrab = 1;
    public Buttons grabButton = Buttons.Trigger;

    private float spawnDelayTimer = 0f;
    private SteamVR_ControllerManager controllers;

    public enum Buttons
    {
        Trigger, Grip, Either
    }

    new void Start()
    {
        base.Start();

        controllers = FindObjectOfType<SteamVR_ControllerManager>();
        spawnDelayTimer = 0f;
    }

    new void Update()
    {
        base.Update();

        if (spawnDelayTimer > 0)
        {
            spawnDelayTimer -= Time.deltaTime;
        }
    }

    public override void OnTriggerDown(WandController wand)
    {
        if (grabButton != Buttons.Trigger && grabButton != Buttons.Either)
            return;
        //Debug.Log("Bag - OnTriggerDown");
        if (wand.CanInteract(new StarBehavior()) >= 0 && spawnDelayTimer <= 0.0f)
        {
            //Debug.Log("GrabItem!!!");
            GameObject newItem = PhotonNetwork.Instantiate(spawnPrefab, wand.transform.position, wand.transform.rotation, 0);
            newItem.name = spawnPrefab + "Clone";
            InteractObject IObj = newItem.GetComponent<InteractObject>();
            Valve.VR.EVRButtonId btn = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;
            IObj.InitPickup(wand, maxGrab, btn);

            spawnDelayTimer = spawnDelay;
        }
    }

    public override void OnGripDown(WandController wand)
    {
        if (grabButton != Buttons.Grip && grabButton != Buttons.Either)
            return;
        //Debug.Log("Bag - OnTriggerDown");
        if (wand.CanInteract(new StarBehavior()) >= 0 && spawnDelayTimer <= 0.0f)
        {
            //Debug.Log("GrabItem!!!");
            GameObject newItem = PhotonNetwork.Instantiate(spawnPrefab, wand.transform.position, wand.transform.rotation, 0);
            newItem.name = spawnPrefab + "Clone";
            InteractObject IObj = newItem.GetComponent<InteractObject>();
            Valve.VR.EVRButtonId btn = Valve.VR.EVRButtonId.k_EButton_Grip;
            IObj.InitPickup(wand, maxGrab, btn);

            spawnDelayTimer = spawnDelay;
        }
    }
}
