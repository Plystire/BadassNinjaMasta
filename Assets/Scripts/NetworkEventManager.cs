﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NetworkEventManager : Photon.PunBehaviour {

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

    public void OnEvent(byte code, object content, int senderId)
    {
        Debug.Log("Received Event! " + code);
        switch((EventCodes)code)
        {
            case EventCodes.SpawnWeapon:
                // Spawn a item attached to GameObject
                foreach(playerRig pr in playerRigs)
                {
                    if (pr.id == senderId)
                    {
                        // Spawn item onto this rig
                        Dictionary<string, object> cont = (Dictionary<string, object>)content;

                        GameObject toSpawn = weaponPrefabs[(byte)cont["spawn"]];
                        bodyPart parent;

                        switch((AttachPoints)cont["attachTo"])
                        {
                            case AttachPoints.RightHand:
                                parent = pr.right;
                                break;
                            case AttachPoints.LeftHand:
                                parent = pr.left;
                                break;
                            default:
                                parent = pr.head;
                                break;
                        }

                        WandController wand = parent.gameObject.GetComponent<WandController>();

                        int count = 1;
                        if (toSpawn.name.Contains("Star"))
                            count = 4;

                        GameObject clone = (GameObject)Instantiate(toSpawn, null, false);
                        InteractObject intObj = clone.GetComponent<InteractObject>();
                        intObj.InitNetworkPickup(parent.gameObject, count);

                        parent.attachments.Add(intObj);

                        break;
                    }
                }
                break;

            case EventCodes.ReleaseWeapon:
                // Force release of weapon in hand given velocities and orientation
                foreach(playerRig pr in playerRigs)
                {
                    if (pr.id == senderId)
                    {
                        Dictionary<string, object> cont = (Dictionary<string, object>)content;
                        bodyPart wand;
                        if ((bool)cont["isRight"])
                        {   // Right hand
                            wand = pr.right;
                        } else
                        {   // Left hand
                            wand = pr.left;
                        }

                        // Grab first attachment to bodyPart and release it
                        if (wand.attachments.Count > 0)
                        {
                            wand.attachments[0].EndInteractionFromNetwork((Vector3)cont["pos"], (Quaternion)cont["rot"], (Vector3)cont["vel"], (Vector3)cont["avel"]);   // No wand
                            wand.attachments.RemoveAt(0);
                        }

                        break;
                    }
                }
                break;
        }
    }

    public static void RaiseEvent(byte eventCode, object eventContent, bool sendReliable, RaiseEventOptions options)
    {
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
    }
}
