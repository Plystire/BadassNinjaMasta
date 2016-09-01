using UnityEngine;
using System.Collections;

public class PlyWare_NetworkPlayerRigBehavior : Photon.PunBehaviour {

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
            // Tell the EventManager about this player, their ID and their GameObjects
            SteamVR_ControllerManager CM = GetComponent<SteamVR_ControllerManager>();
            ID = info.sender.ID;
            PlyWare_NetworkEventManager.checkInPlayer(ID, CM.right, CM.left, head);

            // Inform the wand controllers that they are network entities
            PlyWare_WandController[] ret = GetComponentsInChildren<PlyWare_WandController>();
            foreach(PlyWare_WandController wc in ret)
            {
                wc.networkMode = true;
            }
        }
    }

    void OnDestroy()
    {
        // Take this ID off our network list
        PlyWare_NetworkEventManager.checkOutPlayer(ID);
    }
}
