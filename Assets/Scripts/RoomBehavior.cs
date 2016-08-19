using UnityEngine;
using System.Collections;
using Photon;

public class RoomBehavior : Photon.PunBehaviour {

    private GameObject player;

	// Use this for initialization
	void Start () {
        PhotonNetwork.logLevel = PhotonLogLevel.Full;
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

    new public void OnJoinedRoom()
    {
        player = PhotonNetwork.Instantiate("NetworkCameraRig", Vector3.zero, Quaternion.identity, 0);
        // Enable our control scripts for this instance
        enableBehavior(player.GetComponent<SteamVR_ControllerManager>());
        enableBehavior(player.GetComponent<SteamVR_PlayArea>());
        enableBehavior(player.GetComponent<PlayerTeleportController>());
        // And stuff on children
        foreach (ShadowStepController ssc in player.GetComponentsInChildren<ShadowStepController>())
        {
            ssc.enabled = true;
        }
        foreach (WandController ssc in player.GetComponentsInChildren<WandController>())
        {
            ssc.enabled = true;
        }
        //foreach (ThrowingStar_WandController ssc in player.GetComponentsInChildren<ThrowingStar_WandController>())
        //{
        //    ssc.enabled = true;
        //}
        foreach (SteamVR_TrackedObject ssc in player.GetComponentsInChildren<SteamVR_TrackedObject>())
        {
            ssc.enabled = true;
        }
        foreach (SteamVR_Ears ssc in player.GetComponentsInChildren<SteamVR_Ears>())
        {
            ssc.enabled = true;
        }
        foreach (AudioListener ssc in player.GetComponentsInChildren<AudioListener>())
        {
            ssc.enabled = true;
        }
        foreach (Camera ssc in player.GetComponentsInChildren<Camera>())
        {
            ssc.enabled = true;
        }
        foreach (FlareLayer ssc in player.GetComponentsInChildren<FlareLayer>())
        {
            ssc.enabled = true;
        }
        foreach (SteamVR_Camera ssc in player.GetComponentsInChildren<SteamVR_Camera>())
        {
            ssc.enabled = true;
        }
        foreach (GUILayer ssc in player.GetComponentsInChildren<GUILayer>())
        {
            ssc.enabled = true;
        }
        foreach (PlayerBodyBehavior ssc in player.GetComponentsInChildren<PlayerBodyBehavior>())
        {
            ssc.enabled = true;
        }
    }


    void OnPhotonRandomJoinFailed()
    {
        Debug.Log("Can't join random room!");
        PhotonNetwork.CreateRoom(null); // Make new room
    }

}
