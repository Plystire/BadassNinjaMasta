using UnityEngine;
using System.Collections;

public class AssassinsBladeController : MonoBehaviour {

    private Transform sword;
    private Transform scabbard;

	// Use this for initialization
	void Start () {
        sword = transform.FindChild("Sword");
        scabbard = transform.FindChild("Scabbard");
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
