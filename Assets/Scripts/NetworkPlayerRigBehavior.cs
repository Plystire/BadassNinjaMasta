using UnityEngine;
using System.Collections;

public class NetworkPlayerRigBehavior : Photon.PunBehaviour {

    public GameObject head;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    new public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        PhotonView PV = GetComponent<PhotonView>();
        if (PV && !PV.isMine)
        {
            // Tell the EventManager about this player, their ID and their GamObjects
            SteamVR_ControllerManager CM = GetComponent<SteamVR_ControllerManager>();
            GameObject right = CM.right;
            GameObject left = CM.left;

            NetworkEventManager.checkInPlayer(info.sender.ID, right, left, head);
        }
    }
}
