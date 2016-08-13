using UnityEngine;
using System.Collections;

public class ThrowingStar_WandController : MonoBehaviour {

    private StarBehavior[] stars;
    private Vector3[] starPosOffset;
    private Vector3[] starRotOffset;


    // CONSTANTS

    private const int maxStars = 4;     // 4 stars fit between 5 fingers


	// Use this for initialization
	void Start () {
        // Initialize arrays
        stars = new StarBehavior[maxStars];
        starPosOffset = new Vector3[maxStars];
        starRotOffset = new Vector3[maxStars];

        // Initialize throwing star offsets
        // -- These are hard-coded and represent offsets that match proper hand placement
        // Position Offsets
        starPosOffset[0] = new Vector3(0.0f, -0.05f, -0.022f);
        starPosOffset[1] = new Vector3(0.0f, -0.05f, -0.046f);
        starPosOffset[2] = new Vector3(0.0f, -0.05f, -0.07f);
        starPosOffset[3] = new Vector3(0.0f, -0.05f, -0.094f);
        // Rotation Offsets
        starRotOffset[0] = new Vector3(40.0f, 0.0f, 90.0f);
        starRotOffset[1] = new Vector3(47.5f, 0.0f, 90.0f);
        starRotOffset[2] = new Vector3(55.0f, 0.0f, 90.0f);
        starRotOffset[3] = new Vector3(62.5f, 0.0f, 90.0f);
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    // PUBLICS

    public void addStar(StarBehavior star)
    {
        int ind = getFreeStar();

        if (ind >= 0)
        {
            stars[ind] = star;
            Debug.Log("[addStar] Index: " + ind);
        }
    }

    public void getStarOffsets(StarBehavior star, out Vector3 pos, out Vector3 rot)
    {
        int ind = findStar(star);

        if (ind >= 0)
        {
            pos = starPosOffset[ind];
            rot = starRotOffset[ind];
        } else
        {
            Debug.Log("[getStarOffsets] Couldn't find star");
            pos = new Vector3();
            rot = new Vector3();
        }
    }

    public void removeStar(StarBehavior star)
    {
        int ind = findStar(star);

        if (ind >= 0)
        {
            stars[ind] = null;
            Debug.Log("[removeStar] Index: " + ind);
        }
    }


    // PRIVATES

    private int getFreeStar()
    {   // Find free star index
        for (int i = 0; i < maxStars; i++)
        {
            if (stars[i] == null)
                return i;
        }
        return -1;
    }

    private int findStar(StarBehavior star)
    {
        for (int i = 0; i < maxStars; i++)
        {
            if (stars[i] == star)
                return i;
        }
        return -1;
    }
}
