using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ResetInteract : InteractObject {

	// Use this for initialization
	new void Start () {
        //base.Start();
	}
	
	// Update is called once per frame
	new void Update () {
        //base.Update();
	}

    public override void OnGripDown(WandController wand)
    {
        Debug.Log("Reset!");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
