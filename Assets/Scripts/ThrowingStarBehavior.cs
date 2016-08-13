using UnityEngine;
using System.Collections;

public class ThrowingStarBehavior : MonoBehaviour
{
    private GameObject controller;

    public GameObject heldBy;

    public float lifeSpan = 5.0f;

    private void Start()
    {
    }

    private void Update()
    {
        if (heldBy != null)
        {

        }
        //if(lifeSpan-- <= 0)
        //{
        //    Debug.Log("Destroyed!");
        //    Destroy(gameObject);
        //}
    }

    private void GrabStar(GameObject controller)
    {
        Debug.Log("GrabStar!");
        transform.SetParent(controller.transform);
        
    }
}
