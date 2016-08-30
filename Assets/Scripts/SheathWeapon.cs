using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Valve.VR;

public class SheathWeapon : InteractObject
{
    public enum sheathVector
    {
        XAxis, YAxis, ZAxis
    }

    public Transform bladeTip;
    public Transform sheathTip;
    public float bladeLength = 0.0f;
    public sheathVector sheathingVector;

    public Collider BladeCollider;
    public GameObject Scabbard;

    private List<WandController> triggerWand = new List<WandController>();

    private bool sheathing = true;  // By default we are sheathed and thus "sheathing"
    private bool held = false;      // True when grabbed by wand
    private float curBladeSheathPos = 0.0f; // How far out of the sheath the blade is
    private float lastWandPos = 0.0f;
    private float beginPos = 0.0f;  // Beginning position of sheathing routine. Prevents auto-pop when first sheathing
    private bool canPop = false;    // Set true when weapon can pop from sheath or pop into sheath

    private bool collWithScabbard = true;   // Currently colliding with scabbard

    private GameObject attachWand = null;
    private Rigidbody rig;
    private GameObject stickyCollider = null;

    // Use this for initialization
    public override void Start ()
    {
        base.Start();

        stickyCollider = transform.Find("StickyCollider").gameObject;   // Obtain our sticky collider, so we can turn it on when the blade is picked up
        stickyCollider.SetActive(false);

        rig = GetComponent<Rigidbody>();  // Turn off collision when sheathed
        //rig.detectCollisions = false;

        BladeCollider.enabled = false;  // Turn off blade collider until we are unsheathed
        
        if (bladeLength == 0.0f)
        {
            Debug.LogError("Blade length is not set on: " + name);
        }
    }

