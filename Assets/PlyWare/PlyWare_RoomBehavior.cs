using UnityEngine;
using System.Collections;
using Photon;

public class PlyWare_RoomBehavior : Photon.PunBehaviour {

    private GameObject player;

	// Use this for initialization
	void Start () {
        //PhotonNetwork.logLevel = PhotonLogLevel.Full;
        PhotonNetwork.autoJoinLobby = true;
        PhotonNetwork.ConnectUsingSettings("0.0.1");
	}
	
	// Update is called once per frame
	void Update () {
        //Debug.Log(PhotonNetwork.connectionStateDetailed.ToString());
	}

    new public void OnConnectedToMaster()
    {
        if (!PhotonNetwork.autoJoinLobby)
            PhotonNetwork.autoJoinLobby = true;
    }

    new public void OnJoinedLobby()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    private void enableBehavior(Behaviour thing)
    {
        if (thing != null)
            thing.enabled = true;
        else
            Debug.LogWarning("enableBehavior failed to enable: " + thing.GetType());
    }

    private void enableBehavior(Behaviour[] things)
    {
        foreach (Behaviour ssc in things)
        {
            ssc.enabled = true;
        }
    }

    new public void OnJoinedRoom()
    {
        player = PhotonNetwork.Instantiate("BNM_NetworkCameraRig", Vector3.zero, Quaternion.identity, 0);
        // Enable our control scripts for this instance
        enableBehavior(player.GetComponent<SteamVR_ControllerManager>());
        enableBehavior(player.GetComponent<SteamVR_PlayArea>());
        //enableBehavior(player.GetComponent<PlayerTeleportController>());
        // And stuff on children
        //enableBehavior(player.GetComponentsInChildren<PlyWare_WandController>());
        foreach(PlyWare_WandController wand in player.GetComponentsInChildren<PlyWare_WandController>(true))
        {   // Disable networkMode on this instance
            wand.networkMode = false;
        }
        enableBehavior(player.GetComponentsInChildren<SteamVR_TrackedObject>(true));
        enableBehavior(player.GetComponentsInChildren<SteamVR_Ears>(true));
        enableBehavior(player.GetComponentsInChildren<AudioListener>(true));
        enableBehavior(player.GetComponentsInChildren<Camera>(true));
        enableBehavior(player.GetComponentsInChildren<FlareLayer>(true));
        enableBehavior(player.GetComponentsInChildren<SteamVR_Camera>(true));
        enableBehavior(player.GetComponentsInChildren<GUILayer>(true));
    }


    void OnPhotonRandomJoinFailed()
    {
        Debug.Log("Can't join random room!");
        PhotonNetwork.CreateRoom(null); // Make new room
    }

}
