using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NetworkEventManager : Photon.PunBehaviour {

    public List<GameObject> weaponPrefabs;

    // Player components
    struct playerRig
    {
        public List<Component> comps;
        public int id;
    }
    private List<playerRig> playerRigs = new List<playerRig>();

    // Event codes
    public enum EventCodes
    {
        SpawnWeapon,
        ReleaseWeapon
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

    private void OnEvent(byte code, object content, int senderId)
    {
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
                                parent = (Transform)pr.comps[0];
                                break;
                            case AttachPoints.LeftHand:
                                parent = (Transform)pr.comps[1];
                                break;
                            default:
                                parent = (Transform)pr.comps[2];
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

    new public void OnPhotonInstantiate(PhotonMessageInfo info)
    {   // Track all camera rigs that players instantiate
        List<Component> li = info.photonView.ObservedComponents;
        playerRig pr = new playerRig();
        pr.comps = li;
        pr.id = info.sender.ID;
        playerRigs.Add(pr);
    }
}
