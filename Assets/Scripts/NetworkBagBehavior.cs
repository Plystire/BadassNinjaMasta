using UnityEngine;
using System.Collections;
using Photon;
using System.Collections.Generic;

public class NetworkBagBehavior : InteractObject
{
    public GameObject spawnPrefab;
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
        Valve.VR.EVRButtonId btn = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;
        grabItem(wand, btn);
    }
    
    private void grabItem(WandController wand, Valve.VR.EVRButtonId btn) { 
        if (wand.CanInteract(new StarBehavior()) >= 0 && spawnDelayTimer <= 0.0f)
        {
            //Debug.Log("GrabItem!!!");
            RaiseEventOptions REO = new RaiseEventOptions();
            Dictionary<string, object> content = new Dictionary<string, object>();
            NetworkEventManager.AttachPoints att = NetworkEventManager.AttachPoints.LeftHand;
            if (wand.name.Contains("right"))
                att = NetworkEventManager.AttachPoints.RightHand;
            byte toSpawn = 0;
            if (spawnPrefab.name.Contains("Kunai"))
                toSpawn = 1;
            content.Add("attachTo", att);
            content.Add("spawn", toSpawn);
            content.Add("grabButton", btn);
            NetworkEventManager.RaiseEvent((byte)NetworkEventManager.EventCodes.SpawnWeapon, content, true, REO);

            GameObject newItem = (GameObject)Instantiate(spawnPrefab, wand.transform.position, wand.transform.rotation);
            newItem.name = spawnPrefab + "Clone";
            InteractObject IObj = newItem.GetComponent<InteractObject>();
            IObj.InitPickup(wand, maxGrab, btn);

            spawnDelayTimer = spawnDelay;
        }
    }

    public override void OnGripDown(WandController wand)
    {
        if (grabButton != Buttons.Grip && grabButton != Buttons.Either)
            return;
        Valve.VR.EVRButtonId btn = Valve.VR.EVRButtonId.k_EButton_Grip;
        grabItem(wand, btn);
    }
}
