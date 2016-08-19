using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NetworkEventManager : Photon.PunBehaviour {

    public List<GameObject> weaponPrefabs;

    // Player components
    struct playerRig
    {
        public GameObject right;
        public GameObject left;
        public GameObject head;
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

                        Transform parent;
                        switch((AttachPoints)cont["attachTo"])
                        {
                            case AttachPoints.RightHand:
                                parent = pr.right.transform;
                                break;
                            case AttachPoints.LeftHand:
                                parent = pr.left.transform;
                                break;
                            default:
                                parent = pr.head.transform;
                                break;
                        }

                        WandController wand = parent.GetComponent<WandController>();

                        int count = 1;
                        if (toSpawn.name.Contains("Star"))
                            count = 4;

                        Valve.VR.EVRButtonId btn = (Valve.VR.EVRButtonId)cont["grabButton"];

                        InteractObject clone = (InteractObject)Instantiate(toSpawn, parent, false);
                        clone.InitPickup(wand, count, btn);
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
        pr.right = right;
        pr.left = left;
        pr.head = head;
        pr.id = ID;
        playerRigs.Add(pr);
    }
}
