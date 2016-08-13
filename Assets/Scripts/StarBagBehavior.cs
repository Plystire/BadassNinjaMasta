using UnityEngine;
using System.Collections;

public class StarBagBehavior : InteractObject
{
    public GameObject throwingStarPrefab;
    public float spawnDelay = 1f;

    private float spawnDelayTimer = 0f;
    private SteamVR_ControllerManager controllers;

    new void Start()
    {
        base.Start();

        controllers = FindObjectOfType<SteamVR_ControllerManager>();
        spawnDelayTimer = 0f;
    }

    new void Update()
    {
        base.Update();

        if (spawnDelayTimer > 0)
        {
            spawnDelayTimer -= Time.deltaTime;
        }
    }

    public override void OnTriggerDown(WandController wand)
    {
        Debug.Log("StarBag - OnTriggerDown");
        if (wand.CanInteract(new StarBehavior()) >= 0 && spawnDelayTimer <= 0.0f)
        {
            Debug.Log("GrabStar!!!");
            GameObject newThrowingStar = (GameObject)Instantiate(throwingStarPrefab, wand.transform.position, wand.transform.rotation);
            newThrowingStar.name = "ThrowingStarClone";
            StarBehavior IObj = newThrowingStar.GetComponent<StarBehavior>();
            IObj.Init(wand);

            spawnDelayTimer = spawnDelay;
        }
    }
}