    // Update is called once per frame
    public override void Update () {
        base.Update();
        
        if (held)
        {
            if (sheathing)
            {   // Sheathing
                float cap = CaptureWandPos();
                float delta = cap - lastWandPos;
                lastWandPos = cap;
                float n = delta + curBladeSheathPos;    // new pos
                Debug.Log("Sheathing weapon: loc: " + n + " ; delta: " + delta);
                if (!canPop && (n >= 3 && n <= bladeLength - 3))
                {   // Can pop now
                    canPop = true;
                }
                if (n < 0)
                    n = 0;
                if (canPop)
                {
                    if (n <= 0)
                    {   // Pop into sheath
                        held = false;
                    }
                    else if (n >= bladeLength)
                    {   // Pull out the weapon
                        Debug.Log("Weapon out!");
                        sheathing = false;  // We are no longer sheathing, we are wielding!
                        WandController wand = attachWand.GetComponent<WandController>();
                        if (wand)
                        {
                            InitPickup(wand, 1, EVRButtonId.k_EButton_Grip);
                            Transform interactionPoint = GetInteractionPoint();
                            Debug.Log("Moving sword to wand location: " + interactionPoint.position + " ; " + interactionPoint.rotation);
                            transform.position = interactionPoint.position; // Initialize orientation and location
                            transform.rotation = interactionPoint.rotation;

                            rig.isKinematic = false;
                        }
                        else
                            Debug.LogError("WandController null on Unsheath");
                    }
                }

                if (sheathing)  // Update sheathing position
                    SetSheathingPos(n);
            } else
            {   // Wielding

            }
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        
        if (held && !sheathing)
        {   // Check for placing back in scabbard
            //if ((bladeTip.position - sheathTip.position).sqrMagnitude < 0.25f)
            //{   // Distance close enough
            //    // Begin sheathing
            //    beginPos = bladeLength;
            //    WandController wand = attachWand.GetComponent<WandController>();
            //    BeginSheathing(wand);
            //    EndInteraction(attachWand);    // End normal interaction
            //}
        }

        if (sheathing)
        {   // Make sure sword matches location with scabbard
            //transform.position = Scabbard.transform.position;
            Debug.Log("Resetting position for sheath");
            Vector3 lPos = Vector3.zero;
            lPos.z = curBladeSheathPos;
            transform.localPosition = lPos;

            if (held)
            {
                Scabbard.transform.LookAt(attachWand.transform);
            }

            //transform.rotation = Scabbard.transform.rotation;
        }
    }

    void OnCollisionEnter(Collision col)
    {
        Debug.Log("Collision!");
        // If we collide with the scabbard, flag it
        if (col.gameObject == Scabbard)
        {
            Debug.Log("----Scabbard hit!");
            collWithScabbard = true;
        }
    }

    void OnCollisionExit(Collision col)
    {
        Debug.Log("Collision out!");
        if (col.gameObject == Scabbard)
        {
            Debug.Log("---- Scabbard off");
            collWithScabbard = false;
        }
    }

    void OnTriggerEnter(Collider col)
    {
        Debug.Log("Assassin's Blade: TriggerEnter");

        WandController wand = col.GetComponent<WandController>();

        if (wand)
            triggerWand.Add(wand);
    }

    void OnTriggerExit(Collider col)
    {
        Debug.Log("Assassin's Blade: TriggerExit");

        WandController wand = col.GetComponent<WandController>();

        if (wand)
            triggerWand.Remove(wand);
    }

    public override void OnGripDown(WandController wand)
    {
        if (triggerWand.Count > 0)
        {
            Debug.Log("Blade Grip");
            held = true;
            attachWand = wand.gameObject;
            if (sheathing)
            {
                Scabbard.transform.LookAt(attachWand.transform);    // Make scabbard look at wand
                beginPos = 0.0f;
                BeginSheathing(wand);
            }
            else
                InitPickup(wand, 1, EVRButtonId.k_EButton_Grip);
        }
    }

    public override void OnGripUp(WandController wand)
    {
        if (held)
        {
            held = false;
            if (collWithScabbard && !sheathing)
            {   // Snap back into sheath
                sheathing = true;
                curBladeSheathPos = 0;
                rig.isKinematic = true;
                stickyCollider.SetActive(false);
                BladeCollider.enabled = false;
                transform.SetParent(Scabbard.transform);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
            }
        }
        //if (held && sheathing)
        //{
        //    held = false;
        //} else if (held && !sheathing)
        //{
        //    held = false;
        //    //EndInteraction(wand.gameObject);
        //}
    }

    private void BeginSheathing(WandController wand)
    {
        Debug.Log("Begin Sheathing");
        lastWandPos = CaptureWandPos();
        canPop = false; // Recalculate canPop
        curBladeSheathPos = beginPos;

        rig.isKinematic = true;

        transform.SetParent(Scabbard.transform);
        transform.localPosition = Vector3.zero;
    }

    public override void InitPickup(WandController wand, int maxCount, EVRButtonId btn)
    {
        base.InitPickup(wand, maxCount, btn);

        BladeCollider.enabled = true;    // Turn collisions back on!

        stickyCollider.SetActive(true);
    }

    public override void BeginInteraction(GameObject wand)
    {
        base.BeginInteraction(wand);

        // Free from scabbard parent
        transform.SetParent(null);

        SetInteractionPointLocal(new Vector3(0.0f, 0.0f, 0.225f), new Vector3(5, 180, 180));

        Rigidbody rig = GetComponent<Rigidbody>();
        if (rig)
        {
            //rig.isKinematic = true;
        }
    }

    public override void EndInteraction(GameObject wand)
    {
        base.EndInteraction(wand);

        if (sheathing)  // If we're sheathing our blade, disable sticky collision
            stickyCollider.SetActive(false);

        Rigidbody rig = GetComponent<Rigidbody>();
        if (rig)
        {
            //rig.isKinematic = false;
        }
    }

    private float CaptureWandPos()
    {
        switch (sheathingVector)
        {
            case sheathVector.XAxis:
                return Scabbard.transform.worldToLocalMatrix.MultiplyPoint(attachWand.transform.position).x;
            case sheathVector.YAxis:
                return Scabbard.transform.worldToLocalMatrix.MultiplyPoint(attachWand.transform.position).y;
            case sheathVector.ZAxis:
                return Scabbard.transform.worldToLocalMatrix.MultiplyPoint(attachWand.transform.position).z;
            default:
                return 0.0f;
        }
    }

    private void SetSheathingPos(float pos)
    {
        curBladeSheathPos = pos;
        Vector3 n = transform.localPosition;
        switch (sheathingVector)
        {
            case sheathVector.XAxis:
                n.x = pos;
                break;
            case sheathVector.YAxis:
                n.y = pos;
                break; 
            case sheathVector.ZAxis:
                n.z = pos;
                break;
        }
        transform.localPosition = n;
    }
}
