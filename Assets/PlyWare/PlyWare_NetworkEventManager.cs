using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlyWare_NetworkEventManager : Photon.PunBehaviour {

    public List<GameObject> weaponPrefabs;

    // Player components
    struct bodyPart
    {
        public GameObject gameObject;
        public List<InteractObject> attachments;
    }
    struct playerRig
    {
        public bodyPart right;
        public bodyPart left;
        public bodyPart head;
        public int id;
    }
    private static List<playerRig> playerRigs = new List<playerRig>();

    // Event codes
    public enum EventCodes
    {
        PickupInteractObject,
        DropInteractObject,
        SpawnInteractObject,
        SpawnWeapon,
        ReleaseWeapon,
        PlayerJoined
    }
    // Attach points
    public enum AttachPoints
    {
        RightHand, LeftHand, Head
    }

    // Use this for initialization
    void Start() {
        PhotonNetwork.OnEventCall += OnEvent;
    }

    // Update is called once per frame
    void Update() {
        
    }

    public void OnEvent(byte code, object contentObj, int senderId)
    {

        Debug.LogWarning("[Received Event]: " + ((EventCodes)code) + " ; " + senderId);

        playerRig daPlr = new playerRig();
        daPlr.id = -1;
        foreach (playerRig pr in playerRigs)
        {
            if (pr.id == senderId)
            {
                daPlr = pr;
                break;
            }
        }
        Debug.Log(daPlr.id);
        if (daPlr.id == -1) // Uhhhh, yeah
            return;

        // Grab our content
        Dictionary<string, object> content = (Dictionary<string, object>)contentObj;

        PlyWare_InteractObject[] IObjs = FindObjectsOfType<PlyWare_InteractObject>();
        PlyWare_InteractObject pickupObj = null;

        switch ((EventCodes)code)
        {
            case EventCodes.PickupInteractObject:
                // Find the object with networkID
                foreach (PlyWare_InteractObject IObj in IObjs)
                {
                    if (IObj.networkID == (string)content["networkID"])
                    {
                        pickupObj = IObj;
                        break;
                    }
                }

                if (pickupObj)
                {   // Pick the object ... up
                    PlyWare_WandController hand = null;

                    switch ((AttachPoints)content["attachTo"])
                    {
                        case AttachPoints.RightHand:
                            hand = daPlr.right.gameObject.GetComponent<PlyWare_WandController>();
                            break;
                        case AttachPoints.LeftHand:
                            hand = daPlr.left.gameObject.GetComponent<PlyWare_WandController>();
                            break;
                        default:
                            hand = daPlr.right.gameObject.GetComponent<PlyWare_WandController>();
                            break;
                    }

                    if (hand)
                    {   // Attach to hand at given orientation
                        //
                        Vector3 relPos = (Vector3)content["relativeWandPos"];
                        Quaternion relRot = (Quaternion)content["relativeWandRot"];

                        hand.transform.position = pickupObj.transform.position + relPos;
                        hand.transform.rotation = relRot * pickupObj.transform.rotation;

                        hand.autoPickup = (Valve.VR.EVRButtonId)content["dropBtn"];
                        hand.autoPickupMaxInteractions = (int)content["maxInt"];
                    }
                }
                break;
            case EventCodes.DropInteractObject:
                // Find and pickup the object with networkID
                foreach (PlyWare_InteractObject IObj in IObjs)
                {
                    if (IObj.networkID == (string)content["networkID"])
                    {
                        pickupObj = IObj;
                        break;
                    }
                }

                if (pickupObj)
                {   // Throw object
                    PlyWare_WandController hand = null;

                    switch ((AttachPoints)content["attachTo"])
                    {
                        case AttachPoints.RightHand:
                            hand = daPlr.right.gameObject.GetComponent<PlyWare_WandController>();
                            break;
                        case AttachPoints.LeftHand:
                            hand = daPlr.left.gameObject.GetComponent<PlyWare_WandController>();
                            break;
                        default:
                            hand = daPlr.right.gameObject.GetComponent<PlyWare_WandController>();
                            break;
                    }

                    if (hand)
                    {   // Throw from given orientation
                        //
                        Vector3 pos = (Vector3)content["pos"];
                        Quaternion rot = (Quaternion)content["rot"];
                        Vector3 vel = (Vector3)content["vel"];
                        Vector3 avel = (Vector3)content["avel"];

                        hand.dropInteractObject(true);

                        Rigidbody rig = pickupObj.GetComponent<Rigidbody>();

                        rig.isKinematic = true; // Turn kinematic on for moving of transform

                        pickupObj.transform.position = pos;
                        pickupObj.transform.rotation = rot;

                        rig.isKinematic = false;

                        if (rig)
                        {   // Apply desired physics
                            rig.velocity = vel;
                            rig.angularVelocity = avel;
                        }
                    }
                }
                break;
            case EventCodes.SpawnWeapon:
                // Spawn a item attached to GameObject
                // Spawn item onto this rig

                GameObject toSpawn = weaponPrefabs[(byte)content["spawn"]];
                bodyPart parent;

                switch((AttachPoints)content["attachTo"])
                {
                    case AttachPoints.RightHand:
                        parent = daPlr.right;
                        break;
                    case AttachPoints.LeftHand:
                        parent = daPlr.left;
                        break;
                    default:
                        parent = daPlr.head;
                        break;
                }

                PlyWare_WandController wand = parent.gameObject.GetComponent<PlyWare_WandController>();

                int count = 1;
                if (toSpawn.name.Contains("Star"))
                    count = 4;

                GameObject clone = (GameObject)Instantiate(toSpawn, null, false);
                InteractObject intObj = clone.GetComponent<InteractObject>();
                intObj.InitNetworkPickup(parent.gameObject, count);

                parent.attachments.Add(intObj);
                break;

            case EventCodes.ReleaseWeapon:
                // Force release of weapon in hand given velocities and orientation
                bodyPart wandBP;
                if ((bool)content["isRight"])
                {   // Right hand
                    wandBP = daPlr.right;
                } else
                {   // Left hand
                    wandBP = daPlr.left;
                }

                // Grab first attachment to bodyPart and release it
                if (wandBP.attachments.Count > 0)
                {
                    wandBP.attachments[0].EndInteractionFromNetwork((Vector3)content["pos"], (Quaternion)content["rot"], (Vector3)content["vel"], (Vector3)content["avel"]);   // No wand
                    wandBP.attachments.RemoveAt(0);
                }
                break;
        }
    }

    public static void RaiseEvent(byte eventCode, object eventContent, bool sendReliable, RaiseEventOptions options)
    {
        Debug.LogWarning("Sending Photon Event: " + (EventCodes)eventCode);
        PhotonNetwork.RaiseEvent(eventCode, eventContent, sendReliable, options);
    }

    public static void checkInPlayer(int ID, GameObject right, GameObject left, GameObject head)
    {   // Track all camera rigs that players instantiate
        playerRig pr = new playerRig();
        pr.right.gameObject = right;
        pr.right.attachments = new List<InteractObject>();
        pr.left.gameObject = left;
        pr.left.attachments = new List<InteractObject>();
        pr.head.gameObject = head;
        pr.head.attachments = new List<InteractObject>();
        pr.id = ID;
        playerRigs.Add(pr);

        Debug.Log("EventMngr: Checked in player");
    }

    public static void checkOutPlayer(int ID)
    {   // Track all camera rigs that players instantiate
        foreach(playerRig pr in playerRigs)
        {
            if (pr.id == ID)
            {
                playerRigs.Remove(pr);
                break;
            }
        }

        Debug.Log("EventMngr: Checked out player");
    }
}
