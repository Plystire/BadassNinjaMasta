using UnityEngine;
using System.Collections;
using Photon;

public class RoomBehavior : Photon.PunBehaviour {

	// Use this for initialization
	void Start () {
        PhotonNetwork.autoJoinLobby = true;
        PhotonNetwork.ConnectUsingSettings("0.0.1");
	}
	
	// Update is called once per frame
	void Update () {
        Debug.Log(PhotonNetwork.connectionStateDetailed.ToString());
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
}
