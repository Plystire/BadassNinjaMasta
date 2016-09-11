using UnityEngine;
using System.Collections;
using Photon;
using System.Collections.Generic;
using Valve.VR;

public class PlyWare_InteractObjectSpawnerBehavior : PlyWare_InteractObject
{
    [Space(10)]
    [Header("Spawner Variables")]
    public EVRButtonId spawnButton = EVRButtonId.k_EButton_SteamVR_Trigger;

    public GameObject spawnPrefab;
    public float spawnDelay = 1f;

    public int maxGrab = 1;

    private float spawnDelayTimer = 0f;

    public byte networkSpawnID;

    private Rigidbody rig;

    new void Start()
    {
        base.Start();
        
        spawnDelayTimer = 0f;

        rig = GetComponent<Rigidbody>();
    }

    new void Update()
    {
        base.Update();

        if (spawnDelayTimer > 0)
        {
            spawnDelayTimer -= Time.deltaTime;
        }
    }

    public override void WandButtonDown(PlyWare_WandController wand, EVRButtonId btn)
    {
        if (btn == spawnButton)
        {   // Spawn our item

            // If we're in network mode, tell the network about this
            if (networkMode)
            {
                //PhotonView PV = GetComponent<PhotonView>();
                //PV.RPC("spawnItem", PhotonTargets.All, wand, btn);

                RaiseEventOptions REO = new RaiseEventOptions();
                Dictionary<string, object> content = new Dictionary<string, object>();
                PlyWare_NetworkEventManager.AttachPoints att = PlyWare_NetworkEventManager.AttachPoints.LeftHand;
                if (wand.isRight)
                    att = PlyWare_NetworkEventManager.AttachPoints.RightHand;
                content.Add("attachTo", att);
                content.Add("spawn", networkSpawnID);
                PlyWare_NetworkEventManager.RaiseEvent((byte)PlyWare_NetworkEventManager.EventCodes.SpawnInteractObjectIntoHand, content, true, REO);
            } else
            {
                spawnItem(wand, btn);
            }
        } else if(btn == pickupButton)
        {   // Remove kinematic during pickup
            if (!rig)
                rig = GetComponent<Rigidbody>();

            if (rig)
            {
                rig.isKinematic = false;
            } else
            {
                Debug.LogError("Rigidbody not found on InteractObjectSpawner: " + name);
            }
        }

        base.WandButtonDown(wand, btn); // Make sure we can still pickup our object using pickupButton
    }

    public override void EndInteraction(GameObject wand, bool viaNetwork = false)
    {
        base.EndInteraction(wand, viaNetwork);
    }

    //[PunRPC]
    public virtual void spawnItem(PlyWare_WandController wand, Valve.VR.EVRButtonId btn) {
        if (wand.CanInteract(this.GetType()) >= 0 && spawnDelayTimer <= 0.0f)
        {
            //Debug.Log("GrabItem!!!");

            GameObject newItem = (GameObject)Instantiate(spawnPrefab, wand.transform.position, wand.transform.rotation);
            newItem.name = spawnPrefab + "Clone";
            PlyWare_InteractObject IObj = newItem.GetComponent<PlyWare_InteractObject>();
            IObj.InitPickup(wand, maxGrab, btn);

            spawnDelayTimer = spawnDelay;
        }
    }
}
