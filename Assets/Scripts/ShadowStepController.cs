using UnityEngine;
using System.Collections;

public class ShadowStepController : MonoBehaviour {

    // OnTeleport event setup
    public struct TeleportEventArgs
    {
        public Vector3 targPos;
        public float spd;
    }

    public delegate void TeleportAction(object sender, TeleportEventArgs e);
    public static event TeleportAction OnTeleport;  // To teleport use OnTeleport() when not null
    //

    private WandController wand;

    private bool teleporting = false;
    private Vector2 beginAxis;

    /// <summary>
    /// Time taken to complete transportation
    /// </summary>
    public float speedFactor = 0.5f;

    /// <summary>
    /// Maximum distance can travel per sprint
    /// </summary>
    public float sprintDistance = 8.0f;

	// Use this for initialization
	void Start () {
        wand = GetComponent<WandController>();
        if (wand == null)
            Debug.Log("ShadowStep.wand NULL");
	}
	
	// Update is called once per frame
	void Update () {
        // Check for teleportation process
        if (!teleporting && wand.padTouched)
        {
            Debug.Log("Begin ShadowStep!");
            teleporting = true;
            beginAxis = wand.padAxis;
        }

        if(teleporting && !wand.padTouched)
        {
            // TELEPORT!!!
            TeleportEventArgs e = new TeleportEventArgs();
            Vector2 tAxis = wand.padAxis - beginAxis;   // Get our vector from the player
            float mag = tAxis.magnitude;
            tAxis /= mag > 1.0f ? mag : 1.0f;       // Normalize! (Maximize our scalar to 1.0f magnitude)

            tAxis *= sprintDistance;  // Teleport away!

            // Get speed factor
            e.spd = speedFactor;

            e.targPos = transform.localToWorldMatrix.MultiplyPoint(new Vector3(tAxis.x, 0, tAxis.y));   // Transform our local projection into world space!

            if (OnTeleport != null)
                OnTeleport(this, e);

            teleporting = false;
        }

	}
}
