using UnityEngine;
using System.Collections;

public class PlayerTeleportController : MonoBehaviour {

    private bool teleporting = false;

    private Vector3 translateTo;
    private Vector3 beginPos;
    private float translateSpd;
    private float beginTime;

	// Use this for initialization
	void Start () {
        ShadowStepController.OnTeleport += Teleport;
	}
	
	// Update is called once per frame
	void Update () {

        if (teleporting)
        {
            float scalar = (Time.time - beginTime) / translateSpd;
            if (scalar >= 1.0f)
            {   // Finished teleporting
                scalar = 1.0f;
                teleporting = false;
            }
            transform.position = (translateTo - beginPos) * scalar + beginPos;  // Multiply distance to travel by time scalar and add to original position
        }
	}

    private void Teleport(object s, ShadowStepController.TeleportEventArgs e)
    {   // Actually perform the teleport

        Vector3 offset = transform.position - ((ShadowStepController)s).transform.position;

        Vector3 target = e.targPos + offset;
        checkTerrain(ref target);

        // Instant teleport
        //transform.position = target;

        // Translation teleport
        translateTo = target;
        beginPos = transform.position;
        translateSpd = e.spd;
        beginTime = Time.time;

        teleporting = true;
    }

    private void checkTerrain(ref Vector3 pos)
    {
        Terrain terra = GameObject.FindObjectOfType<Terrain>();

        float samp = terra.SampleHeight(pos);

        if (pos.y < samp)
        {
            pos.y = samp;
        }
    }
}
