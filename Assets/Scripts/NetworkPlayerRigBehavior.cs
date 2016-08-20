using UnityEngine;
using System.Collections;

public class NetworkPlayerRigBehavior : Photon.PunBehaviour {

    public GameObject head;

    private int ID;

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

            ID = info.sender.ID;
            NetworkEventManager.checkInPlayer(ID, right, left, head);
        }
    }

    void OnDestroy()
    {
        // Take this ID off our network list
        NetworkEventManager.checkOutPlayer(ID);
    }
}
